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

        if (isLocal)
        {
            // Local DynamoDB configuration - always use port 8101 for your setup
            var localEndpoint = "http://localhost:8101";
            
            // Override if environment variable is set
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DYNAMODB_ENDPOINT")))
            {
                localEndpoint = Environment.GetEnvironmentVariable("DYNAMODB_ENDPOINT");
            }

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
