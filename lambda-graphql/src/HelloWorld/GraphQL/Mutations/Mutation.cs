using HelloWorld.GraphQL.Types;
using HelloWorld.Services;
using HelloWorld.Models;

namespace HelloWorld.GraphQL.Mutations;

/// <summary>
/// GraphQL Mutation resolvers
/// </summary>
public class Mutation
{
    private readonly DriverPositionService? _driverPositionService;

    public Mutation(DriverPositionService? driverPositionService = null)
    {
        _driverPositionService = driverPositionService;
    }

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

    /// <summary>
    /// Create or update a driver position - simplified for car tracking
    /// </summary>
    public async Task<DriverPositionResult> SaveDriverPosition(DriverPositionInput input)
    {
        try
        {
            if (_driverPositionService == null)
                throw new InvalidOperationException("DriverPositionService not available");

            var driverPosition = new DriverPosition
            {
                IdRuta = input.IdRuta,
                IdDriver = input.IdDriver,
                Longitude = input.Longitude,
                Latitude = input.Latitude
            };

            var savedPosition = await _driverPositionService.SaveDriverPositionAsync(driverPosition);

            return new DriverPositionResult
            {
                Success = true,
                Message = "Driver position saved successfully",
                DriverPosition = MapToGraphQLType(savedPosition)
            };
        }
        catch (Exception ex)
        {
            return new DriverPositionResult
            {
                Success = false,
                Message = $"Error saving driver position: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Delete a driver position
    /// </summary>
    public async Task<DriverPositionResult> DeleteDriverPosition(string idRuta, string idDriver)
    {
        try
        {
            if (_driverPositionService == null)
                throw new InvalidOperationException("DriverPositionService not available");

            await _driverPositionService.DeleteDriverPositionAsync(idRuta, idDriver);

            return new DriverPositionResult
            {
                Success = true,
                Message = "Driver position deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new DriverPositionResult
            {
                Success = false,
                Message = $"Error deleting driver position: {ex.Message}"
            };
        }
    }

    private static DriverPositionType MapToGraphQLType(DriverPosition position)
    {
        return new DriverPositionType
        {
            IdRuta = position.IdRuta,
            IdDriver = position.IdDriver,
            Longitude = position.Longitude,
            Latitude = position.Latitude
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
