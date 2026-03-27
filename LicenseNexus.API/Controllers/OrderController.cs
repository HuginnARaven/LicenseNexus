using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderRequestDto orderDto)
        {
            if (orderDto == null)
            {
                return BadRequest();
            }

            var createdOrder = await orderService.AddOrderAsync(orderDto);
            if (createdOrder == null)
            {
                return StatusCode(500, "A problem occurred while handling your request.");
            }

            return Ok(createdOrder);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrderRequestDto orderDto)
        {
            if (orderDto == null)
            {
                return BadRequest();
            }

            await orderService.UpdateOrderAsync(id, orderDto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await orderService.DeleteOrderAsync(id);
            return NoContent();
        }
        
        [HttpPost("OrderProduct")]
        public async Task<IActionResult> AddProductToOrder([FromBody] OrderProductRequestDto orderProductDto)
        {
            if (orderProductDto == null)
            {
                return BadRequest();
            }

            var orderProduct = await orderService.AddOrderProductAsync(orderProductDto);
            if (orderProduct == null)
            {
                return NotFound("Product or Price not found.");
            }
            
            return Ok(orderProduct);
        }
        
        [HttpDelete("OrderProduct/{id:int}")]
        public async Task<IActionResult> DeleteOrderProduct(int id)
        {
            await orderService.DeleteOrderProductAsync(id);
            return NoContent();
        }
    }
}
