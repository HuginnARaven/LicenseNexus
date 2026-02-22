using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderByIdAsync(int id);
    Task<Order?> AddOrderAsync(OrderRequestDto orderDto);
    Task UpdateOrderAsync(int id, OrderRequestDto orderDto);
    Task DeleteOrderAsync(int id);
    Task<OrderProduct?> AddOrderProductAsync(OrderProductRequestDto orderProductDto);
}