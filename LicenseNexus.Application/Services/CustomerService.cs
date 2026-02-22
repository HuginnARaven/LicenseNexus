using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class CustomerService(ICustomerRepository repository): ICustomerService
{
    public async Task<IEnumerable<Customer?>> GetAllCustomersAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<Customer?> AddCustomerAsync(CustomerRequestDto customerDto)
    {
        var customer = new Customer
        {
            PartnerId = customerDto.PartnerId,
            AccountName = customerDto.AccountName,
            Email = customerDto.Email,
            LegalName = customerDto.LegalName,
            City = customerDto.City,
            Region = customerDto.Region,
            ZipCode = customerDto.ZipCode,
            CountryCode = customerDto.CountryCode,
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        
        return await repository.AddAsync(customer);;
    }

    public async Task UpdateCustomerAsync(int id, CustomerRequestDto customerDto)
    {
        var customer = new Customer
        {
            Id = id,
            PartnerId = customerDto.PartnerId,
            AccountName = customerDto.AccountName,
            Email = customerDto.Email,
            LegalName = customerDto.LegalName,
            City = customerDto.City,
            Region = customerDto.Region,
            ZipCode = customerDto.ZipCode,
            CountryCode = customerDto.CountryCode,
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        await repository.UpdateAsync(customer);
    }

    public async Task DeleteCustomerAsync(int id)
    {
        await repository.DeleteAsync(id);

    }
}