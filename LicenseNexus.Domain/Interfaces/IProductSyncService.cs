using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IProductSyncService
{
    Task UpdateVendorAsync(Vendor vendor, CancellationToken ct);
}