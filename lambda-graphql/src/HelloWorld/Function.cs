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
    private static readonly HttpClient client = new HttpClient();
    private readonly Query _query = new Query();
    private readonly Mutation _mutation = new Mutation();

    private static async Task<string> GetCallingIP()
    {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

        var msg = await client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext:false);

        return msg.Replace("\n","");
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        try
        {
            // Check if this is a GraphQL request
            var path = apigProxyEvent.Path?.ToLowerInvariant();
            var httpMethod = apigProxyEvent.HttpMethod?.ToUpperInvariant();

            // Handle different endpoints
            if (path == "/graphql" && httpMethod == "POST")
            {
                return await HandleGraphQLRequest(apigProxyEvent, context);
            }
            else if (path == "/hello" || path == "/")
            {
                return await HandleHelloWorldRequest(apigProxyEvent, context);
            }
            else if (path == "/health")
            {
                return await HandleHealthRequest();
            }

            // Default response
            return await HandleHelloWorldRequest(apigProxyEvent, context);
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

    private async Task<APIGatewayProxyResponse> HandleHelloWorldRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var location = await GetCallingIP();
        var body = new Dictionary<string, string>
        {
            { "message", "hello world" },
            { "location", location }
        };

        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(body),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    private async Task<APIGatewayProxyResponse> HandleHealthRequest()
    {
        var serverStatus = await _query.GetServerStatus();
        
        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(serverStatus),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
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

        // Handle simple queries
        if (query.Contains("getHelloWorld"))
        {
            var result = await _query.GetHelloWorld();
            return new { data = new { getHelloWorld = result } };
        }
        else if (query.Contains("getServerStatus"))
        {
            var result = await _query.GetServerStatus();
            return new { data = new { getServerStatus = result } };
        }
        else if (query.Contains("getServerTime"))
        {
            var result = _query.GetServerTime();
            return new { data = new { getServerTime = result } };
        }
        else if (query.Contains("createHelloWorld"))
        {
            var customMessage = request.Variables?.GetValueOrDefault("customMessage")?.ToString();
            var input = new HelloWorldInput { CustomMessage = customMessage };
            var result = _mutation.CreateHelloWorld(input);
            return new { data = new { createHelloWorld = result } };
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
                            new { name = "HelloWorldType", kind = "OBJECT" },
                            new { name = "ServerStatus", kind = "OBJECT" }
                        }
                    }
                }
            };
        }

        return new { errors = new[] { new { message = "Query not supported in this basic implementation" } } };
    }
}

public class GraphQLRequest
{
    public string Query { get; set; } = string.Empty;
    public Dictionary<string, object>? Variables { get; set; }
    public string? OperationName { get; set; }
}