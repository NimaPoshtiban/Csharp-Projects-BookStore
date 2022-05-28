using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;


namespace BookStoreWeb.Areas.Customer.Controllers;
[Area("Customer")]
public class HomeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;


    public HomeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var productList = await  _unitOfWork.Product.GetAllAsync(includeProperties:"Category,CoverType");
        return View(productList);
    }

    public async Task<IActionResult> Details(int productId)
    {
        var cartObj = new ShoppingCart
        {   
            ProductId = productId,
            Product = await _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, "Category,CoverType"),
            Count = 1
        };
        return View(cartObj);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Details(ShoppingCart shoppingCart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        shoppingCart.ApplicationUserId = claim.Value;

        ShoppingCart cartFromDb = await _unitOfWork.ShoppingCart.GetFirstOrDefault(x=>x.ApplicationUserId==claim.Value && x.ProductId==shoppingCart.ProductId);
        if (cartFromDb==null)
        {
            await _unitOfWork.ShoppingCart.AddAsync(shoppingCart);
        }
        else
        {
            _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
        }
       
        await _unitOfWork.SaveAsync();
        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}