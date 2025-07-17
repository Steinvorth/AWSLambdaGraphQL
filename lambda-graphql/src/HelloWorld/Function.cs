using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using HelloWorld.GraphQL.Queries;
using HelloWorld.GraphQL.Mutations;
using HelloWorld.GraphQL.Types;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloWorld;

public class Function
{
    private readonly Query _query = new Query();
    private readonly Mutation _mutation = new Mutation();

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        try
        {
            // Check if this is a GraphQL request
            var path = apigProxyEvent.Path?.ToLowerInvariant();
            var httpMethod = apigProxyEvent.HttpMethod?.ToUpperInvariant();

            // Handle GraphQL endpoint
            if (path == "/graphql" && httpMethod == "POST")
            {
                return await HandleGraphQLRequest(apigProxyEvent, context);
            }
            else if (path == "/health")
            {
                return await HandleHealthRequest();
            }

            // Return 404 for unsupported endpoints
            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(new { error = "Endpoint not found" }),
                StatusCode = 404,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error processing request: {ex.Message}");
            
            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(new { error = "Internal server error", message = ex.Message }),
                StatusCode = 500,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }

    private Task<APIGatewayProxyResponse> HandleHealthRequest()
    {
        var healthStatus = new { status = "healthy", timestamp = DateTime.UtcNow };
        
        return Task.FromResult(new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(healthStatus),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        });
    }

    private async Task<APIGatewayProxyResponse> HandleGraphQLRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            // Parse the GraphQL request
            var requestBody = request.Body;
            if (request.IsBase64Encoded)
            {
                var bytes = Convert.FromBase64String(requestBody);
                requestBody = System.Text.Encoding.UTF8.GetString(bytes);
            }

            var graphqlRequest = JsonSerializer.Deserialize<GraphQLRequest>(requestBody);
            
            if (graphqlRequest == null || string.IsNullOrEmpty(graphqlRequest.Query))
            {
                return new APIGatewayProxyResponse
                {
                    Body = JsonSerializer.Serialize(new { error = "Invalid GraphQL request" }),
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            // Simple GraphQL query resolver (basic implementation)
            var response = await ExecuteGraphQLQuery(graphqlRequest, context);

            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(response),
                StatusCode = 200,
                Headers = new Dictionary<string, string> 
                { 
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "Content-Type" },
                    { "Access-Control-Allow-Methods", "POST, OPTIONS" }
                }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"GraphQL execution error: {ex.Message}");
            
            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(new { errors = new[] { new { message = ex.Message } } }),
                StatusCode = 500,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }

    private async Task<object> ExecuteGraphQLQuery(GraphQLRequest request, ILambdaContext context)
    {
        // This is a simple GraphQL resolver - in production you'd use HotChocolate's execution engine
        var query = request.Query.Trim();

        // Handle driver position queries
        if (query.Contains("driverPositionsByRoute"))
        {
            var idRuta = ExtractStringParameter(query, "idRuta");
            if (!string.IsNullOrEmpty(idRuta))
            {
                var result = await _query.DriverPositionsByRoute(idRuta);
                return new { data = new { driverPositionsByRoute = result } };
            }
        }
        else if (query.Contains("driverPosition") && !query.Contains("driverPositionsByRoute"))
        {
            var idRuta = ExtractStringParameter(query, "idRuta");
            var idDriver = ExtractStringParameter(query, "idDriver");
            if (!string.IsNullOrEmpty(idRuta) && !string.IsNullOrEmpty(idDriver))
            {
                var result = await _query.DriverPosition(idRuta, idDriver);
                return new { data = new { driverPosition = result } };
            }
        }
        else if (query.Contains("allDriverPositions"))
        {
            var result = await _query.AllDriverPositions();
            return new { data = new { allDriverPositions = result } };
        }
        else if (query.Contains("saveDriverPosition"))
        {
            var input = ExtractDriverPositionInput(request);
            if (input != null)
            {
                // For Lambda, we don't have real-time subscriptions, so we use a null event sender
                var result = await _mutation.SaveDriverPosition(input, new MockEventSender());
                return new { data = new { saveDriverPosition = result } };
            }
        }
        else if (query.Contains("deleteDriverPosition"))
        {
            var idRuta = ExtractStringParameter(query, "idRuta");
            var idDriver = ExtractStringParameter(query, "idDriver");
            if (!string.IsNullOrEmpty(idRuta) && !string.IsNullOrEmpty(idDriver))
            {
                var result = await _mutation.DeleteDriverPosition(idRuta, idDriver);
                return new { data = new { deleteDriverPosition = result } };
            }
        }

        // Default introspection query response
        if (query.Contains("__schema") || query.Contains("IntrospectionQuery"))
        {
            return new
            {
                data = new
                {
                    __schema = new
                    {
                        types = new[]
                        {
                            new { name = "Query", kind = "OBJECT" },
                            new { name = "Mutation", kind = "OBJECT" },
                            new { name = "DriverPositionType", kind = "OBJECT" },
                            new { name = "DriverPositionInput", kind = "INPUT_OBJECT" },
                            new { name = "DriverPositionResult", kind = "OBJECT" }
                        }
                    }
                }
            };
        }

        return new { errors = new[] { new { message = "Query not supported in this basic implementation" } } };
    }

    private string ExtractStringParameter(string query, string paramName)
    {
        // Simple parameter extraction - in production use proper GraphQL parsing
        var pattern = $"\"{paramName}\"\\s*:\\s*\"([^\"]+)\"";
        var match = System.Text.RegularExpressions.Regex.Match(query, pattern);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private DriverPositionInput? ExtractDriverPositionInput(GraphQLRequest request)
    {
        // Simple input extraction - in production use proper GraphQL parsing
        if (request.Variables?.TryGetValue("input", out var inputObj) == true)
        {
            try
            {
                var jsonElement = (JsonElement)inputObj;
                return new DriverPositionInput
                {
                    IdRuta = jsonElement.GetProperty("idRuta").GetString() ?? string.Empty,
                    IdDriver = jsonElement.GetProperty("idDriver").GetString() ?? string.Empty,
                    Longitude = jsonElement.GetProperty("longitude").GetDouble(),
                    Latitude = jsonElement.GetProperty("latitude").GetDouble()
                };
            }
            catch
            {
                return null;
            }
        }
        return null;
    }
}

public class GraphQLRequest
{
    public string Query { get; set; } = string.Empty;
    public Dictionary<string, object>? Variables { get; set; }
    public string? OperationName { get; set; }
}

// Mock event sender for Lambda execution (no real-time subscriptions)
public class MockEventSender : HotChocolate.Subscriptions.ITopicEventSender
{
    public ValueTask SendAsync<TMessage>(string topicName, TMessage message, CancellationToken cancellationToken = default)
    {
        // In Lambda, we don't have real-time subscriptions, so this is a no-op
        return ValueTask.CompletedTask;
    }

    public ValueTask CompleteAsync(string topicName)
    {
        // In Lambda, we don't have real-time subscriptions, so this is a no-op
        return ValueTask.CompletedTask;
    }
}