using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class SqlOrderRepository: IOrderRepository
{
    private readonly BaseSqlContext _context;

    public SqlOrderRepository(BaseSqlContext context)
    {
        _context = context;
    }
    
    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderProducts)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        var res = await _context.SaveChangesAsync();
        if (res > 0)
            return order;
        return null;
    }

    public Task<IEnumerable<Order>> GetAllAsync()
    {
        return _context.Orders
            .Include(o => o.OrderProducts)
            .Include(o => o.Customer)
            .ToListAsync()
            .ContinueWith(t => (IEnumerable<Order>)t.Result);
    }

    public async Task UpdateAsync(Order order)
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

    public Task DeleteAsync(int id)
    {
        return _context.Orders
            .Where(o => o.Id == id)
            .ExecuteDeleteAsync();
    }

    public Task<OrderProduct?> AddOrderProduct(OrderProduct orderProduct)
    {
        _context.OrderProducts.Add(orderProduct);
        return _context.SaveChangesAsync().ContinueWith(t => t.Result > 0 ? orderProduct : null);
    }
}