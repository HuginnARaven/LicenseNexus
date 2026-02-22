using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponseDto?>> GetAllCustomersAsync();
    Task<CustomerResponseDto?> GetCustomerByIdAsync(int id);
    Task<CustomerResponseDto?> AddCustomerAsync(CustomerRequestDto customer);
    Task UpdateCustomerAsync(int id, CustomerRequestDto customer);
    Task DeleteCustomerAsync(int id);
}