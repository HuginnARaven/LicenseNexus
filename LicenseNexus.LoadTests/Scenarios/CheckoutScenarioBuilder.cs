using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using LicenseNexus.LoadTests.Helpers;
using System.Text.Json;
using System.Text;

namespace LicenseNexus.LoadTests.Scenarios;

public class CheckoutScenarioBuilder
{
    public static ScenarioProps Build(
        HttpClient httpClient,
        string baseUrl,
        int concurrentUsers,
        TimeSpan warmUpDuration,
        TimeSpan loadDuration)
    {
        return Scenario.Create("checkout_scenario", async context => 
            {
                // Create Order
                var orderPayload = PayloadGenerator.GetRandomOrder();
                var orderJson = JsonSerializer.Serialize(orderPayload);
                var orderContent = new StringContent(orderJson, Encoding.UTF8, "application/json");

                var createOrderRequest = Http.CreateRequest("POST", $"{baseUrl}/api/order")
                    .WithHeader("Accept", "application/json")
                    .WithBody(orderContent);

                var orderResponse = await Http.Send(httpClient, createOrderRequest);

                if (orderResponse.StatusCode != "OK" && orderResponse.StatusCode != "Created")
                {
                    return Response.Fail(statusCode: orderResponse.StatusCode, message: "Failed to create order");
                }

                // Extract Order ID
                var orderJsonString = await orderResponse.Payload.Value.Content.ReadAsStringAsync();
                using var orderDoc = JsonDocument.Parse(orderJsonString);
                
                if (!orderDoc.RootElement.TryGetProperty("id", out var idElement))
                {
                     return Response.Fail(statusCode: "Error", message: "Order ID not found in response");
                }
                var orderId = idElement.GetInt32();

                // Add Items to Order
                var itemsCount = Random.Shared.Next(1, 6); // Randomly select from 1 to 5 items
                var orderProducts = PayloadGenerator.GetRandomOrderProducts(itemsCount);

                foreach (var item in orderProducts)
                {
                    item.OrderId = orderId; // Link to the created order
                    
                    var itemJson = JsonSerializer.Serialize(item);
                    var itemContent = new StringContent(itemJson, Encoding.UTF8, "application/json");

                    var addItemRequest = Http.CreateRequest("POST", $"{baseUrl}/api/order/OrderProduct")
                        .WithHeader("Accept", "application/json")
                        .WithBody(itemContent);

                    var itemResponse = await Http.Send(httpClient, addItemRequest);
                    
                    if (itemResponse.StatusCode != "OK" && itemResponse.StatusCode != "Created")
                    {
                         return Response.Fail(statusCode: itemResponse.StatusCode, message: "Failed to add order item");
                    }
                }

                return Response.Ok(statusCode: "Order_Placed");
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.RampingConstant(copies: concurrentUsers, during: warmUpDuration),
                Simulation.KeepConstant(copies: concurrentUsers, during: loadDuration)
            );
    }
}