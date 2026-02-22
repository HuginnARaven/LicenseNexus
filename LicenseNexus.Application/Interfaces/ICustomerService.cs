using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<Customer?>> GetAllCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<Customer?> AddCustomerAsync(CustomerRequestDto customer);
    Task UpdateCustomerAsync(int id, CustomerRequestDto customer);
    Task DeleteCustomerAsync(int id);
}