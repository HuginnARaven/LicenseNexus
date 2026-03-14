using Bogus;
using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Models;

namespace LicenseNexus.LoadTests.Helpers
{
    public static class PayloadGenerator
    {
        private static Faker<ProductFilterDto> FilterFaker;
        private static Faker<ProductPatchFieldsDto> PatchFaker;
        private static Faker<ProductRequestDto> PostFaker;
        private static Faker<OrderRequestDto> OrderFaker;
        private static Faker<OrderProductRequestDto> OrderProductFaker;
        
        private static Dictionary<int, ProductModel> ProductsMap = new();
        private static int[] ProductIds = Array.Empty<int>();
        private static int[] VendorIds = Array.Empty<int>();
        private static int[] GroupIds = Array.Empty<int>();
        private static int[] TypeIds = Array.Empty<int>();
        private static int[] UnitMeasureIds = Array.Empty<int>();
        private static int[] CurrencyIds = Array.Empty<int>();
        private static int[] CustomerIds = Array.Empty<int>();
        private static int[] OrderStatusIds = Array.Empty<int>();
        private static string[] SearchTerms = Array.Empty<string>();
        
        private static readonly Random Random = new();
        
        public static void Initialize(
            List<ProductModel> products, int[] vendorIds, int[] groupIds, 
            int[] typeIds, int[] unitMeasureIds, int[] currencyIds,
            int[] customerIds, int[] orderStatusIds, string[] terms)
        {
            ProductsMap = products.ToDictionary(p => p.Id);
            ProductIds = products.Select(p => p.Id).ToArray();
            VendorIds = vendorIds;
            GroupIds = groupIds;
            TypeIds = typeIds;
            UnitMeasureIds = unitMeasureIds;
            CurrencyIds = currencyIds;
            CustomerIds = customerIds;
            OrderStatusIds = orderStatusIds;
            SearchTerms = terms;

            InitializeFakers();
        }

        private static void InitializeFakers()
        {
            FilterFaker = new Faker<ProductFilterDto>()
                .RuleFor(p => p.GroupId, f => GroupIds.Length > 0 ? f.PickRandom(GroupIds) : null)
                .RuleFor(f => f.VendorId, f => f.Random.Bool() && VendorIds.Length > 0 ? f.PickRandom(VendorIds) : null)
                .RuleFor(f => f.Search, f => f.Random.Bool() && SearchTerms.Length > 0 ? f.PickRandom(SearchTerms) : null)
                .RuleFor(f => f.PriceFrom, f => f.Random.Bool() ? f.Random.Double(10, 100) : null)
                .RuleFor(f => f.PriceTo, (f, o) => o.PriceFrom.HasValue ? f.Random.Double(o.PriceFrom.Value, 1000) : null)
                .RuleFor(f => f.Page, f => f.Random.Int(1, 5))
                .RuleFor(f => f.PageSize, f => f.PickRandom(10, 20, 50));

            PatchFaker = new Faker<ProductPatchFieldsDto>()
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
            
            PostFaker = new Faker<ProductRequestDto>()
                .RuleFor(p => p.Sku, f => f.Commerce.Ean13())
                .RuleFor(p => p.Title, f => f.Commerce.ProductName())
                .RuleFor(p => p.ShortDescription, f => f.Lorem.Sentence())
                .RuleFor(p => p.QuantityMin, f => f.Random.Int(1, 10))
                .RuleFor(p => p.QuantityMax, f => f.Random.Int(10, 100))
                .RuleFor(p => p.StartDate, f => f.Date.Past())
                .RuleFor(p => p.EndDate, f => f.Date.Future())
                .RuleFor(p => p.IsPromo, f => f.Random.Bool())
                .RuleFor(p => p.IsTop, f => f.Random.Bool())
                .RuleFor(p => p.IsNew, f => f.Random.Bool())
                .RuleFor(p => p.Logo, f => f.Image.PicsumUrl())
                .RuleFor(p => p.Author, f => f.Name.FullName())
                .RuleFor(p => p.VendorId, f => f.PickRandom(VendorIds))
                .RuleFor(p => p.ProductGroupId, f => f.PickRandom(GroupIds))
                .RuleFor(p => p.ProductTypeId, f => f.PickRandom(TypeIds))
                .RuleFor(p => p.UnitMeasureId, f => f.PickRandom(UnitMeasureIds))
                .RuleFor(p => p.CurrencyId, f => f.PickRandom(CurrencyIds));

            OrderFaker = new Faker<OrderRequestDto>()
                .RuleFor(o => o.CustomerId, f => CustomerIds.Length > 0 ? f.PickRandom(CustomerIds) : 1)
                .RuleFor(o => o.OrderStatusId, f => OrderStatusIds.Length > 0 ? f.PickRandom(OrderStatusIds) : 1)
                .RuleFor(o => o.PostingDate, f => f.Date.Recent())
                .RuleFor(o => o.InvoiceRequested, f => f.Random.Bool());

            OrderProductFaker = new Faker<OrderProductRequestDto>()
                .RuleFor(op => op.ProductId, f => GetRandomProductId())
                .RuleFor(op => op.PriceId, (f, op) => 
                {
                    var product = ProductsMap[op.ProductId];
                    return f.PickRandom(product.Prices).Id;
                })
                .RuleFor(op => op.Quantity, (f, op) => 
                {
                    var product = ProductsMap[op.ProductId];
                    var min = Math.Max(1, product.Attributes.QuantityMin);
                    var max = Math.Max(min, product.Attributes.QuantityMax);
                    return f.Random.Int(min, max);
                })
                .RuleFor(op => op.CustomerPrice, f => f.Finance.Amount(10, 500))
                .RuleFor(op => op.Status, f => f.PickRandom("Pending", "Shipped", "Delivered"));
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

        public static ProductPatchFieldsDto GetRandomPatch() => PatchFaker.Generate();
        public static ProductRequestDto GetRandomNewProduct() => PostFaker.Generate();
        public static ProductFilterDto GetRandomFilter() => FilterFaker.Generate();
        public static double GetRandomDouble() => Random.NextDouble();
        
        public static OrderRequestDto GetRandomOrder() => OrderFaker.Generate();
        
        public static List<OrderProductRequestDto> GetRandomOrderProducts(int count)
        {
             return OrderProductFaker.Generate(count);
        }
        
        public static string GetRandomSearchTerm()
        {
            if (SearchTerms.Length == 0) return "Laptop"; // Fallback
            return SearchTerms[Random.Next(SearchTerms.Length)];
        }
    }
}