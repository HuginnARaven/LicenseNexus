using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using LicenseNexus.LoadTests.Helpers;

namespace LicenseNexus.LoadTests.Scenarios;

public class TextSearchScenarioBuilder
{
    public static ScenarioProps Build(
        HttpClient httpClient,
        string baseUrl,
        int concurrentUsers,
        TimeSpan warmUpDuration,
        TimeSpan loadDuration)
    {
        return Scenario.Create("text_search_scenario", async context => 
            {
                var searchTerm = PayloadGenerator.GetRandomSearchTerm();
    
                var request = Http.CreateRequest("GET", $"{baseUrl}/api/product/catalog?Search={Uri.EscapeDataString(searchTerm)}&Page=1&PageSize=50")
                    .WithHeader("Accept", "application/json");

                var response = await Http.Send(httpClient, request);

                if (response.StatusCode == "OK")
                {
                    return Response.Ok();
                } 
                return Response.Fail(statusCode: response.StatusCode, message: "Search Failed"); 
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.RampingConstant(copies: concurrentUsers, during: warmUpDuration),
                Simulation.KeepConstant(copies: concurrentUsers, during: loadDuration)
            );
    }
}