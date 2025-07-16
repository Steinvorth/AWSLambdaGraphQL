using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using HelloWorld.Models;
using Microsoft.Extensions.Logging;

namespace HelloWorld.Services;

/// <summary>
/// Service for managing driver positions in DynamoDB
/// </summary>
public class DriverPositionService
{
    private readonly DynamoDBContext _context;
    private readonly ILogger<DriverPositionService>? _logger;

    public DriverPositionService(IAmazonDynamoDB dynamoDbClient, ILogger<DriverPositionService>? logger = null)
    {
        _context = new DynamoDBContext(dynamoDbClient);
        _logger = logger;
    }

    /// <summary>
    /// Get all driver positions for a specific route
    /// </summary>
    public async Task<List<DriverPosition>> GetDriverPositionsByRouteAsync(string idRuta)
    {
        try
        {
            _logger?.LogInformation("Getting driver positions for route: {IdRuta}", idRuta);
            
            var queryConfig = new DynamoDBOperationConfig
            {
                QueryFilter = new List<ScanCondition>()
            };

            var search = _context.QueryAsync<DriverPosition>(idRuta, queryConfig);
            var results = await search.GetRemainingAsync();
            
            _logger?.LogInformation("Found {Count} driver positions for route {IdRuta}", results.Count, idRuta);
            return results;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting driver positions for route {IdRuta}", idRuta);
            throw;
        }
    }

    /// <summary>
    /// Get a specific driver position
    /// </summary>
    public async Task<DriverPosition?> GetDriverPositionAsync(string idRuta, string idDriver)
    {
        try
        {
            _logger?.LogInformation("Getting driver position for route: {IdRuta}, driver: {IdDriver}", idRuta, idDriver);
            
            var result = await _context.LoadAsync<DriverPosition>(idRuta, idDriver);
            
            if (result != null)
            {
                _logger?.LogInformation("Found driver position for route {IdRuta}, driver {IdDriver}", idRuta, idDriver);
            }
            else
            {
                _logger?.LogWarning("Driver position not found for route {IdRuta}, driver {IdDriver}", idRuta, idDriver);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting driver position for route {IdRuta}, driver {IdDriver}", idRuta, idDriver);
            throw;
        }
    }

    /// <summary>
    /// Get all driver positions across all routes
    /// </summary>
    public async Task<List<DriverPosition>> GetAllDriverPositionsAsync()
    {
        try
        {
            _logger?.LogInformation("Getting all driver positions");
            
            var scanConfig = new DynamoDBOperationConfig
            {
                Limit = 100 // Limit for performance
            };

            var search = _context.ScanAsync<DriverPosition>(new List<ScanCondition>(), scanConfig);
            var results = await search.GetRemainingAsync();
            
            _logger?.LogInformation("Found {Count} total driver positions", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting all driver positions");
            throw;
        }
    }

    /// <summary>
    /// Save or update a driver position
    /// </summary>
    public async Task<DriverPosition> SaveDriverPositionAsync(DriverPosition driverPosition)
    {
        try
        {
            _logger?.LogInformation("Saving driver position for route: {IdRuta}, driver: {IdDriver}", 
                driverPosition.IdRuta, driverPosition.IdDriver);
            
            driverPosition.Timestamp = DateTime.UtcNow;
            await _context.SaveAsync(driverPosition);
            
            _logger?.LogInformation("Successfully saved driver position for route {IdRuta}, driver {IdDriver}", 
                driverPosition.IdRuta, driverPosition.IdDriver);
            
            return driverPosition;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving driver position for route {IdRuta}, driver {IdDriver}", 
                driverPosition.IdRuta, driverPosition.IdDriver);
            throw;
        }
    }

    /// <summary>
    /// Delete a driver position
    /// </summary>
    public async Task DeleteDriverPositionAsync(string idRuta, string idDriver)
    {
        try
        {
            _logger?.LogInformation("Deleting driver position for route: {IdRuta}, driver: {IdDriver}", idRuta, idDriver);
            
            await _context.DeleteAsync<DriverPosition>(idRuta, idDriver);
            
            _logger?.LogInformation("Successfully deleted driver position for route {IdRuta}, driver {IdDriver}", idRuta, idDriver);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting driver position for route {IdRuta}, driver {IdDriver}", idRuta, idDriver);
            throw;
        }
    }

    /// <summary>
    /// Get active drivers for a route (status = "Active")
    /// </summary>
    public async Task<List<DriverPosition>> GetActiveDriversForRouteAsync(string idRuta)
    {
        try
        {
            _logger?.LogInformation("Getting active drivers for route: {IdRuta}", idRuta);
            
            var queryConfig = new DynamoDBOperationConfig
            {
                QueryFilter = new List<ScanCondition>
                {
                    new ScanCondition("Status", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, "Active")
                }
            };

            var search = _context.QueryAsync<DriverPosition>(idRuta, queryConfig);
            var results = await search.GetRemainingAsync();
            
            _logger?.LogInformation("Found {Count} active drivers for route {IdRuta}", results.Count, idRuta);
            return results;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting active drivers for route {IdRuta}", idRuta);
            throw;
        }
    }
}
