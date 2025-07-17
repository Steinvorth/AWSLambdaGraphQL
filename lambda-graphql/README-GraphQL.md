# GraphQL Lambda Project

This project has been transformed to support both AWS Lambda execution (for AppSync) and local GraphQL development with Apollo interface.

## Project Structure

- `src/HelloWorld/` - Lambda function with GraphQL support
- `src/GraphQLServer/` - Standalone GraphQL server for development
- `src/HelloWorld/GraphQL/` - Shared GraphQL schema, queries, and mutations

## Quick Start

### 1. Install Dependencies

```bash
dotnet restore
```

### 2. Run Local GraphQL Server

```bash
# Option 1: Using VS Code task
Ctrl+Shift+P -> "Tasks: Run Task" -> "run-graphql-server"

# Option 2: Using terminal
cd src/GraphQLServer
dotnet run
```

The GraphQL server will start at `http://localhost:5001/graphql`

### 3. Test with Apollo Studio

1. Open your browser to `http://localhost:5001/graphql`
2. You'll see the HotChocolate GraphQL IDE (Banana Cake Pop)
3. Try the sample queries from `GraphQL-Queries.md`

### 4. Deploy Lambda for AppSync

```bash
sam build
sam deploy --guided
```

## Available Endpoints

### Local Development Server
- **GraphQL Playground**: `http://localhost:5001/graphql`
- **Health Check**: `http://localhost:5001/health`

### Lambda (After Deployment)
- **GraphQL**: `https://your-api-id.execute-api.region.amazonaws.com/Prod/graphql`
- **Health Check**: `https://your-api-id.execute-api.region.amazonaws.com/Prod/health`

## GraphQL Schema

### Driver Position Queries (DynamoDB)
- `getDriverPositionsByRoute(idRuta: String!)` - Get all driver positions for a specific route
- `getDriverPosition(idRuta: String!, idDriver: String!)` - Get a specific driver position
- `getAllDriverPositions` - Get all driver positions across all routes
- `getActiveDriversForRoute(idRuta: String!)` - Get active drivers for a specific route

### Driver Position Mutations (DynamoDB)
- `saveDriverPosition(input: DriverPositionInput!)` - Create or update a driver position
- `deleteDriverPosition(idRuta: String!, idDriver: String!)` - Delete a driver position
- `updateDriverStatus(idRuta: String!, idDriver: String!, status: String!)` - Update driver status

### Types
- `DriverPositionType` - Driver position data with coordinates and status
- `DriverPositionInput` - Input for creating/updating driver positions
- `DriverPositionResult` - Result type for driver position mutations

## DynamoDB Integration

This project connects to your local DynamoDB instance running on **localhost:8101** with a table called `DriverPosition`.

### Table Schema
- **Partition Key**: `IdRuta` (String) - Route identifier
- **Sort Key**: `IdDriver` (String) - Driver identifier
- **Attributes**:
  - `Longitude` (Number) - Driver's longitude coordinate
  - `Latitude` (Number) - Driver's latitude coordinate
  - `Timestamp` (String) - When the position was recorded
  - `Speed` (Number, optional) - Vehicle speed
  - `Heading` (Number, optional) - Vehicle direction
  - `Status` (String) - Driver status (Active, Inactive, etc.)

### Setup DynamoDB Local

1. **Start DynamoDB Local** on port 8101:
   ```bash
   java -Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar -sharedDb -port 8101
   ```

2. **Create the table and sample data**:
   ```bash
   # Make the script executable
   chmod +x scripts/setup-dynamodb.sh
   
   # Run the setup script
   ./scripts/setup-dynamodb.sh
   ```

   Or manually create the table:
   ```bash
   aws dynamodb create-table \
     --table-name DriverPosition \
     --attribute-definitions \
       AttributeName=IdRuta,AttributeType=S \
       AttributeName=IdDriver,AttributeType=S \
     --key-schema \
       AttributeName=IdRuta,KeyType=HASH \
       AttributeName=IdDriver,KeyType=RANGE \
     --billing-mode PAY_PER_REQUEST \
     --endpoint-url http://localhost:8101 \
     --region us-east-1
   ```

### Configuration

The GraphQL server automatically detects your local DynamoDB setup:
- **Local Development**: Connects to `http://localhost:8101`
- **Production**: Uses AWS DynamoDB with proper credentials

Configuration files:
- `src/GraphQLServer/appsettings.json` - Production settings
- `src/GraphQLServer/appsettings.Development.json` - Development settings

## Testing Queries

See `GraphQL-Queries.md` for comprehensive sample queries and mutations you can test.

### Example Hello World Query
```graphql
query {
  helloWorld {
    message
    location
    timestamp
  }
}
```

### Example Hello World Mutation
```graphql
mutation {
  createHelloWorld(input: { customMessage: "Hello from GraphQL!" }) {
    message
    location
    timestamp
  }
}
```

### Example Driver Position Queries
```graphql
# Get all drivers for a route
query {
  driverPositionsByRoute(idRuta: "ROUTE_NYC_001") {
    idDriver
    longitude
    latitude
    timestamp
    status
  }
}

# Get specific driver
query {
  driverPosition(idRuta: "ROUTE_NYC_001", idDriver: "DRIVER_JOHN") {
    idRuta
    idDriver
    longitude
    latitude
    timestamp
    speed
    heading
    status
  }
}
```

