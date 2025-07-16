# Sample GraphQL Queries for Testing

## Query Examples

### 1. Get Hello World
```graphql
query GetHelloWorld {
  getHelloWorld {
    message
    location
    timestamp
  }
}
```

### 2. Get Custom Hello
```graphql
query GetCustomHello {
  getCustomHello(message: "Hello from GraphQL test!")
}
```

### 3. Get Server Status
```graphql
query GetServerStatus {
  getServerStatus {
    isOnline
    version
    environment
    location
    lastStarted
  }
}
```

### 4. Get Server Time
```graphql
query GetServerTime {
  getServerTime
}
```

## Mutation Examples

### 1. Create Hello World
```graphql
mutation CreateHelloWorld {
  createHelloWorld(input: { customMessage: "Hello from mutation!" }) {
    message
    location
    timestamp
  }
}
```

### 2. Update Server Config
```graphql
mutation UpdateServerConfig {
  updateServerConfig(environment: "production") {
    success
    message
    updatedAt
  }
}
```

## Introspection Query
```graphql
query IntrospectionQuery {
  __schema {
    types {
      name
      kind
    }
  }
}
```

## Using with variables
```graphql
query GetCustomHelloWithVariable($msg: String!) {
  getCustomHello(message: $msg)
}
```

Variables:
```json
{
  "msg": "Hello with variables!"
}
```
