namespace HelloWorld.GraphQL.Types;

/// <summary>
/// Represents a simple Hello World response type for GraphQL
/// </summary>
public class HelloWorldType
{
    public string Message { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Input type for Hello World mutations
/// </summary>
public class HelloWorldInput
{
    public string? CustomMessage { get; set; }
}