### Example Driver Position Mutations
```graphql
# Save/update driver position
mutation {
  saveDriverPosition(input: {
    idRuta: "ROUTE_NYC_001",
    idDriver: "DRIVER_JOHN",
    longitude: -74.0060,
    latitude: 40.7128,
    speed: 25.0,
    heading: 90.0,
    status: "Active"
  }) {
    success
    message
    driverPosition {
      idRuta
      idDriver
      longitude
      latitude
      timestamp
    }
  }
}

# Update driver status
mutation {
  updateDriverStatus(
    idRuta: "ROUTE_NYC_001", 
    idDriver: "DRIVER_JOHN", 
    status: "Inactive"
  ) {
    success
    message
  }
}
```

## AppSync Integration

Your Lambda function now supports GraphQL queries and can be used as a direct data source in AppSync:

1. **Direct Lambda Data Source**: Configure AppSync to use your Lambda as a direct resolver
2. **Request Mapping**: AppSync will send GraphQL requests to your Lambda
3. **Response Mapping**: Lambda returns properly formatted GraphQL responses

### AppSync Configuration Example

```javascript
// Request mapping template
{
  "version": "2018-05-29",
  "operation": "Invoke",
  "payload": {
    "field": "$context.info.fieldName",
    "arguments": $util.toJson($context.arguments),
    "query": "$context.info.selectionSetGraphQL"
  }
}
```

## Development Workflow

1. **Local Development**: Use the GraphQL server for rapid development and testing
2. **Schema Changes**: Update GraphQL schema in `src/HelloWorld/GraphQL/`
3. **Testing**: Use Apollo Studio or GraphQL playground
4. **Deploy**: Use SAM to deploy Lambda for AppSync integration

## Benefits of This Approach

✅ **Apollo Interface**: Full GraphQL IDE for testing
✅ **Local Development**: Fast iteration without deployment
✅ **AppSync Compatible**: Lambda works as AppSync data source
✅ **Shared Code**: Same GraphQL logic for local and Lambda
✅ **Hot Reload**: Local server supports hot reload during development
✅ **Type Safety**: Strong typing with C# and GraphQL
✅ **Real-time Data**: Perfect for real-time driver position tracking
✅ **DynamoDB Integration**: Efficient NoSQL storage for location data
✅ **Local Testing**: Test with local DynamoDB before AWS deployment

## Prerequisites

Before running the GraphQL server with DynamoDB support:

1. **DynamoDB Local**: Download and run DynamoDB Local on port 8101
2. **AWS CLI**: Install AWS CLI for table management
3. **.NET 8**: Ensure you have .NET 8 SDK installed
4. **Table Setup**: Run the setup script to create the DriverPosition table

## Getting Started with DynamoDB

1. **Start DynamoDB Local**:
   ```bash
   java -Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar -sharedDb -port 8101
   ```

2. **Setup the table**:
   ```bash
   ./scripts/setup-dynamodb.sh
   ```

3. **Start the GraphQL server**:
   ```bash
   # Using VS Code debugger (recommended)
   F5 -> Select "GraphQL Server (Development)"
   
   # Or using terminal
   dotnet run --project src/GraphQLServer
   ```

4. **Test the queries**: Open http://localhost:5001/graphql and try the sample queries

## Real-time AppSync Integration

This GraphQL setup is designed specifically for AWS AppSync real-time subscriptions:

1. **Direct Lambda Resolver**: Use your deployed Lambda as a direct data source
2. **Real-time Subscriptions**: AppSync can subscribe to driver position changes
3. **Efficient Queries**: Optimized DynamoDB queries for route-based data retrieval
4. **Scalable Architecture**: Handles multiple routes and drivers simultaneously

## Troubleshooting

### Local Server Won't Start
- Check if port 5001 is available
- Run `dotnet restore` to ensure dependencies are installed

### DynamoDB Connection Issues
- Ensure DynamoDB Local is running on port 8101: `nc -z localhost 8101`
- Check if the DriverPosition table exists: `aws dynamodb list-tables --endpoint-url http://localhost:8101 --region us-east-1`
- Verify table schema: `aws dynamodb describe-table --table-name DriverPosition --endpoint-url http://localhost:8101 --region us-east-1`

### Lambda Deployment Issues
- Ensure AWS CLI is configured
- Check SAM template syntax
- Verify .NET 8 runtime support in your AWS region

### GraphQL Queries Not Working
- Check query syntax in GraphQL playground
- Verify schema matches your resolvers
- Check server logs for errors
- For driver position queries, ensure DynamoDB connection is established

### Sample Data Not Loading
- Run the setup script: `./scripts/setup-dynamodb.sh`
- Manually insert test data using AWS CLI
- Check DynamoDB Local logs for errors

### Performance Issues
- Monitor DynamoDB query patterns
- Consider adding GSI (Global Secondary Index) for complex queries
- Use pagination for large result sets
- Optimize query filters in the service layer
