using System.Security.Claims;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using Stripe.Checkout;


namespace BookStoreWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
    public ShoppingCartViewModel ShoppingCartVM { get; set; }
    public int OrderTotal { get; set; }

    public CartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<IActionResult> Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVM = new ShoppingCartViewModel()
        {
            ListCart = await _unitOfWork.ShoppingCart.GetAllAsync(x => x.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
            OrderHeader = new()
        };
        foreach (var cart in ShoppingCartVM.ListCart)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Price);
        }
        return View(ShoppingCartVM);
    }

    public async Task<IActionResult> Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVM = new ShoppingCartViewModel()
        {
            ListCart = await _unitOfWork.ShoppingCart.GetAllAsync(x => x.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
            OrderHeader = new()
        };
        ShoppingCartVM.OrderHeader.ApplicationUser =
            await _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);

        ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
        ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
        ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
        ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
        ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
        ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

        foreach (var cart in ShoppingCartVM.ListCart)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Price);
        }
        return View(ShoppingCartVM);
    }
    [HttpPost]
    [ActionName(nameof(Summary))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SummaryPOST()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVM.ListCart = await _unitOfWork.ShoppingCart.GetAllAsync(x => x.ApplicationUserId == claim.Value,
            includeProperties: "Product");

        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
        ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
        ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
        ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

        foreach (var cart in ShoppingCartVM.ListCart)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Price);
        }

        await _unitOfWork.OrderHeader.AddAsync(ShoppingCartVM.OrderHeader);
        await _unitOfWork.SaveAsync();

        foreach (var cart in ShoppingCartVM.ListCart)
        {
            OrderDetail orderDetail = new()
            {
                ProductId = cart.ProductId,
                OrderId = ShoppingCartVM.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count
            };
            await _unitOfWork.OrderDetail.AddAsync(orderDetail);
            await _unitOfWork.SaveAsync();
        }

        #region Stripe Settings

        var domain = @"https://localhost:7110";

        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>(),

            Mode = "payment",
            SuccessUrl = $"{domain}/Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
            CancelUrl = $"{domain}/Customer/Cart/Index"
        };

        foreach (var item in ShoppingCartVM.ListCart)
        {

            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Title,
                    },

                },
                Quantity = item.Count,
            };
            options.LineItems.Add(sessionLineItem);
        }

        var service = new SessionService();
        Session session = service.Create(options);

        await _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id,
            session.PaymentIntentId);

        await _unitOfWork.SaveAsync();

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);

        #endregion
    }

    public async Task<IActionResult> Plus(int cartId)
    {
        var cart = await _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
        _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
        await _unitOfWork.SaveAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Minus(int cartId)
    {
        var cart = await _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
        if (cart.Count <= 1)
        {
            _unitOfWork.ShoppingCart.Remove(cart);
            var count = (await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == cart.ApplicationUserId)).ToList().Count - 1;
            HttpContext.Session.SetInt32(SD.SessionCart, count);
        }
        else
        {
            _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
        }

        await _unitOfWork.SaveAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Remove(int cartId)
    {
        var cart = await _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
        _unitOfWork.ShoppingCart.Remove(cart);
        await _unitOfWork.SaveAsync();
        var count = (await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == cart.ApplicationUserId)).ToList().Count;
        HttpContext.Session.SetInt32(SD.SessionCart,count);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> OrderConfirmation(int id)
    {
        var orderHeader = await _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id);
        var service = new SessionService();
        Session session = service.Get(orderHeader.SessionId);

        // check stripe status
        if (session.PaymentStatus.ToLower() == "paid")
        {
            await _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
            await _unitOfWork.SaveAsync();
        }

        var shoppingCarts = (await _unitOfWork.ShoppingCart.GetAllAsync(u=>u.ApplicationUserId==orderHeader.ApplicationUserId)).ToList();
        HttpContext.Session.Clear();
        _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
        await _unitOfWork.SaveAsync();

        return View(id);
    }

    private double GetPriceBasedOnQuantity(int quantity, double price, double price50, double price100)
    {
        if (quantity <= 50)
        {
            return price;
        }
        else
        {
            if (quantity <= 100)
            {
                return price50;
            }
            else
            {
                return price100;
            }
        }
    }
}