using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IEntityUpdatedEvent { }

public record VendorUpdatedEvent(Vendor vendor) : IEntityUpdatedEvent;
public record CategoryUpdatedEvent(Category category) : IEntityUpdatedEvent;
public record GroupUpdatedEvent(ProductGroup group) : IEntityUpdatedEvent;
public record ProductTypeUpdatedEvent(ProductType productType) : IEntityUpdatedEvent;
public record UnitMeasureUpdatedEvent(UnitMeasure unitMeasure) : IEntityUpdatedEvent;
public record CurrencyUpdatedEvent(Currency currency) : IEntityUpdatedEvent;
public record TagUpdatedEvent(Tag tag) : IEntityUpdatedEvent;

public interface IEventPublisher
{
    ValueTask PublishAsync(IEntityUpdatedEvent entityUpdatedEvent, CancellationToken cancellationToken = default);
}