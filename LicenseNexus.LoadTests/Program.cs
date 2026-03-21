using NBomber.CSharp;
using NBomber.Http.CSharp;
using LicenseNexus.LoadTests.Helpers;
using LicenseNexus.LoadTests.Scenarios;
using System.Text.Json;
using System.Text;
using NBomber.Contracts.Stats;
using System.Collections.Concurrent;
using LicenseNexus.Domain.Models;
using System.Diagnostics;
using NBomber.Contracts;

using var httpClient = new HttpClient();

// Configuration
var baseUrl = "http://localhost:5000"; 
var warmUpDuration = TimeSpan.FromSeconds(30);
var loadDuration = TimeSpan.FromMinutes(5);
var concurrentUsers = 100;

var products = await FetchProductsAsync(httpClient, $"{baseUrl}/api/product");
var vendorIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/vendor");
var groupIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/productgroup");
var typeIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/producttype");
var unitMeasureIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/unitmeasure");
var currencyIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/currency");
var customerIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/customer");
var orderStatusIds = await FetchIdsAsync(httpClient, $"{baseUrl}/api/orderstatus");
var terms = products
    .Select(p => p.Title)
    .Where(x => !string.IsNullOrEmpty(x))
    .SelectMany(x => x!.Split(' ', StringSplitOptions.RemoveEmptyEntries))
    .Where(x => x.Length > 3) // taking 3+ letter word TODO: mb change later
    .Distinct()
    .ToArray();
var tagsNames = await FetchPropertyStringValuesAsync(httpClient, $"{baseUrl}/api/tag", "name");

PayloadGenerator.Initialize(products, vendorIds, groupIds, typeIds, unitMeasureIds, currencyIds, customerIds, orderStatusIds, terms, tagsNames);

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

async Task<string[]> FetchPropertyStringValuesAsync(HttpClient client, string url, string propertyName)
{
    try
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error loading from {url}: {response.StatusCode}");
            return Array.Empty<string>();
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(jsonString);
        
        return document.RootElement
            .EnumerateArray()
            .Select(x => x.GetProperty(propertyName).ToString())
            .ToArray();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Parsing error {url}: {ex.Message}");
        return Array.Empty<string>();
    }
}

async Task<List<ProductModel>> FetchProductsAsync(HttpClient client, string url)
{
    try
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error loading from {url}: {response.StatusCode}");
            return new List<ProductModel>();
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<List<ProductModel>>(jsonString, options) ?? new List<ProductModel>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Parsing error {url}: {ex.Message}");
        return new List<ProductModel>();
    }
}

// Read-Heavy Scenario (Product Search & Pagination)
var readScenario = ReadHeavyScenarioBuilder.Build(httpClient, baseUrl, concurrentUsers, warmUpDuration, loadDuration);

// Write-Heavy Scenario (80% PATCH, 20% POST)
var writeScenario = WriteHeavyScenarioBuilder.Build(httpClient, baseUrl, concurrentUsers, warmUpDuration, loadDuration);

// Mixed Scenario (80% Read, 20% PATCH)
var mixedScenario = MixedScenarioBuilder.Build(httpClient, baseUrl, concurrentUsers, warmUpDuration, loadDuration);

var consistencyScenario = ConsistencyTestScenarioBuilder.Build(httpClient, baseUrl);

var checkoutScenario = CheckoutScenarioBuilder.Build(httpClient, baseUrl, concurrentUsers, warmUpDuration, loadDuration);

var textSearchScenario = TextSearchScenarioBuilder.Build(httpClient, baseUrl, concurrentUsers, warmUpDuration, loadDuration);

var scenarioDict = new Dictionary<string, ScenarioProps[]>
{
    { "read", [readScenario] },
    { "write", writeScenario },
    { "mixed", mixedScenario },
    { "consistency", consistencyScenario },
    { "checkout", [checkoutScenario] },
    { "textsearch", [textSearchScenario] }
};

var currArch = "mongo";

var targetScenario = args.Length > 0 ? args[0].ToLower() : "read";

if (!scenarioDict.ContainsKey(targetScenario))
{
    Console.WriteLine($"Scenario '{targetScenario}' not found. Valid scenarios: {string.Join(", ", scenarioDict.Keys)}");
    return;
}

var selectedScenario = scenarioDict[targetScenario];
var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
var reportFolder = $"./reports/{timestamp}_{currArch}_{targetScenario}";
Directory.CreateDirectory(reportFolder);

HardwareMonitor.Start($"{reportFolder}/{currArch}_{targetScenario}_load_test_metrics.csv");
NBomberRunner
    .RegisterScenarios(selectedScenario)
    .WithReportFileName($"{currArch}_{targetScenario}_load_test_report")
    .WithReportFolder(reportFolder)
    .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
    .Run();
HardwareMonitor.Stop();