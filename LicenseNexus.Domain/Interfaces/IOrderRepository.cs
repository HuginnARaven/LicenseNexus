using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IOrderRepository: IBaseRepository<Order>
{
    Task<OrderProduct?> AddOrderProduct(OrderProduct orderProduct);
    Task DeleteOrderProduct(int id);
}