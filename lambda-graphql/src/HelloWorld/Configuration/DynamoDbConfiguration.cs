using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;

namespace HelloWorld.Configuration;

/// <summary>
/// Configuration for DynamoDB connection
/// </summary>
public static class DynamoDbConfiguration
{
    /// <summary>
    /// Creates DynamoDB client for local or AWS environment
    /// </summary>
    public static IAmazonDynamoDB CreateDynamoDbClient(IConfiguration? configuration = null)
    {
        var config = new AmazonDynamoDBConfig();
        
        // Check if we're running locally and should connect to local DynamoDB
        var isLocal = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ||
                     Environment.GetEnvironmentVariable("AWS_SAM_LOCAL") == "true" ||
                     configuration?.GetValue<bool>("DynamoDB:UseLocal") == true;

        // Check for LocalStack environment
        var isLocalStack = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOCALSTACK_HOSTNAME")) ||
                          Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL") != null;

        if (isLocal || isLocalStack)
        {
            // Priority order for endpoint configuration:
            // 1. DYNAMODB_ENDPOINT environment variable
            // 2. AWS_ENDPOINT_URL environment variable (LocalStack)
            // 3. Configuration file LocalEndpoint
            // 4. Default LocalStack endpoint
            var localEndpoint = Environment.GetEnvironmentVariable("DYNAMODB_ENDPOINT") ??
                               Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL") ??
                               configuration?.GetValue<string>("DynamoDB:LocalEndpoint") ??
                               "http://localhost:4566";

            config.ServiceURL = localEndpoint;
            config.UseHttp = true;
            
            Console.WriteLine($"Connecting to local DynamoDB at: {localEndpoint}");
        }
        else
        {
            // AWS DynamoDB configuration
            var region = configuration?.GetValue<string>("AWS:Region") ?? 
                        Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION") ?? 
                        "us-east-1";
            config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
            
            Console.WriteLine($"Connecting to AWS DynamoDB in region: {region}");
        }

        return new AmazonDynamoDBClient(config);
    }

    /// <summary>
    /// Validates DynamoDB connection and table existence
    /// </summary>
    public static async Task<bool> ValidateConnectionAsync(IAmazonDynamoDB client)
    {
        try
        {
            // Try to list tables to validate connection
            var response = await client.ListTablesAsync();
            
            // Check if DriverPosition table exists
            var tableExists = response.TableNames.Contains("DriverPosition");
            
            if (!tableExists)
            {
                Console.WriteLine("Warning: DriverPosition table not found. You may need to create it.");
                Console.WriteLine("You can create the table using AWS CLI or DynamoDB console.");
            }
            else
            {
                Console.WriteLine("✓ DriverPosition table found");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to DynamoDB: {ex.Message}");
            return false;
        }
    }
}
