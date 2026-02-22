using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> AddAsync(Order order);
    Task<IEnumerable<Order>> GetAllAsync();
    Task UpdateAsync(Order order);
    Task DeleteAsync(int id);
    
    Task<OrderProduct?> AddOrderProduct(OrderProduct orderProduct);
}