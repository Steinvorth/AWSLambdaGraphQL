using HelloWorld.GraphQL.Types;
using HelloWorld.Services;
using HelloWorld.Models;

namespace HelloWorld.GraphQL.Queries;

/// <summary>
/// GraphQL Query resolvers
/// </summary>
public class Query
{
    private static readonly HttpClient client = new HttpClient();
    private readonly DriverPositionService? _driverPositionService;

    public Query(DriverPositionService? driverPositionService = null)
    {
        _driverPositionService = driverPositionService;
    }

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

    /// <summary>
    /// Get all driver positions for a specific route
    /// </summary>
    public async Task<List<DriverPositionType>> GetDriverPositionsByRoute(string idRuta)
    {
        if (_driverPositionService == null)
            throw new InvalidOperationException("DriverPositionService not available");

        var positions = await _driverPositionService.GetDriverPositionsByRouteAsync(idRuta);
        return positions.Select(MapToGraphQLType).ToList();
    }

    /// <summary>
    /// Get a specific driver position
    /// </summary>
    public async Task<DriverPositionType?> GetDriverPosition(string idRuta, string idDriver)
    {
        if (_driverPositionService == null)
            throw new InvalidOperationException("DriverPositionService not available");

        var position = await _driverPositionService.GetDriverPositionAsync(idRuta, idDriver);
        return position != null ? MapToGraphQLType(position) : null;
    }

    /// <summary>
    /// Get all driver positions across all routes
    /// </summary>
    public async Task<List<DriverPositionType>> GetAllDriverPositions()
    {
        if (_driverPositionService == null)
            throw new InvalidOperationException("DriverPositionService not available");

        var positions = await _driverPositionService.GetAllDriverPositionsAsync();
        return positions.Select(MapToGraphQLType).ToList();
    }

    /// <summary>
    /// Get active drivers for a specific route
    /// </summary>
    public async Task<List<DriverPositionType>> GetActiveDriversForRoute(string idRuta)
    {
        if (_driverPositionService == null)
            throw new InvalidOperationException("DriverPositionService not available");

        var positions = await _driverPositionService.GetActiveDriversForRouteAsync(idRuta);
        return positions.Select(MapToGraphQLType).ToList();
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

    private static DriverPositionType MapToGraphQLType(DriverPosition position)
    {
        return new DriverPositionType
        {
            IdRuta = position.IdRuta,
            IdDriver = position.IdDriver,
            Longitude = position.Longitude,
            Latitude = position.Latitude,
            Timestamp = position.Timestamp,
            Speed = position.Speed,
            Heading = position.Heading,
            Status = position.Status
        };
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
