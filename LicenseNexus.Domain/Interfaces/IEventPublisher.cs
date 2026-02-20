using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IEntityUpdatedEvent { }

public record VendorUpdatedEvent(Vendor vendor) : IEntityUpdatedEvent;
public record CategoryUpdatedEvent(Category category) : IEntityUpdatedEvent;

public interface IEventPublisher
{
    ValueTask PublishAsync(IEntityUpdatedEvent entityUpdatedEvent, CancellationToken cancellationToken = default);
}