using HelloWorld.GraphQL.Types;

namespace HelloWorld.GraphQL.Queries;

/// <summary>
/// GraphQL Query resolvers
/// </summary>
public class Query
{
    private static readonly HttpClient client = new HttpClient();

    /// <summary>
    /// Returns a simple hello world message with location
    /// </summary>
    public async Task<HelloWorldType> GetHelloWorld()
    {
        var location = await GetCallingIP();
        
        return new HelloWorldType
        {
            Message = "hello world",
            Location = location,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Returns a customized hello message
    /// </summary>
    public HelloWorldType GetCustomHello(string message = "Hello from GraphQL!")
    {
        return new HelloWorldType
        {
            Message = message,
            Location = "localhost",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Returns current server time
    /// </summary>
    public DateTime GetServerTime() => DateTime.UtcNow;

    /// <summary>
    /// Returns server status information
    /// </summary>
    public async Task<ServerStatus> GetServerStatus()
    {
        var location = await GetCallingIP();
        
        return new ServerStatus
        {
            IsOnline = true,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            Location = location,
            LastStarted = DateTime.UtcNow
        };
    }

    private static async Task<string> GetCallingIP()
    {
        try
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

            var msg = await client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(false);
            return msg.Replace("\n", "").Trim();
        }
        catch (Exception)
        {
            return "localhost";
        }
    }
}

/// <summary>
/// Server status type for GraphQL
/// </summary>
public class ServerStatus
{
    public bool IsOnline { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime LastStarted { get; set; }
}
