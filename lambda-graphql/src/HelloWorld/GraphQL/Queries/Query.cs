using HelloWorld.GraphQL.Types;
using HelloWorld.Services;
using HelloWorld.Models;

namespace HelloWorld.GraphQL.Queries;

/// <summary>
/// GraphQL Query resolvers
/// </summary>
public class Query
{
    private readonly DriverPositionService? _driverPositionService;

    public Query(DriverPositionService? driverPositionService = null)
    {
        _driverPositionService = driverPositionService;
    }



    /// <summary>
    /// Get all driver positions for a specific route
    /// </summary>
    public async Task<List<DriverPositionType>> DriverPositionsByRoute(string idRuta)
    {
        if (_driverPositionService == null)
            throw new InvalidOperationException("DriverPositionService not available");

        var positions = await _driverPositionService.GetDriverPositionsByRouteAsync(idRuta);
        return positions.Select(MapToGraphQLType).ToList();
    }

    /// <summary>
    /// Get a specific driver position
    /// </summary>
    public async Task<DriverPositionType?> DriverPosition(string idRuta, string idDriver)
    {
        if (_driverPositionService == null)
            throw new InvalidOperationException("DriverPositionService not available");

        var position = await _driverPositionService.GetDriverPositionAsync(idRuta, idDriver);
        return position != null ? MapToGraphQLType(position) : null;
    }

    /// <summary>
    /// Get all driver positions across all routes
    /// </summary>
    public async Task<List<DriverPositionType>> AllDriverPositions()
    {
        if (_driverPositionService == null)
            throw new InvalidOperationException("DriverPositionService not available");

        var positions = await _driverPositionService.GetAllDriverPositionsAsync();
        return positions.Select(MapToGraphQLType).ToList();
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
