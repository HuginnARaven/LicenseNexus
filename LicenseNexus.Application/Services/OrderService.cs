using FluentValidation;
using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace LicenseNexus.Application.Services;

public class OrderService(
    IOrderRepository orderRepository, 
    IProductRepository productRepository, 
    IValidator<OrderRequestDto> orderValidator, 
    IValidator<OrderProductRequestDto> orderProductValidator
): IOrderService
{
    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await orderRepository.GetAllAsync(
            false,
            o => o.OrderProducts, 
            o => o.Customer!
            );
        return orders.Select(MapOrderToDto);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
    {
        var order = await orderRepository.GetByIdAsync(id, false, o => o.OrderProducts, o => o.Customer!);
        if (order == null)
            return null;
        return MapOrderToDto(order);
    }

    public async Task<OrderResponseDto?> AddOrderAsync(OrderRequestDto orderDto)
    {
        await orderValidator.ValidateAndThrowAsync(orderDto);
        
        var order = new Order
        {
            CustomerId = orderDto.CustomerId,
            OrderStatusId = orderDto.OrderStatusId,
            DocumentNum = GenerateDocumentNumber(),
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
        await orderValidator.ValidateAndThrowAsync(orderDto);

        var order = new Order
        {
            Id = id,
            CustomerId = orderDto.CustomerId,
            OrderStatusId = orderDto.OrderStatusId,
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
        await orderProductValidator.ValidateAndThrowAsync(orderProductDto);

        var price = await productRepository.GetPriceAsync(orderProductDto.ProductId, orderProductDto.PriceId);
        var product = await productRepository.GetByIdAsync(orderProductDto.ProductId);

        if (product == null)
        {
            throw new ValidationException($"Product with ID {orderProductDto.ProductId} not found.");
        }
        
        if (price == null)
        {
            throw new ValidationException($"Price with ID {orderProductDto.ProductId} does not belong to Product with ID {orderProductDto.ProductId}");
        }

        if (product.Attributes.QuantityMax < orderProductDto.Quantity ||
            product.Attributes.QuantityMin > orderProductDto.Quantity)
        {
            throw new ValidationException($"Quantity must be between {product.Attributes.QuantityMin} and {product.Attributes.QuantityMax}.");
        }
        
        var orderProduct = new OrderProduct
        {
            OrderId = orderProductDto.OrderId,
            ProductId = orderProductDto.ProductId,
            Quantity = orderProductDto.Quantity,
            CustomerPrice = orderProductDto.CustomerPrice,
            Status = orderProductDto.Status,
            PartnerPrice = price.Price * (decimal)0.8, //TODO: create field price-coefficient in partner
            SumTotal = orderProductDto.Quantity * price.Price,
            ChargeType = "one_time", //TODO: change for usage of enum
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
    
    private static string GenerateDocumentNumber()
    {
        return string.Create(7, Random.Shared, (span, random) =>
        {
            for (int i = 0; i < 6; i++)
            {
                span[i] = (char)('0' + random.Next(10));
            }
            
            span[6] = (char)('A' + random.Next(26)); 
        });
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