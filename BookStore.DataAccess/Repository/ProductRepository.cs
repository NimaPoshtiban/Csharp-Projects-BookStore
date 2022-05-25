using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DataAccess.Repository;

public class ProductRepository : Repository<Product> , IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task Update(Product obj)
    {
        var objFromdb = await  _db.Products.FirstOrDefaultAsync(x => x.Id == obj.Id);
        if (objFromdb != null)
        {
            objFromdb.Title = obj.Title;
            objFromdb.ISBN = obj.ISBN;
            objFromdb.Price50 = obj.Price50;
            objFromdb.Price100 = obj.Price100;
            objFromdb.ListPrice = obj.ListPrice;
            objFromdb.Description = obj.Description;
            objFromdb.Price = obj.Price;
            objFromdb.CategoryId = obj.CategoryId;
            objFromdb.CategoryId = obj.CoverTypeId;
            objFromdb.Author = obj.Author;
            if (obj.ImageUrl != null)
            {
                objFromdb.ImageUrl = obj.ImageUrl;
            }
        }
    }
}