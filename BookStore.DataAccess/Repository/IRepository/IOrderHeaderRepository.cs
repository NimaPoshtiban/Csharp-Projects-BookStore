using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Models;

namespace BookStore.DataAccess.Repository.IRepository;

public interface IOrderHeaderRepository:IRepository<OrderHeader>
{
    void Update(OrderHeader obj);
    Task UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
}