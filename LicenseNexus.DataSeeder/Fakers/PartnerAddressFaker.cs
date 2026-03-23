using Bogus;
using Bogus.Extensions.UnitedStates;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public class PartnerAddressFaker : Faker<PartnerAddress>
{
    public PartnerAddressFaker() : base("en")
    {
        RuleFor(a => a.AddressType, f => f.PickRandom("Billing", "Shipping", "Office"));
        RuleFor(a => a.City, f => f.Address.City());
        RuleFor(a => a.AddressFull, f => f.Address.FullAddress());
        RuleFor(a => a.Region, f => f.Address.State());
        RuleFor(a => a.ZipCode, f => f.Address.ZipCode());
    }
    
    public List<PartnerAddress> GenerateForPartner(int partnerId, int count = 0)
    {
        if (count <= 0) count = new Random().Next(1, 3);
        var addresses = Generate(count);
        foreach (var address in addresses)
        {
            address.PartnerId = partnerId;
        }
        return addresses;
    }
}