#!/bin/bash

# Setup DynamoDB table for DriverPosition
# Requires AWS CLI to be installed and DynamoDB Local running on port 8101

DYNAMODB_ENDPOINT="http://localhost:8101"
TABLE_NAME="DriverPosition"

echo "Setting up DynamoDB table: $TABLE_NAME"
echo "DynamoDB endpoint: $DYNAMODB_ENDPOINT"

# Check if DynamoDB Local is running
echo "Checking if DynamoDB Local is running on port 8101..."
if ! nc -z localhost 8101; then
    echo "❌ DynamoDB Local is not running on port 8101"
    echo "Please start DynamoDB Local with:"
    echo "java -Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar -sharedDb -port 8101"
    exit 1
fi

echo "✅ DynamoDB Local is running"

# Check if table already exists
echo "Checking if table $TABLE_NAME exists..."
TABLE_EXISTS=$(aws dynamodb describe-table \
    --table-name $TABLE_NAME \
    --endpoint-url $DYNAMODB_ENDPOINT \
    --region us-east-1 \
    --output text \
    --query 'Table.TableName' 2>/dev/null)

if [ "$TABLE_EXISTS" = "$TABLE_NAME" ]; then
    echo "✅ Table $TABLE_NAME already exists"
    
    # Show table description
    echo "Table description:"
    aws dynamodb describe-table \
        --table-name $TABLE_NAME \
        --endpoint-url $DYNAMODB_ENDPOINT \
        --region us-east-1 \
        --output table \
        --query 'Table.[TableName,TableStatus,KeySchema,AttributeDefinitions]'
else
    echo "⚠️  Table $TABLE_NAME does not exist. Creating..."
    
    # Create the table
    aws dynamodb create-table \
        --table-name $TABLE_NAME \
        --attribute-definitions \
            AttributeName=IdRuta,AttributeType=S \
            AttributeName=IdDriver,AttributeType=S \
        --key-schema \
            AttributeName=IdRuta,KeyType=HASH \
            AttributeName=IdDriver,KeyType=RANGE \
        --billing-mode PAY_PER_REQUEST \
        --endpoint-url $DYNAMODB_ENDPOINT \
        --region us-east-1

    if [ $? -eq 0 ]; then
        echo "✅ Table $TABLE_NAME created successfully"
        
        # Wait for table to be active
        echo "Waiting for table to be active..."
        aws dynamodb wait table-exists \
            --table-name $TABLE_NAME \
            --endpoint-url $DYNAMODB_ENDPOINT \
            --region us-east-1
        
        echo "✅ Table is now active"
    else
        echo "❌ Failed to create table $TABLE_NAME"
        exit 1
    fi
fi

# Insert sample data
echo "Inserting sample data..."

# Sample driver positions
aws dynamodb put-item \
    --table-name $TABLE_NAME \
    --item '{
        "IdRuta": {"S": "ROUTE_NYC_001"},
        "IdDriver": {"S": "DRIVER_JOHN"},
        "Longitude": {"N": "-74.0060"},
        "Latitude": {"N": "40.7128"},
        "Timestamp": {"S": "'$(date -u +%Y-%m-%dT%H:%M:%S.%3NZ)'"},
        "Speed": {"N": "25.5"},
        "Heading": {"N": "90.0"},
        "Status": {"S": "Active"}
    }' \
    --endpoint-url $DYNAMODB_ENDPOINT \
    --region us-east-1

aws dynamodb put-item \
    --table-name $TABLE_NAME \
    --item '{
        "IdRuta": {"S": "ROUTE_NYC_001"},
        "IdDriver": {"S": "DRIVER_MARY"},
        "Longitude": {"N": "-74.0110"},
        "Latitude": {"N": "40.7180"},
        "Timestamp": {"S": "'$(date -u +%Y-%m-%dT%H:%M:%S.%3NZ)'"},
        "Speed": {"N": "30.0"},
        "Heading": {"N": "180.0"},
        "Status": {"S": "Active"}
    }' \
    --endpoint-url $DYNAMODB_ENDPOINT \
    --region us-east-1

aws dynamodb put-item \
    --table-name $TABLE_NAME \
    --item '{
        "IdRuta": {"S": "ROUTE_NYC_002"},
        "IdDriver": {"S": "DRIVER_BOB"},
        "Longitude": {"N": "-74.0200"},
        "Latitude": {"N": "40.7200"},
        "Timestamp": {"S": "'$(date -u +%Y-%m-%dT%H:%M:%S.%3NZ)'"},
        "Speed": {"N": "15.0"},
        "Heading": {"N": "270.0"},
        "Status": {"S": "Inactive"}
    }' \
    --endpoint-url $DYNAMODB_ENDPOINT \
    --region us-east-1

echo "✅ Sample data inserted"

# Show sample data
echo "Sample data in table:"
aws dynamodb scan \
    --table-name $TABLE_NAME \
    --endpoint-url $DYNAMODB_ENDPOINT \
    --region us-east-1 \
    --output table \
    --query 'Items[*].[IdRuta.S,IdDriver.S,Longitude.N,Latitude.N,Status.S]'

echo ""
echo "🎉 Setup complete!"
echo ""
echo "You can now:"
echo "1. Start the GraphQL server: dotnet run --project src/GraphQLServer"
echo "2. Open Apollo Studio at: http://localhost:5001/graphql"
echo "3. Test the driver position queries and mutations"
echo ""
echo "Sample query to try:"
echo 'query { getDriverPositionsByRoute(idRuta: "ROUTE_NYC_001") { idDriver longitude latitude status } }'
