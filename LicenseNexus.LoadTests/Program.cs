using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using NBomber.Http;
using LicenseNexus.LoadTests;
using System.Text.Json;
using System.Text;
using NBomber.Contracts.Stats;

using var httpClient = new HttpClient();

// Configuration
var baseUrl = "http://localhost:5000"; 
var warmUpDuration = TimeSpan.FromSeconds(30);
var loadDuration = TimeSpan.FromMinutes(2);
var concurrentUsers = 100;

var productIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/product");
var vendorIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/vendor");
var groupIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/productgroup");
var typeIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/producttype");
var unitMeasureIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/unitmeasure");
var currencyIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/currency");

PayloadGenerator.Initialize(productIds, vendorIds, groupIds, typeIds, unitMeasureIds, currencyIds);

async Task<int[]> FetchIdsAsync(HttpClient client, string url)
{
    try
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error loading from {url}: {response.StatusCode}");
            return Array.Empty<int>();
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(jsonString);
        
        return document.RootElement
            .EnumerateArray()
            .Select(x => x.GetProperty("id").GetInt32())
            .ToArray();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Parsing error {url}: {ex.Message}");
        return Array.Empty<int>();
    }
}

// 1. Read-Heavy Scenario (Product Search & Pagination)
var readScenario = Scenario.Create("read_heavy_scenario", async context =>
{
    var filter = PayloadGenerator.GetRandomFilter();
    
    // Construct query string manually
    var queryParams = new List<string>();
    if (filter.CategoryId.HasValue) queryParams.Add($"CategoryId={filter.CategoryId}");
    if (filter.GroupId.HasValue) queryParams.Add($"GroupId={filter.GroupId}");
    if (filter.VendorId.HasValue) queryParams.Add($"VendorId={filter.VendorId}");
    if (!string.IsNullOrEmpty(filter.Search)) queryParams.Add($"Search={Uri.EscapeDataString(filter.Search)}");
    if (filter.PriceFrom.HasValue) queryParams.Add($"PriceFrom={filter.PriceFrom}");
    if (filter.PriceTo.HasValue) queryParams.Add($"PriceTo={filter.PriceTo}");
    queryParams.Add($"Page={filter.Page}");
    queryParams.Add($"PageSize={filter.PageSize}");
    
    var queryString = string.Join("&", queryParams);
    var url = $"{baseUrl}/api/product/catalog?{queryString}";

    var request = Http.CreateRequest("GET", url)
        .WithHeader("Accept", "application/json");

    return await Http.Send(httpClient, request);
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.RampingConstant(copies: concurrentUsers, during: warmUpDuration),
    Simulation.KeepConstant(copies: concurrentUsers, during: loadDuration)
);

// 2. Write-Heavy Scenario (Product Partial Updates)
var writeScenario = Scenario.Create("write_heavy_scenario", async context =>
{
    var productId = PayloadGenerator.GetRandomProductId();
    var patchPayload = PayloadGenerator.GetRandomPatch();
    var url = $"{baseUrl}/api/product/{productId}";

    var jsonPayload = JsonSerializer.Serialize(patchPayload);
    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

    var request = Http.CreateRequest("PUT", url)
        .WithHeader("Accept", "application/json")
        .WithBody(content);

    return await Http.Send(httpClient, request);
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.RampingConstant(copies: concurrentUsers, during: warmUpDuration),
    Simulation.KeepConstant(copies: concurrentUsers, during: loadDuration)
);

// 3. Mixed Scenario (80% Read, 20% Write)
var mixedScenario = Scenario.Create("mixed_scenario", async context =>
{
    var randomValue = PayloadGenerator.GetRandomDouble();
    
    if (randomValue < 0.8) // 80% Read
    {
        var filter = PayloadGenerator.GetRandomFilter();
        var queryParams = new List<string>();
        if (filter.CategoryId.HasValue) queryParams.Add($"CategoryId={filter.CategoryId}");
        if (filter.GroupId.HasValue) queryParams.Add($"GroupId={filter.GroupId}");
        if (filter.VendorId.HasValue) queryParams.Add($"VendorId={filter.VendorId}");
        if (!string.IsNullOrEmpty(filter.Search)) queryParams.Add($"Search={Uri.EscapeDataString(filter.Search)}");
        if (filter.PriceFrom.HasValue) queryParams.Add($"PriceFrom={filter.PriceFrom}");
        if (filter.PriceTo.HasValue) queryParams.Add($"PriceTo={filter.PriceTo}");
        queryParams.Add($"Page={filter.Page}");
        queryParams.Add($"PageSize={filter.PageSize}");
        
        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/api/product/catalog?{queryString}";

        var request = Http.CreateRequest("GET", url)
            .WithHeader("Accept", "application/json");

        return await Http.Send(httpClient, request);
    }
    else // 20% Write
    {
        var productId = PayloadGenerator.GetRandomProductId();
        var patchPayload = PayloadGenerator.GetRandomPatch();
        var url = $"{baseUrl}/api/product/{productId}";

        var jsonPayload = JsonSerializer.Serialize(patchPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var request = Http.CreateRequest("PATCH", url)
            .WithHeader("Accept", "application/json")
            .WithBody(content);

        return await Http.Send(httpClient, request);
    }
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.RampingConstant(copies: concurrentUsers, during: warmUpDuration),
    Simulation.KeepConstant(copies: concurrentUsers, during: loadDuration)
);

// Run NBomber
NBomberRunner
    .RegisterScenarios(writeScenario)
    .WithReportFileName("license_nexus_load_test")
    .WithReportFolder("./reports")
    .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
    .Run();