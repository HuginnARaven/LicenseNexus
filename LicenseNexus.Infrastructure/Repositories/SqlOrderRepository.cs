using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class SqlOrderRepository(BaseSqlContext context) : BaseSqlRepository<Order>(context), IOrderRepository
{
    public override async Task UpdateAsync(Order order)
    {
        await _context.Orders.Where(o => o.Id == order.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(o => o.CustomerId, order.CustomerId)
            .SetProperty(o => o.OrderStatusId, order.OrderStatusId)
            .SetProperty(o => o.OrderTotalSum, order.OrderTotalSum)
            .SetProperty(o => o.DocumentNum, order.DocumentNum)
            .SetProperty(o => o.PostingDate, order.PostingDate)
            .SetProperty(o => o.InvoiceRequested, order.InvoiceRequested)
        );
    }

    public async Task<OrderProduct?> AddOrderProduct(OrderProduct orderProduct)
    {
        _context.OrderProducts.Add(orderProduct);
        return await _context.SaveChangesAsync().ContinueWith(t => t.Result > 0 ? orderProduct : null);
    }

    public async Task DeleteOrderProduct(int id)
    {
        await _context.OrderProducts.Where(op => op.Id == id).ExecuteDeleteAsync();
    }
}