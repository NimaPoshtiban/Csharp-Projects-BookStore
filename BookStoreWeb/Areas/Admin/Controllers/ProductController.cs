using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStoreWeb.Areas.Admin.Controllers;
[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _hostEnvironment = hostEnvironment;
    }
    public IActionResult Index()
    {
        return View();
    }


    [HttpGet]
    public async Task<IActionResult> Upsert(int? id)
    {
        ProductViewModel productVM = new()
        {
            Product = new(),
            CategoryList = (await _unitOfWork.Category.GetAllAsync()).Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }
            ),
            CoverTypeList = (await _unitOfWork.CoverType.GetAllAsync()).Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }
            )
        };
        if (id == null || id == 0)
        { // creating product

            return await Task.FromResult<IActionResult>(View(productVM));
        }
        else
        {
            productVM.Product = await _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            return await Task.FromResult<IActionResult>(View(productVM));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(ProductViewModel obj, IFormFile? file)
    {
        if (!ModelState.IsValid)
        {
            return View(obj);
        }

        var rootPath = _hostEnvironment.WebRootPath;
        if (file != null)
        {
            string filename = Guid.NewGuid().ToString();
            var uploads = Path.Combine(rootPath, @"Images\Products");
            var extension = Path.GetExtension(file.FileName);

            if (obj.Product.ImageUrl != null)
            {
                var oldImage = Path.Combine(rootPath, obj.Product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImage))
                {
                    System.IO.File.Delete(oldImage);
                }
            }

            await using (var filestream = new FileStream(Path.Combine(uploads, filename + extension), FileMode.Create))
            {
                await file.CopyToAsync(filestream);
            }

            obj.Product.ImageUrl = @"\Images\Products\" + filename + extension;
        }

        if (obj.Product.Id == 0)
        {
            await _unitOfWork.Product.AddAsync(obj.Product);
            TempData["Success"] = "Product Created Successfully";
        }
        else
        {
            await _unitOfWork.Product.Update(obj.Product);
            TempData["Success"] = "Product Updated Successfully";
        }
        
        await _unitOfWork.SaveAsync();
        return RedirectToAction(nameof(Index));
    }

    
    #region Api Calls

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var productList = await _unitOfWork.Product.GetAllAsync(includeProperties:"Category");
        return Json(new {data=productList});
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int? id)
    {
        var obj = await _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }

        var oldImage = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
        if (System.IO.File.Exists(oldImage))
        {
            System.IO.File.Delete(oldImage);
        }

        _unitOfWork.Product.Remove(obj);
        await _unitOfWork.SaveAsync();

        return Json(new { success = true, message = "Delete Successful" });

    }

    #endregion
}