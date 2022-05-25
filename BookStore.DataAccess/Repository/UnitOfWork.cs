﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;

namespace BookStore.DataAccess.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        Category = new CategoryRepository(_db);
        CoverType = new CoverTypeRepository(_db);
        Product = new ProductRepository(_db);
    }
    public ICategoryRepository Category { get; private set; }
    public ICoverTypeRepository CoverType { get; private set; }
    public IProductRepository Product { get; private set; }
    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}