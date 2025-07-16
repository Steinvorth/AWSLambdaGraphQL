# Sample GraphQL Queries for Testing

## Basic Query Examples

### 1. Get Hello World
```graphql
query GetHelloWorld {
  helloWorld {
    message
    location
    timestamp
  }
}
```

### 2. Get Custom Hello
```graphql
query GetCustomHello {
  customHello(message: "Hello from GraphQL test!")
}
```

### 3. Get Server Status
```graphql
query GetServerStatus {
  serverStatus {
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
  serverTime
}
```

## Driver Position Queries (DynamoDB)

### 1. Get All Driver Positions for a Route
```graphql
query GetDriverPositionsByRoute($idRuta: String!) {
  driverPositionsByRoute(idRuta: $idRuta) {
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

Variables:
```json
{
  "idRuta": "ROUTE_001"
}
```

### 2. Get Specific Driver Position
```graphql
query GetDriverPosition($idRuta: String!, $idDriver: String!) {
  driverPosition(idRuta: $idRuta, idDriver: $idDriver) {
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

Variables:
```json
{
  "idRuta": "ROUTE_001",
  "idDriver": "DRIVER_001"
}
```

### 3. Get All Driver Positions
```graphql
query GetAllDriverPositions {
  allDriverPositions {
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

### 4. Get Active Drivers for Route
```graphql
query GetActiveDriversForRoute($idRuta: String!) {
  activeDriversForRoute(idRuta: $idRuta) {
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

Variables:
```json
{
  "idRuta": "ROUTE_001"
}
```

## Driver Position Mutations (DynamoDB)

### 1. Save Driver Position
```graphql
mutation SaveDriverPosition($input: DriverPositionInput!) {
  saveDriverPosition(input: $input) {
    success
    message
    driverPosition {
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
}
```

Variables:
```json
{
  "input": {
    "idRuta": "ROUTE_001",
    "idDriver": "DRIVER_001",
    "longitude": -74.0060,
    "latitude": 40.7128,
    "speed": 35.5,
    "heading": 180.0,
    "status": "Active"
  }
}
```

### 2. Update Driver Status
```graphql
mutation UpdateDriverStatus($idRuta: String!, $idDriver: String!, $status: String!) {
  updateDriverStatus(idRuta: $idRuta, idDriver: $idDriver, status: $status) {
    success
    message
    driverPosition {
      idRuta
      idDriver
      status
      timestamp
    }
  }
}
```

Variables:
```json
{
  "idRuta": "ROUTE_001",
  "idDriver": "DRIVER_001",
  "status": "Inactive"
}
```

### 3. Delete Driver Position
```graphql
mutation DeleteDriverPosition($idRuta: String!, $idDriver: String!) {
  deleteDriverPosition(idRuta: $idRuta, idDriver: $idDriver) {
    success
    message
  }
}
```

Variables:
```json
{
  "idRuta": "ROUTE_001",
  "idDriver": "DRIVER_001"
}
```

## Basic Mutation Examples

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

## Sample Real-time Updates

### Simulate Driver Movement
```graphql
mutation SimulateDriverMovement {
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
      speed
      heading
      status
    }
  }
}
```

### Track Multiple Drivers
```graphql
query TrackMultipleDrivers {
  route1: driverPositionsByRoute(idRuta: "ROUTE_NYC_001") {
    idDriver
    longitude
    latitude
    timestamp
    status
  }
  route2: driverPositionsByRoute(idRuta: "ROUTE_NYC_002") {
    idDriver
    longitude
    latitude
    timestamp
    status
  }
}
```
