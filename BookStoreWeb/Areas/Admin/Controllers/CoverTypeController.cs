using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreWeb.Areas.Admin.Controllers;
[Area("Admin")]
public class CoverTypeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CoverTypeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<IActionResult> Index()
    {
         var obj = await _unitOfWork.CoverType.GetAllAsync();
         return View(obj);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CoverType obj)
    {
        if (!ModelState.IsValid)
        {
            return View(obj);
        }

        await _unitOfWork.CoverType.AddAsync(obj);
        await _unitOfWork.SaveAsync();
        TempData["Success"] = "Cover Type Created Successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var coverType = await _unitOfWork.CoverType.GetFirstOrDefault(x=>x.Id==id);
        if (coverType == null)
        {
            return NotFound();
        }
        return View(coverType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CoverType obj)
    {
        if (!ModelState.IsValid)
        {
            return View(obj);
        }
        _unitOfWork.CoverType.Update(obj);
        await _unitOfWork.SaveAsync();
        TempData["Success"] = "Cover Type Updated Successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var coverType = await _unitOfWork.CoverType.GetFirstOrDefault(x=>x.Id==id);
        if (coverType == null)
        {
            return NotFound();
        }

        return View(coverType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePOST(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        var obj =await _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);
        if (obj == null)
        {
            return NotFound();
        }
        _unitOfWork.CoverType.Remove(obj);
        await _unitOfWork.SaveAsync();
        TempData["Success"] = "Cover Type Deleted Successfully";
        return RedirectToAction(nameof(Index));
    }
}