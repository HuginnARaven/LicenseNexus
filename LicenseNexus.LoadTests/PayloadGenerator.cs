using Bogus;
using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Models;
using System;
using System.Linq;

namespace LicenseNexus.LoadTests
{
    public static class PayloadGenerator
    {
        private static Faker<ProductFilterDto> FilterFaker;
        private static Faker<ProductPatchFields> PatchFaker;
        private static int[] ProductIds = Array.Empty<int>();
        private static int[] VendorIds = Array.Empty<int>();
        private static int[] GroupIds = Array.Empty<int>();
        private static int[] TypeIds = Array.Empty<int>();
        private static int[] UnitMeasureIds = Array.Empty<int>();
        private static int[] CurrencyIds = Array.Empty<int>();
        private static readonly Random Random = new();
        
        public static void Initialize(
            int[] productIds, int[] vendorIds, int[] groupIds, 
            int[] typeIds, int[] unitMeasureIds, int[] currencyIds)
        {
            ProductIds = productIds;
            VendorIds = vendorIds;
            GroupIds = groupIds;
            TypeIds = typeIds;
            UnitMeasureIds = unitMeasureIds;
            CurrencyIds = currencyIds;

            // Ініціалізуємо Bogus ТІЛЬКИ ПІСЛЯ того, як завантажили реальні ID
            InitializeFakers();
        }

        private static void InitializeFakers()
        {
            FilterFaker = new Faker<ProductFilterDto>()
                .RuleFor(p => p.GroupId, f => GroupIds.Length > 0 ? f.PickRandom(GroupIds) : null)
                .RuleFor(f => f.VendorId, f => f.Random.Bool() && VendorIds.Length > 0 ? f.PickRandom(VendorIds) : null)
                .RuleFor(f => f.Search, f => f.Random.Bool() ? f.Commerce.ProductName() : null)
                .RuleFor(f => f.PriceFrom, f => f.Random.Bool() ? f.Random.Double(10, 100) : null)
                .RuleFor(f => f.PriceTo, (f, o) => o.PriceFrom.HasValue ? f.Random.Double(o.PriceFrom.Value, 1000) : null)
                .RuleFor(f => f.Page, f => f.Random.Int(1, 5))
                .RuleFor(f => f.PageSize, f => f.PickRandom(10, 20, 50));

            PatchFaker = new Faker<ProductPatchFields>()
                .RuleFor(p => p.Sku, f => f.Commerce.Ean13())
                .RuleFor(p => p.Title, f => f.Commerce.ProductName())
                .RuleFor(p => p.ShortDescription, f => f.Lorem.Sentence())
                .RuleFor(p => p.QuantityMin, f => f.Random.Int(1, 10))
                .RuleFor(p => p.QuantityMax, (f, p) => (p.QuantityMin ?? 1) + f.Random.Int(10, 100))
                .RuleFor(p => p.StartDate, f => f.Date.Past())
                .RuleFor(p => p.EndDate, f => f.Date.Future())
                .RuleFor(p => p.IsPromo, f => f.Random.Bool())
                .RuleFor(p => p.IsTop, f => f.Random.Bool())
                .RuleFor(p => p.IsNew, f => f.Random.Bool())
                .RuleFor(p => p.Logo, f => f.Image.PicsumUrl())
                .RuleFor(p => p.Author, f => f.Name.FullName())
                .RuleFor(p => p.VendorId, f => VendorIds.Length > 0 ? f.PickRandom(VendorIds) : null)
                .RuleFor(p => p.ProductGroupId, f => GroupIds.Length > 0 ? f.PickRandom(GroupIds) : null)
                .RuleFor(p => p.ProductTypeId, f => TypeIds.Length > 0 ? f.PickRandom(TypeIds) : null)
                .RuleFor(p => p.UnitMeasureId, f => UnitMeasureIds.Length > 0 ? f.PickRandom(UnitMeasureIds) : null)
                .RuleFor(p => p.CurrencyId, f => CurrencyIds.Length > 0 ? f.PickRandom(CurrencyIds) : null);
        }

        public static int GetRandomProductId()
        {
            if (ProductIds.Length == 0) throw new Exception("ProductIds not initialized!");
            return ProductIds[Random.Next(ProductIds.Length)];
        }
        
        public static int GetRandomVendorId()
        {
            if (VendorIds.Length == 0) throw new Exception("VendorIds not initialized!");
            return VendorIds[Random.Next(VendorIds.Length)];
        }

        public static ProductPatchFields GetRandomPatch() => PatchFaker.Generate();
        public static ProductFilterDto GetRandomFilter() => FilterFaker.Generate();
        public static double GetRandomDouble() => Random.NextDouble();
    }
}