using LicenseNexus.Domain.Models;

namespace LicenseNexus.Domain.Interfaces;

public interface ICartRepository
{
    Task<IEnumerable<CartItem>> GetCartAsync(int customerId);
    Task AddToCartAsync(int customerId, int productId, int quantity);
    Task RemoveFromCartAsync(int customerId, int productId);
    Task ClearCartAsync(int customerId);
}