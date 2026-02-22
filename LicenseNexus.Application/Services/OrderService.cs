using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class OrderService(IOrderRepository orderRepository, IProductRepository productRepository): IOrderService
{
    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await orderRepository.GetAllAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await orderRepository.GetByIdAsync(id);
    }

    public async Task<Order?> AddOrderAsync(OrderRequestDto orderDto)
    {
        var order = new Order
        {
            CustomerId = orderDto.CustomerId,
            OrderStatusId = orderDto.OrderStatusId,
            DocumentNum = orderDto.DocumentNum,
            PostingDate = orderDto.PostingDate,
            InvoiceRequested = orderDto.InvoiceRequested,
            OrderTotalSum = 0
        };

        return await orderRepository.AddAsync(order);
    }

    public async Task UpdateOrderAsync(int id, OrderRequestDto orderDto)
    {
        var order = new Order
        {
            Id = id,
            CustomerId = orderDto.CustomerId,
            OrderStatusId = orderDto.OrderStatusId,
            DocumentNum = orderDto.DocumentNum,
            PostingDate = orderDto.PostingDate,
            InvoiceRequested = orderDto.InvoiceRequested,
        };

        await orderRepository.UpdateAsync(order);
    }

    public async Task DeleteOrderAsync(int id)
    {
        await orderRepository.DeleteAsync(id);

    }

    public async Task<OrderProduct?> AddOrderProductAsync(OrderProductRequestDto orderProductDto)
    {
        var price = await productRepository.GetPriceAsync(orderProductDto.ProductId, orderProductDto.PriceId);
        if (price == null)
            return  null;
        
        var orderProduct = new OrderProduct
        {
            OrderId = orderProductDto.OrderId,
            ProductId = orderProductDto.ProductId,
            Quantity = orderProductDto.Quantity, //TODO: Validate Product.maxQuantity > Quantity > Product.minQuantity
            CustomerPrice = orderProductDto.CustomerPrice,
            Status = orderProductDto.Status,
            PartnerPrice = price.Price,
            SumTotal = orderProductDto.Quantity * price.Price,
            ChargeType = price.BillingPlan, //TODO: change for smth else
            TermDuration = price.TermDuration,
            BillingCycle = price.BillingPlan
        };
        
        return await orderRepository.AddOrderProduct(orderProduct);
    }
}