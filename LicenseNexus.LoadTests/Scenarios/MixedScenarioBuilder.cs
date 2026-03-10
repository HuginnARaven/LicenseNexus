using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using LicenseNexus.LoadTests.Helpers;
using System.Text.Json;
using System.Text;

namespace LicenseNexus.LoadTests.Scenarios;

public class MixedScenarioBuilder
{
    public static ScenarioProps Build(
        HttpClient httpClient,
        string baseUrl,
        int concurrentUsers,
        TimeSpan warmUpDuration,
        TimeSpan loadDuration)
    {
        return Scenario.Create("mixed_scenario", async context => 
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
    }
}