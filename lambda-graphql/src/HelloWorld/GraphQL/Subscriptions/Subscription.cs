using HotChocolate;
using HotChocolate.Types;
using HelloWorld.GraphQL.Types;

namespace HelloWorld.GraphQL.Subscriptions;

/// <summary>
/// GraphQL Subscription resolvers for real-time updates.
/// </summary>
public class Subscription
{
    /// <summary>
    /// Subscribes to driver position updates for a specific route.
    /// When a mutation updates a driver's position, this subscription will fire
    /// and push the new position to subscribed clients.
    /// </summary>
    /// <param name="driverPosition">The updated driver position data.</param>
    /// <param name="idRuta">The route ID to filter events by.</param>
    /// <returns>The updated driver position.</returns>
    [Subscribe]
    [Topic("DriverPositionUpdated_{idRuta}")]
    public DriverPositionType OnDriverPositionUpdated(
        [EventMessage] DriverPositionType driverPosition,
        string idRuta)
    {
        // This method is triggered when a message is published to the topic.
        // The driverPosition is the payload sent from the mutation.
        // The idRuta argument is used by HotChocolate to filter which clients receive the update.
        return driverPosition;
    }
}
