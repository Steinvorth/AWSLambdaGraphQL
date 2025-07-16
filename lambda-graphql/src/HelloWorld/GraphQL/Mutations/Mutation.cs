using HelloWorld.GraphQL.Types;

namespace HelloWorld.GraphQL.Mutations;

/// <summary>
/// GraphQL Mutation resolvers
/// </summary>
public class Mutation
{
    /// <summary>
    /// Creates a custom hello world message
    /// </summary>
    public HelloWorldType CreateHelloWorld(HelloWorldInput input)
    {
        return new HelloWorldType
        {
            Message = input.CustomMessage ?? "hello world from mutation",
            Location = "localhost",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates server configuration (demo mutation)
    /// </summary>
    public ServerUpdateResult UpdateServerConfig(string environment)
    {
        return new ServerUpdateResult
        {
            Success = true,
            Message = $"Server environment updated to: {environment}",
            UpdatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Result type for server update mutations
/// </summary>
public class ServerUpdateResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
