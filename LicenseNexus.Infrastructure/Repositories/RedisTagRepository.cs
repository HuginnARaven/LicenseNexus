using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisTagRepository(ExtendedSqlContext context): ITagRepository //TODO: implement caching
{
    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await context.Tags.ToListAsync();
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        return await context.Tags.FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.Tags.AnyAsync(pt => pt.Id == id, cancellationToken);
    }


    public async Task<Tag?> AddAsync(Tag tag)
    {
        await context.Tags.AddAsync(tag);
        var res = await context.SaveChangesAsync();
        if (res > 0)
            return tag;
        return null;
    }

    public async Task UpdateAsync(Tag tag)
    {
        context.Tags.Update(tag);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var tag = await context.Tags.FindAsync(id);
        if (tag != null)
        {
            context.Tags.Remove(tag);
            await context.SaveChangesAsync();
        }
    }
}