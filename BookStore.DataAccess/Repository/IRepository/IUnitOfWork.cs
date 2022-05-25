﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    ICategoryRepository Category { get; }
    ICoverTypeRepository CoverType { get; }
    IProductRepository Product { get; }
    Task SaveAsync();
}