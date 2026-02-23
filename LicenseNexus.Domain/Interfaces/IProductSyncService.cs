using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IProductSyncService
{
    Task UpdateVendorAsync(Vendor vendor, CancellationToken ct);
    Task UpdateCategoryAsync(Category category, CancellationToken ct);
    Task UpdateGroupAsync(ProductGroup group, CancellationToken ct);
    Task UpdateProductTypeAsync(ProductType productType, CancellationToken ct);
    Task UpdateUnitMeasureAsync(UnitMeasure unitMeasure, CancellationToken ct);
    Task UpdateCurrencyAsync(Currency currency, CancellationToken ct);
    Task UpdateTagAsync(Tag tag, CancellationToken ct);
}