using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
    Task<OrderResponseDto?> GetOrderByIdAsync(int id);
    Task<OrderResponseDto?> AddOrderAsync(OrderRequestDto orderDto);
    Task UpdateOrderAsync(int id, OrderRequestDto orderDto);
    Task DeleteOrderAsync(int id);
    Task<OrderProductResponseDto?> AddOrderProductAsync(OrderProductRequestDto orderProductDto);
    Task DeleteOrderProductAsync(int id);
}