using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using LicenseNexus.LoadTests.Helpers;
using System.Text.Json;
using System.Text;

namespace LicenseNexus.LoadTests.Scenarios;

public class WriteHeavyScenarioBuilder
{
    public static ScenarioProps Build(
        HttpClient httpClient,
        string baseUrl,
        int concurrentUsers,
        TimeSpan warmUpDuration,
        TimeSpan loadDuration)
    {
        return Scenario.Create("write_heavy_scenario", async context => 
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
                Simulation.RampingConstant(copies: concurrentUsers, during: warmUpDuration),
                Simulation.KeepConstant(copies: concurrentUsers, during: loadDuration)
            );
    }
}