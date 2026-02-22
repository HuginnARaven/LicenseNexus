using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class OrderService(IOrderRepository orderRepository, IProductRepository productRepository): IOrderService
{
    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await orderRepository.GetAllAsync();
        return orders.Select(MapOrderToDto);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order == null)
            return null;
        return MapOrderToDto(order);
    }

    public async Task<OrderResponseDto?> AddOrderAsync(OrderRequestDto orderDto)
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
        
        var orderRes = await orderRepository.AddAsync(order);
        if (orderRes == null)
            return null;
        return MapOrderToDto(order);
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

    public async Task<OrderProductResponseDto?> AddOrderProductAsync(OrderProductRequestDto orderProductDto)
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
        
        var orderProdRes = await orderRepository.AddOrderProduct(orderProduct);
        if (orderProdRes == null)
            return null;
        
        return MapOrderProductProductToDto(orderProdRes);
    }

    public async Task DeleteOrderProductAsync(int id)
    {
        await orderRepository.DeleteOrderProduct(id);
    }

    private OrderResponseDto MapOrderToDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderStatusId = order.OrderStatusId,
            OrderTotalSum = order.OrderTotalSum,
            DocumentNum = order.DocumentNum,
            PostingDate = order.PostingDate,
            InvoiceRequested = order.InvoiceRequested,
            OrderProducts = order.OrderProducts.Select(op => new OrderProductResponseDto
            {
                Id = op.Id,
                ProductId = op.ProductId,
                Quantity = op.Quantity,
                CustomerPrice = op.CustomerPrice,
                PartnerPrice = op.PartnerPrice,
                SumTotal = op.SumTotal,
                ChargeType = op.ChargeType,
                TermDuration = op.TermDuration,
                BillingCycle = op.BillingCycle,
                Status = op.Status
            }).ToList()
        };
    }

    private OrderProductResponseDto MapOrderProductProductToDto(OrderProduct op)
    {
        return new OrderProductResponseDto
        {
            Id = op.Id,
            ProductId = op.ProductId,
            Quantity = op.Quantity,
            CustomerPrice = op.CustomerPrice,
            PartnerPrice = op.PartnerPrice,
            SumTotal = op.SumTotal,
            ChargeType = op.ChargeType,
            TermDuration = op.TermDuration,
            BillingCycle = op.BillingCycle,
            Status = op.Status
        };
    }
}