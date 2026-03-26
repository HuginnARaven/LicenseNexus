using LicenseNexus.DataSeeder.Fakers;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Infrastructure.Data.Contexts;

namespace LicenseNexus.DataSeeder.Seeders;

public class PartnerSeeder
{
    private readonly ExtendedSqlContext _extendedSqlContext;
    private readonly BaseSqlContext _baseSqlContext;

    public PartnerSeeder(ExtendedSqlContext extendedSqlContext, BaseSqlContext baseSqlContext)
    {
        _extendedSqlContext = extendedSqlContext;
        _baseSqlContext = baseSqlContext;
    }

    public async Task SeedAsync(int totalPartnersCount)
    {
        Console.WriteLine($"Starting to seed {totalPartnersCount} partners...");

        var partnerFaker = new PartnerFaker();
        var partnerAddressFaker = new PartnerAddressFaker();
        var customerFaker = new CustomerFaker();
        
        var partners = partnerFaker.Generate(totalPartnersCount);
        await _extendedSqlContext.Partners.AddRangeAsync(partners);
        await _extendedSqlContext.SaveChangesAsync();

        var basePartners = partners.Select(p => new Partner() {
            Status = p.Status,
            CountryCode = p.CountryCode,
            FullCompanyName = p.FullCompanyName,
            RegistrationNumber = p.RegistrationNumber,
            TaxNumber = p.TaxNumber,
            BankAccountNumber = p.BankAccountNumber,
            BankName = p.BankName,
            Phone = p.Phone,
            CreatedDate = p.CreatedDate,
            Author = p.Author
        }).ToList();
        await _baseSqlContext.Partners.AddRangeAsync(basePartners);
        await _baseSqlContext.SaveChangesAsync();
        
        Console.WriteLine($"Saved {partners.Count} partners to SQL.");
        
        for (int i = 0; i < partners.Count; i++)
        {
            var partner = partners[i];
            var basePartner = basePartners[i];
            var addresses = partnerAddressFaker.GenerateForPartner(partner.Id);
            await _extendedSqlContext.PartnerAddresses.AddRangeAsync(addresses);

            var baseAddresses = addresses.Select(a => new PartnerAddress() {
                PartnerId = basePartner.Id,
                AddressType = a.AddressType,
                City = a.City,
                AddressFull = a.AddressFull,
                Region = a.Region,
                ZipCode = a.ZipCode
            }).ToList();
            await _baseSqlContext.PartnerAddresses.AddRangeAsync(baseAddresses);
            
            var customers = customerFaker.GenerateForPartner(partner.Id, new Random().Next(2, 5));
            await _extendedSqlContext.Customers.AddRangeAsync(customers);
            
            var baseCustomers = customers.Select(c => new Customer() {
                PartnerId = basePartner.Id,
                AccountName = c.AccountName,
                Email = c.Email,
                LegalName = c.LegalName,
                City = c.City,
                Region = c.Region,
                ZipCode = c.ZipCode,
                CountryCode = c.CountryCode,
                Status = c.Status,
                CreatedDate = c.CreatedDate
            }).ToList();
            await _baseSqlContext.Customers.AddRangeAsync(baseCustomers);
        }
        await _extendedSqlContext.SaveChangesAsync();
        await _baseSqlContext.SaveChangesAsync();
        Console.WriteLine("Saved partner addresses and customers to SQL.");
        
        Console.WriteLine("Finished seeding partners successfully.");
    }
}
    