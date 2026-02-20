using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Data.Contexts;

public class RedisContext
{
    private readonly ConnectionMultiplexer _redis;
        
    public RedisContext(IConfiguration configuration)
    {
        var connectionString = configuration["RedisSettings:ConnectionString"];
        _redis = ConnectionMultiplexer.Connect(connectionString!);
    }
    
    public IDatabase Database => _redis.GetDatabase();
    
    public IServer Server => _redis.GetServer(_redis.GetEndPoints().First());
}