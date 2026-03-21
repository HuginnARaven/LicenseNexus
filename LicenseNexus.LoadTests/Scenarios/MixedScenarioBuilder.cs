using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using LicenseNexus.LoadTests.Helpers;
using System.Text.Json;
using System.Text;

namespace LicenseNexus.LoadTests.Scenarios;

public class MixedScenarioBuilder
{
    public static ScenarioProps[] Build(
        HttpClient httpClient,
        string baseUrl,
        int concurrentUsers,
        TimeSpan warmUpDuration,
        TimeSpan loadDuration)
    {
        int readCopies = (int)Math.Round(concurrentUsers * 0.8);
        int writeCopies = concurrentUsers - readCopies;
        
        var readScenario = Scenario.Create("mixed_read_scenario", async context =>
        {
            var queryString = PayloadGenerator.GetRandomPreGeneratedUrl();
            var url = $"{baseUrl}/api/product/catalog?{queryString}";
            
            var request = Http.CreateRequest("GET", url)
                .WithHeader("Accept", "application/json");
            
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.RampingConstant(copies: readCopies, during: warmUpDuration),
            Simulation.KeepConstant(copies: readCopies, during: loadDuration)
        );
        
        var writeScenario = Scenario.Create("mixed_write_scenario", async context =>
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
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.RampingConstant(copies: writeCopies, during: warmUpDuration),
            Simulation.KeepConstant(copies: writeCopies, during: loadDuration)
        );
        
        return new[] { readScenario, writeScenario };
    }
}