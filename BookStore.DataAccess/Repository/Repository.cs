﻿using System.Linq.Expressions;
using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DataAccess.Repository;

public class Repository<T> : IRepository<T>  where T : class
{
    private readonly ApplicationDbContext _db;
    internal DbSet<T> dbSet;
    public Repository(ApplicationDbContext db)
    {
        _db = db;
        dbSet = _db.Set<T>();
    }
    public async Task<T> GetFirstOrDefault(Expression<Func<T, bool>> filter)
    {
        IQueryable<T> query = dbSet;
        query =  dbSet.Where(filter);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        IQueryable<T> query = dbSet;
        return await query.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public void Remove(T entity)
    {
        dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        dbSet.RemoveRange(entities);
    }
}