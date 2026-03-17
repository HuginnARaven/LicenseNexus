using FluentValidation;
using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Exceptions;
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
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    IValidator<ProductRequestDto> productRequestValidator,
    IValidator<ProductPriceRequestDto> productPriceRequestValidator,
    IValidator<ProductPatchFieldsDto> productPatchFieldsDtoValidator
    ): IProductService
{
    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        var res = await productRepository.GetByIdAsync(id);
        if (res == null) return null;
        return MapModelToDto(res);
    }

    public async Task<IEnumerable<ProductModel>> GetAllAsync()
    {
        return await productRepository.GetAllAsync();
    }

    public async Task<PaginatedResult<ProductListItemModel>> GetPaginatedAsync(ProductFilterDto filter)
    {
        return await productRepository.GetPaginatedAsync(
            filter.Page, 
            filter.PageSize, 
            filter.CategoryId, 
            filter.GroupId, 
            filter.VendorId, 
            filter.Search, 
            filter.PriceFrom, 
            filter.PriceTo, 
            filter.IsPromo, 
            filter.Tags);
    }

    public async Task<ProductModel?> AddAsync(ProductRequestDto product)
    {
        await productRequestValidator.ValidateAndThrowAsync(product);
        
        product.IsNew = true;
        var model = await MapDtoToModel(product);
        
        return await productRepository.AddAsync(model);
    }

    public async Task UpdateAsync(int id, ProductRequestDto product)
    {
        await productRequestValidator.ValidateAndThrowAsync(product);
        if (!await productRepository.ExistsAsync(id))
            throw new NotFoundException($"Product with ID {id} not found");
        
        var model = await MapDtoToModel(product);
        model.Id = id;
        
        await productRepository.UpdateAsync(model);
    }

    public async Task PatchAsync(int id, ProductPatchFieldsDto updates)
    {
        await productPatchFieldsDtoValidator.ValidateAndThrowAsync(updates);
        if (!await productRepository.ExistsAsync(id))
            throw new NotFoundException($"Product with ID {id} not found");

        var patchFieldsModel = new ProductPatchFieldsModel
        {
            Sku = updates.Sku,
            Title = updates.Title,
            ShortDescription = updates.ShortDescription,
            QuantityMin = updates.QuantityMin,
            QuantityMax = updates.QuantityMax,
            StartDate = updates.StartDate,
            EndDate = updates.EndDate,
            IsPromo = updates.IsPromo,
            IsTop = updates.IsTop,
            IsNew = updates.IsNew,
            Logo = updates.Logo,
            Author = updates.Author,
        };

        if (updates.UnitMeasureId.HasValue)
        {
            var unitMeasure = await unitMeasureRepository.GetByIdAsync((int)updates.UnitMeasureId);
            patchFieldsModel.UnitMeasureId = unitMeasure!.Id;
            patchFieldsModel.UnitMeasureName = unitMeasure.Name;
        }
        
        if (updates.ProductTypeId.HasValue)
        {
            var productType = await productTypeRepository.GetByIdAsync((int)updates.ProductTypeId);
            patchFieldsModel.ProductTypeId = productType!.Id;
            patchFieldsModel.ProductTypeName = productType.TypeName;
        }

        if (updates.VendorId.HasValue)
        {
            var vendor = await vendorRepository.GetByIdAsync((int)updates.VendorId);
            patchFieldsModel.Vendor = new VendorModel
            {
                Id = vendor!.Id,
                Name = vendor.Name,
                CountryCode = vendor.CountryCode
            };
        }

        if (updates.ProductGroupId.HasValue)
        {
            var group = await productGroupRepository.GetByIdAsync((int)updates.ProductGroupId);
            var category = await categoryRepository.GetByIdAsync(group!.CategoryId);
            patchFieldsModel.Group = new GroupModel
            {
                Id = group.Id,
                Name = group.Name,
                CategoryId = group.CategoryId,
                CategoryName = category?.CategoryName ?? ""
            };
        }

        if (updates.CurrencyId.HasValue)
        {
            var currency = await currencyRepository.GetByIdAsync((int)updates.CurrencyId);
            patchFieldsModel.Currency = new CurrencyModel
            {
                Id = currency!.Id,
                Name = currency.Name,
                LiteralCode = currency.LiteralCode
            };
        }
        
        await productRepository.PatchAsync(id, patchFieldsModel);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await productRepository.ExistsAsync(id))
            throw new NotFoundException($"Product with ID {id} not found");
        await productRepository.DeleteAsync(id);
    }

    public async Task<ProductPrice?> AddProductPrice(int productId, ProductPriceRequestDto priceDto)
    {
        await productPriceRequestValidator.ValidateAndThrowAsync(priceDto);
        if (!await productRepository.ExistsAsync(productId))
            throw new NotFoundException($"Product with ID {productId} not found");
        
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
        await productPriceRequestValidator.ValidateAndThrowAsync(price);
        if (!await productRepository.ExistsPriceAsync(productId, priceId))
            throw new NotFoundException($"Price with ID {priceId} for product {productId} not found");

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
        if (!await productRepository.ExistsPriceAsync(productId, priceId))
            throw new NotFoundException($"Price with ID {priceId} for product {productId} not found");
        await productRepository.DeletePrice(productId, priceId);
    }

    public async Task AddProductTag(int productId, int tagId)
    {
        if (!await productRepository.ExistsAsync(productId))
            throw new NotFoundException($"Product with ID {productId} not found");
        if (!await tagRepository.ExistsAsync(tagId))
            throw new NotFoundException($"Tag with ID {tagId} not found");
        
        await productRepository.AddTag(productId, tagId);
    }

    public async Task DeleteProductTag(int productId, int tagId)
    {
        if (!await productRepository.ExistsAsync(productId))
            throw new NotFoundException($"Product with ID {productId} not found");
        if (!await tagRepository.ExistsAsync(tagId))
            throw new NotFoundException($"Tag with ID {tagId} not found");
        await productRepository.DeleteTag(productId, tagId);
    }

    private ProductResponseDto MapModelToDto(ProductModel model)
    {
        return new ProductResponseDto
        {
            Id = model.Id,
            Sku = model.Sku,
            Title = model.Title,
            IsActive = model.IsActive,
            Tags = model.Tags.Select(t => t.Name).ToList(),
            TypeId = model.Classification.TypeId,
            TypeName = model.Classification.TypeName,
            UnitMeasureId = model.Classification.UnitMeasureId,
            UnitMeasureName = model.Classification.UnitMeasureName,
            Vendor = new ProductVendorDto
            {
                Id = model.Classification.Vendor.Id,
                Name = model.Classification.Vendor.Name,
                CountryCode = model.Classification.Vendor.CountryCode
            },
            Group = new ProductGroupDto
            {
                Id = model.Classification.Group.Id,
                Name = model.Classification.Group.Name,
                CategoryId = model.Classification.Group.CategoryId,
                CategoryName = model.Classification.Group.CategoryName
            },
            Attributes = new ProductAttributesDto
            {
                ShortDescription = model.Attributes.ShortDescription,
                QuantityMin = model.Attributes.QuantityMin,
                QuantityMax = model.Attributes.QuantityMax,
                IsPromo = model.Attributes.IsPromo,
                IsTop = model.Attributes.IsTop,
                IsNew = model.Attributes.IsNew,
                Logo = model.Attributes.Logo,
                StartDate = model.Attributes.StartDate,
                EndDate = model.Attributes.EndDate
            },
            Descriptions = model.Descriptions.Select(d => new ProductDescriptionDto
            {
                Id = d.Id,
                FullText = d.FullText,
                LanguageCode = d.LanguageCode
            }).ToList(),
            Currency = new ProductPriceDto
            {
                Id = model.Currency.Id,
                Price = model.Prices.FirstOrDefault()?.Price ?? 0,
                TermDuration = model.Prices.FirstOrDefault()?.TermDuration,
                BillingPlan = model.Prices.FirstOrDefault()?.BillingPlan,
                Segment = model.Prices.FirstOrDefault()?.Segment,
                CountryCode = model.Prices.FirstOrDefault()?.CountryCode,
                StartDate = model.Prices.FirstOrDefault()?.StartDate ?? DateTime.UtcNow
            },
            Prices = model.Prices.Select(p => new ProductCurrencyDto
            {
                Id = p.Id,
                LiteralCode = model.Currency.LiteralCode,
                Name = model.Currency.Name
            }).ToList()
        };
    }

    private async Task<ProductModel> MapDtoToModel(ProductRequestDto dto)
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