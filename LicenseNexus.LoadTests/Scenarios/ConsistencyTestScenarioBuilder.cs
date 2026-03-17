using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using LicenseNexus.LoadTests.Helpers; 

namespace LicenseNexus.LoadTests.Scenarios;

public static class ConsistencyTestScenarioBuilder
{
    private static readonly ConcurrentDictionary<int, string> ExpectedVendorNames = new();

    public static ScenarioProps[] Build(HttpClient httpClient, string baseUrl)
    {
        var vendorUpdateScenario = Scenario.Create("vendor_mutator", async context => 
        {
            var vendorId = PayloadGenerator.GetRandomVendorId(); 
            var newVendorName = $"Vendor_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            
            var payload = new 
            { 
                Name = newVendorName, 
                originalName = "TestVendor", 
                description = "Vendor description", 
                countryCode = "TST", 
                logo = "TestVendor1.logo" 
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var request = Http.CreateRequest("PUT", $"{baseUrl}/api/vendor/{vendorId}")
                .WithHeader("Accept", "application/json")
                .WithBody(content);

            var response = await Http.Send(httpClient, request);

            if (response.StatusCode == "NoContent" || response.StatusCode == "OK")
            {
                ExpectedVendorNames[vendorId] = newVendorName;
                return Response.Ok();
            }
            
            return Response.Fail();
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(2)));

        var consistencyWatcherScenario = Scenario.Create("consistency_watcher", async context => 
        {
            var productId = PayloadGenerator.GetRandomProductId();
            
            var request = Http.CreateRequest("GET", $"{baseUrl}/api/product/{productId}")
                .WithHeader("Accept", "application/json");

            var response = await Http.Send(httpClient, request);

            if (response.StatusCode == "OK")
            {
                var httpMessage = response.Payload.Value;
                var jsonString = await httpMessage.Content.ReadAsStringAsync();
                
                using var document = JsonDocument.Parse(jsonString);
                var productVendorId = document.RootElement.GetProperty("vendor").GetProperty("id").GetInt32();
                var actualVendorName = document.RootElement.GetProperty("vendor").GetProperty("name").GetString();
                
                if (ExpectedVendorNames.TryGetValue(productVendorId, out var expectedName))
                {
                    if (actualVendorName == expectedName)
                    {
                        return Response.Ok(statusCode: "In_Sync");
                    }
                    else
                    {
                        return Response.Fail(statusCode: "Out_Of_Sync", message: "Stale Data");
                    }
                }
                else
                {
                     return Response.Ok(statusCode: "Not_Mutated_Yet");
                }
            }
            return Response.Fail(statusCode: response.StatusCode, message: "Request failed");
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.KeepConstant(copies: 10, during: TimeSpan.FromMinutes(2)));
        
        return new[] { vendorUpdateScenario, consistencyWatcherScenario };
    }
}