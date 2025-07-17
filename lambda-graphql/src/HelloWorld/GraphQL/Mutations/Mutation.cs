using HelloWorld.GraphQL.Types;
using HelloWorld.Services;
using HelloWorld.Models;
using HotChocolate.Subscriptions;

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
    /// Create or update a driver position - simplified for car tracking
    /// </summary>
    public async Task<DriverPositionResult> SaveDriverPosition(
        DriverPositionInput input,
        [Service] ITopicEventSender eventSender)
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

            var driverPositionType = MapToGraphQLType(savedPosition);

            // Publish the update to the subscription topic
            await eventSender.SendAsync($"DriverPositionUpdated_{input.IdRuta}", driverPositionType);

            return new DriverPositionResult
            {
                Success = true,
                Message = "Driver position saved successfully",
                DriverPosition = driverPositionType
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
