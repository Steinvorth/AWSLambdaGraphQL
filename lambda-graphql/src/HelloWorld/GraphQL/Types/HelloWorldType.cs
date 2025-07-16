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

/// <summary>
/// GraphQL type for DriverPosition - simplified for real-time car tracking
/// </summary>
public class DriverPositionType
{
    public string IdRuta { get; set; } = string.Empty;
    public string IdDriver { get; set; } = string.Empty;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}

/// <summary>
/// Input type for creating/updating driver positions - simplified
/// </summary>
public class DriverPositionInput
{
    public string IdRuta { get; set; } = string.Empty;
    public string IdDriver { get; set; } = string.Empty;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}

/// <summary>
/// Result type for driver position mutations
/// </summary>
public class DriverPositionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DriverPositionType? DriverPosition { get; set; }
}
