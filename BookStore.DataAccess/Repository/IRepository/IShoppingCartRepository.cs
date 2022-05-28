using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Models;

namespace BookStore.DataAccess.Repository.IRepository;

public interface IShoppingCartRepository:IRepository<ShoppingCart>
{
    int IncrementCount(ShoppingCart cart, int count);
    int DecrementCount(ShoppingCart cart, int count);
}