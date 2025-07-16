using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;

namespace HelloWorld.Models;

/// <summary>
/// DynamoDB model for DriverPosition table
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

    /// <summary>
    /// Timestamp when the position was recorded
    /// </summary>
    [DynamoDBProperty("Timestamp")]
    public string TimestampString { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp as DateTime for convenience (computed property)
    /// </summary>
    [DynamoDBIgnore]
    public DateTime Timestamp 
    {
        get
        {
            if (DateTime.TryParse(TimestampString, out var result))
                return result;
            return DateTime.UtcNow;
        }
        set
        {
            TimestampString = value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }
    }

    /// <summary>
    /// Optional: Speed of the driver (if available)
    /// </summary>
    [DynamoDBProperty("Speed")]
    public double? Speed { get; set; }

    /// <summary>
    /// Optional: Heading/direction of the driver (if available)
    /// </summary>
    [DynamoDBProperty("Heading")]
    public double? Heading { get; set; }

    /// <summary>
    /// Status of the driver (Active, Inactive, etc.)
    /// </summary>
    [DynamoDBProperty("Status")]
    public string Status { get; set; } = "Active";
}
