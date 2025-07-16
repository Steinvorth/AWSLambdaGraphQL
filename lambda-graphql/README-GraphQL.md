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
- **Hello World**: `https://your-api-id.execute-api.region.amazonaws.com/Prod/hello`
- **Health Check**: `https://your-api-id.execute-api.region.amazonaws.com/Prod/health`

## GraphQL Schema

### Queries
- `getHelloWorld` - Returns hello world with location
- `getCustomHello(message: String!)` - Returns custom hello message
- `getServerStatus` - Returns server status information
- `getServerTime` - Returns current server time

### Mutations
- `createHelloWorld(input: HelloWorldInput!)` - Creates a custom hello world
- `updateServerConfig(environment: String!)` - Updates server configuration

### Types
- `HelloWorldType` - Basic hello world response
- `ServerStatus` - Server status information
- `HelloWorldInput` - Input for hello world mutations

## Testing Queries

See `GraphQL-Queries.md` for sample queries and mutations you can test.

### Example Query
```graphql
query {
  getHelloWorld {
    message
    location
    timestamp
  }
}
```

### Example Mutation
```graphql
mutation {
  createHelloWorld(input: { customMessage: "Hello from GraphQL!" }) {
    message
    location
    timestamp
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

## Troubleshooting

### Local Server Won't Start
- Check if port 5001 is available
- Run `dotnet restore` to ensure dependencies are installed

### Lambda Deployment Issues
- Ensure AWS CLI is configured
- Check SAM template syntax
- Verify .NET 8 runtime support in your AWS region

### GraphQL Queries Not Working
- Check query syntax in GraphQL playground
- Verify schema matches your resolvers
- Check server logs for errors
