using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models.ViewModels;


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

    public async Task<IActionResult> Details(int id)
    {
        var cartObj = new ShoppingCart
        {
            Product = await _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, "Category,CoverType"),
            Count = 1
        };
        return View(cartObj);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}