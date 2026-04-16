namespace LicenseNexus.Application.DTOs;

public class PartnerResponseDto
{
    public int Id { get; set; }
    public required string Status { get; set; }
    public string? CountryCode { get; set; }
    public string FullCompanyName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public required DateTime CreatedDate { get; set; }
    public string Author { get; set; } = string.Empty;
    public List<PartnerAddressResponseDto> Addresses { get; set; } = [];
    public List<CustomerResponseDto> Customers { get; set; } = [];
}

public class PartnerAddressResponseDto
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public required string AddressType { get; set; }
    public required string City { get; set; }
    public required string AddressFull { get; set; }
    public string? Region { get; set; }
    public string? ZipCode { get; set; }
}