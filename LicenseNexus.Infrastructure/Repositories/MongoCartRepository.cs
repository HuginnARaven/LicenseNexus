using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoEntities;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoCartRepository: ICartRepository
{
    private readonly IMongoCollection<CartDocument> _carts;

    public MongoCartRepository(MongoContext context)
    {
        _carts = context.Carts;
    }
    
    public async Task<IEnumerable<CartItem>> GetCartAsync(int customerId)
    {
        var filter = Builders<CartDocument>.Filter.Eq(c => c.UserId, customerId);
        var cart = await _carts.Find(filter).FirstOrDefaultAsync();
        
        if (cart == null)
        {
            return Enumerable.Empty<CartItem>();
        }

        return cart.Items.Select(i => new CartItem
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity
        });
    }

    public async Task AddToCartAsync(int customerId, int productId, int quantity)
    {
        var filter = Builders<CartDocument>.Filter.And(
            Builders<CartDocument>.Filter.Eq(c => c.UserId, customerId),
            Builders<CartDocument>.Filter.ElemMatch(c => c.Items, i => i.ProductId == productId));

        var update = Builders<CartDocument>.Update
            .Inc("items.$.quantity", quantity);

        var result = await _carts.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0)
        {
            var pushFilter = Builders<CartDocument>.Filter.Eq(c => c.UserId, customerId);
            var pushUpdate = Builders<CartDocument>.Update
                .Push(c => c.Items, new CartItemDoc { ProductId = productId, Quantity = quantity });
            
            await _carts.UpdateOneAsync(pushFilter, pushUpdate, new UpdateOptions { IsUpsert = true });
        }
    }

    public async Task RemoveFromCartAsync(int customerId, int productId)
    {
        var filter = Builders<CartDocument>.Filter.Eq(c => c.UserId, customerId);
        var update = Builders<CartDocument>.Update
            .PullFilter(c => c.Items, i => i.ProductId == productId);

        await _carts.UpdateOneAsync(filter, update);
    }

    public async Task ClearCartAsync(int customerId)
    {
        var filter = Builders<CartDocument>.Filter.Eq(c => c.UserId, customerId);
        await _carts.DeleteOneAsync(filter);
    }
}