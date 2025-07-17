using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

namespace HelloWorld.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestHealthEndpoint()
    {
        var request = new APIGatewayProxyRequest
        {
            Path = "/health",
            HttpMethod = "GET"
        };
        var context = new TestLambdaContext();

        var function = new Function();
        var response = await function.FunctionHandler(request, context);

        Assert.Equal(200, response.StatusCode);
        Assert.Contains("healthy", response.Body);
    }

    [Fact]
    public async Task TestDriverPositionGraphQLQuery()
    {
        var graphqlRequest = new
        {
            query = "{ __schema { types { name } } }"
        };

        var request = new APIGatewayProxyRequest
        {
            Path = "/graphql",
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(graphqlRequest)
        };
        var context = new TestLambdaContext();

        var function = new Function();
        var response = await function.FunctionHandler(request, context);

        Assert.Equal(200, response.StatusCode);
        Assert.Contains("data", response.Body);
    }
}