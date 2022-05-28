using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreWeb.Areas.Admin.Controllers;
[Area("Admin")]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CompanyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Upsert(int? id)
    {
        var company = new Company();
        if (id == null || id == 0)
        {
            return View(company);
        }
        else
        {
            company = await _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
            return View(company);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(Company obj)
    {
        if (!ModelState.IsValid)
        {
            return View(obj);
        }
        if (obj.Id == 0)
        {
            await _unitOfWork.Company.AddAsync(obj);
            TempData["Success"] = "Company Created Successfully";
        }
        else
        {
            _unitOfWork.Company.Update(obj);
            TempData["Success"] = "Company Updated Successfully";
        }

        await _unitOfWork.SaveAsync();

        return RedirectToAction(nameof(Index));
    }

    #region Api Calls

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companyList = await _unitOfWork.Company.GetAllAsync();
        return Json(new { data = companyList });
    }


    [HttpDelete]
    public async Task<IActionResult> Delete(int? id)
    {
        var obj = await _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error While Deleting" });
        }
        _unitOfWork.Company.Remove(obj);
        await _unitOfWork.SaveAsync();
        return Json(new { success = true, message = "Company deleted Successfully" });
    }
    #endregion
}