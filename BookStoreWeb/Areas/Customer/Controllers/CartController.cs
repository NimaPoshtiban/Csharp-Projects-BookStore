using System.Security.Claims;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
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
                includeProperties: "Product")
        };
        foreach (var cart in ShoppingCartVM.ListCart)
        {
            cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            ShoppingCartVM.CartTotal += (cart.Count * cart.Price);
        }
        return View(ShoppingCartVM);
    }

    public async Task<IActionResult> Summary()
    {
        //var claimsIdentity = (ClaimsIdentity)User.Identity;
        //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        //ShoppingCartVM = new ShoppingCartViewModel()
        //{
        //    ListCart = await _unitOfWork.ShoppingCart.GetAllAsync(x => x.ApplicationUserId == claim.Value,
        //        includeProperties: "Product")
        //};
        //foreach (var cart in ShoppingCartVM.ListCart)
        //{
        //    cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
        //    ShoppingCartVM.CartTotal += (cart.Count * cart.Price);
        //}
        return View(ShoppingCartVM);
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
        return RedirectToAction(nameof(Index));
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