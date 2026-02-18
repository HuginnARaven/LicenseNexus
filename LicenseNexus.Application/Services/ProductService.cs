using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;

namespace LicenseNexus.Application.Services;

public class ProductService(
    IProductRepository productRepository, 
    IVendorRepository vendorRepository, 
    IProductGroupRepository productGroupRepository,
    IProductTypeRepository productTypeRepository,
    IUnitMeasureRepository unitMeasureRepository,
    ICurrencyRepository currencyRepository,
    ICategoryRepository categoryRepository
    ): IProductService
{
    public async Task<ProductModel?> GetByIdAsync(int id)
    {
        return await productRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<ProductModel>> GetAllAsync()
    {
        return await productRepository.GetAllAsync();
    }

    public async Task AddAsync(ProductRequestDTO product)
    {
        var model = await MapDtoToModel(product);
        await productRepository.AddAsync(model);
    }

    public async Task UpdateAsync(int id, ProductRequestDTO product)
    {
        var model = await MapDtoToModel(product);
        model.Id = id;
        await productRepository.UpdateAsync(model);
    }

    public async Task DeleteAsync(int id)
    {
        await productRepository.DeleteAsync(id);
    }

    private async Task<ProductModel> MapDtoToModel(ProductRequestDTO dto)
    {
        var vendor = await vendorRepository.GetByIdAsync(dto.VendorId);
        var group = await productGroupRepository.GetByIdAsync(dto.ProductGroupId);
        var type = await productTypeRepository.GetByIdAsync(dto.ProductTypeId);
        var unitMeasure = await unitMeasureRepository.GetByIdAsync(dto.UnitMeasureId);
        var currency = await currencyRepository.GetByIdAsync(dto.CurrencyId);
        var category = await categoryRepository.GetByIdAsync(group?.CategoryId ?? 0);
        
        return new ProductModel
        {
            Sku = dto.Sku,
            Title = dto.Title,
            IsActive = (group?.IsActive ?? false) && (category?.IsActive ?? false),
            Tags = new List<string>(),
            Classification = new ClassificationModel
            {
                TypeId = dto.ProductTypeId,
                TypeName = type?.TypeName ?? "",
                UnitMeasureId = dto.UnitMeasureId,
                UnitMeasureName = unitMeasure?.Name ?? "",
                Vendor = new VendorModel
                {
                    Id = dto.VendorId,
                    Name = vendor?.Name ?? "",
                    CountryCode = vendor?.CountryCode ?? ""
                },
                Group = new GroupModel
                {
                    Id = dto.ProductGroupId,
                    Name = group?.Name ?? "",
                    CategoryId = group?.CategoryId ?? 0,
                    CategoryName = category?.CategoryName ?? ""
                }
            },
            Attributes = new AttributesModel
            {
                ShortDescription = dto.ShortDescription ?? "",
                QuantityMin = dto.QuantityMin,
                QuantityMax = dto.QuantityMax,
                IsPromo = dto.IsPromo,
                IsTop = dto.IsTop,
                IsNew = dto.IsNew,
                Logo = dto.Logo,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedDate = DateTime.UtcNow,
                Author = dto.Author
            },
            Currency = new CurrencyModel
            {
                Id = dto.CurrencyId,
                LiteralCode = currency?.LiteralCode ?? "",
                Name = currency?.Name ?? ""
            },
            Descriptions = new List<DescriptionModel>(),
            Prices = new List<ProductPriceModel>()
        };
    }
}
