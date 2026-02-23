using FluentValidation;
using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class CustomerService(
    ICustomerRepository repository,
    IValidator<CustomerRequestDto> validator
    ): ICustomerService
{
    public async Task<IEnumerable<CustomerResponseDto?>> GetAllCustomersAsync()
    {
        var customers = await repository.GetAllAsync();
        return customers.Select(MapCustomerToDto);
    }

    public async Task<CustomerResponseDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await repository.GetByIdAsync(id);
        if (customer == null)
            return null;
        return MapCustomerToDto(customer);
    }

    public async Task<CustomerResponseDto?> AddCustomerAsync(CustomerRequestDto customerDto)
    {
        await validator.ValidateAndThrowAsync(customerDto);
        
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
        
        var createdCustomer = await repository.AddAsync(customer);
        if (createdCustomer == null)
            return null;

        return MapCustomerToDto(createdCustomer);
    }

    public async Task UpdateCustomerAsync(int id, CustomerRequestDto customerDto)
    {
        await validator.ValidateAndThrowAsync(customerDto);

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
    
    private CustomerResponseDto MapCustomerToDto(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            PartnerId = customer.PartnerId,
            AccountName = customer.AccountName,
            Email = customer.Email,
            LegalName = customer.LegalName,
            City = customer.City,
            Region = customer.Region,
            ZipCode = customer.ZipCode,
            CountryCode = customer.CountryCode
        };
    }

}