using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;

namespace HelloWorld.Models;

/// <summary>
/// DynamoDB model for DriverPosition table - simplified for real-time car tracking
/// </summary>
[DynamoDBTable("DriverPosition")]
public class DriverPosition
{
    /// <summary>
    /// Route ID - Partition Key
    /// </summary>
    [DynamoDBHashKey("IdRuta")]
    [Required]
    public string IdRuta { get; set; } = string.Empty;

    /// <summary>
    /// Driver ID - Sort Key
    /// </summary>
    [DynamoDBRangeKey("IdDriver")]
    [Required]
    public string IdDriver { get; set; } = string.Empty;

    /// <summary>
    /// Driver's current longitude
    /// </summary>
    [DynamoDBProperty("Longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// Driver's current latitude
    /// </summary>
    [DynamoDBProperty("Latitude")]
    public double Latitude { get; set; }
}
