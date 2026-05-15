using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using LicenseNexus.LoadTests.Helpers;

namespace LicenseNexus.LoadTests.Scenarios;

public static class ReadHeavyScenarioBuilder
{
    public static ScenarioProps Build(
        HttpClient httpClient, 
        string baseUrl, 
        int concurrentUsers, 
        TimeSpan warmUpDuration, 
        TimeSpan loadDuration)
    {
        return Scenario.Create("read_heavy_scenario", async context => 
            {
                var queryString = PayloadGenerator.GetRandomPreGeneratedUrl();
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
    }
}