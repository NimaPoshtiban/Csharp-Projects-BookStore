using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreWeb.Controllers;

public class CategoryController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<IActionResult> Index()
    {
        var data = await _unitOfWork.Category.GetAllAsync();
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return await Task.FromResult<IActionResult>(View());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category obj)
    {
        if (obj.Name == obj.DisplayOrder.ToString())
        {
            ModelState.AddModelError("name", "The Display order cannot exactly match the Name");
        }
        if (!ModelState.IsValid)
        {
            return View(obj);
        }
        await _unitOfWork.Category.AddAsync(obj);
        await _unitOfWork.SaveAsync();
        TempData["Success"] = "Category Created Successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var category = await _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category obj)
    {
        if (obj.Name == obj.DisplayOrder.ToString())
        {
            ModelState.AddModelError("name", "The Display order cannot exactly match the Name");
        }
        if (!ModelState.IsValid)
        {
            return View(obj);
        }

        _unitOfWork.Category.Update(obj);
        await _unitOfWork.SaveAsync();
        TempData["Success"] = "Category Updated Successfully";
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var category = await _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePOST(int? id)
    {
        var obj = await _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return NotFound();
        }

        _unitOfWork.Category.Remove(obj);
        await _unitOfWork.SaveAsync();
        TempData["Success"] = "Category Deleted Successfully";
        return RedirectToAction(nameof(Index));
    }
}