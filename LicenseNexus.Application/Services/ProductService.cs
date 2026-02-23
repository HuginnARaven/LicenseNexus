using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
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

    public async Task<PaginatedResult<ProductModel>> GetPaginatedAsync(ProductFilterDto filter)
    {
        Console.WriteLine("GetPaginatedAsync");
        return await productRepository.GetPaginatedAsync(
            filter.Page, 
            filter.PageSize, 
            filter.CategoryId, 
            filter.GroupId, 
            filter.VendorId, 
            filter.Search, 
            filter.PriceFrom, 
            filter.PriceTo);
    }

    public async Task<ProductModel?> AddAsync(ProductRequestDTO product)
    {
        var model = await MapDtoToModel(product);
        return await productRepository.AddAsync(model);
    }

    public async Task UpdateAsync(int id, ProductRequestDTO product)
    {
        var model = await MapDtoToModel(product);
        model.Id = id;
        await productRepository.UpdateAsync(model);
    }

    public async Task PatchAsync(int id, ProductPatchFields updates)
    {
        await productRepository.PatchAsync(id, updates);
    }

    public async Task DeleteAsync(int id)
    {
        await productRepository.DeleteAsync(id);
    }

    public async Task<ProductPrice?> AddProductPrice(int productId, ProductPriceRequestDto priceDto)
    {
        var price = new ProductPrice
        {
            ProductId = productId,
            Price = priceDto.Price,
            TermDuration = priceDto.TermDuration,
            BillingPlan = priceDto.BillingPlan,
            CountryCode = priceDto.CountryCode,
            Segment = priceDto.Segment,
            StartDate = priceDto.StartDate
        };
        
        return await productRepository.AddPrice(price);
    }

    public async Task UpdateProductPrice(int productId, int priceId, ProductPriceRequestDto price)
    {
        var productPrice = new ProductPrice
        {
            Id = priceId,
            ProductId = productId,
            Price = price.Price,
            TermDuration = price.TermDuration,
            BillingPlan = price.BillingPlan,
            CountryCode = price.CountryCode,
            Segment = price.Segment,
            StartDate = price.StartDate
        };

        await productRepository.UpdatePrice(productPrice);
    }

    public async Task DeleteProductPrice(int productId, int priceId)
    {
        await productRepository.DeletePrice(productId, priceId);
    }

    public async Task AddProductTag(int productId, int tagId)
    {
        await productRepository.AddTag(productId, tagId);
    }

    public async Task DeleteProductTag(int productId, int tagId)
    {
        await productRepository.DeleteTag(productId, tagId);
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
            Tags = new List<TagModel>(),
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