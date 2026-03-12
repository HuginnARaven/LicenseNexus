using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using LicenseNexus.LoadTests.Helpers;
using System.Text.Json;
using System.Text;

namespace LicenseNexus.LoadTests.Scenarios;

public class WriteHeavyScenarioBuilder
{
    public static ScenarioProps[] Build(
        HttpClient httpClient,
        string baseUrl,
        int concurrentUsers,
        TimeSpan warmUpDuration,
        TimeSpan loadDuration)
    {
        int patchCopies = (int)Math.Round(concurrentUsers * 0.8);
        int postCopies = concurrentUsers - patchCopies;
        
        var patchScenario = Scenario.Create("write_heavy_patch", async context => 
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
                Simulation.RampingConstant(copies: patchCopies, during: warmUpDuration),
                Simulation.KeepConstant(copies: patchCopies, during: loadDuration)
            );
        
        var postScenario = Scenario.Create("write_heavy_post", async context => 
            {
                var newProductPayload = PayloadGenerator.GetRandomNewProduct(); 
                var url = $"{baseUrl}/api/product";
                
                var jsonPayload = JsonSerializer.Serialize(newProductPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var request = Http.CreateRequest("POST", url)
                    .WithHeader("Accept", "application/json")
                    .WithBody(content);

                var response = await Http.Send(httpClient, request);

                if (response.StatusCode == "OK" || response.StatusCode == "Created") 
                    return Response.Ok();

                return Response.Fail(statusCode: response.StatusCode);
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.RampingConstant(copies: postCopies, during: warmUpDuration),
                Simulation.KeepConstant(copies: postCopies, during: loadDuration)
            );
        
        return new[] { patchScenario, postScenario };
    }
}