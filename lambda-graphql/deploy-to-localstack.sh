#!/bin/bash

# Deploy Lambda GraphQL to LocalStack
# This script builds and deploys your SAM application to LocalStack

set -e

echo "🚀 Starting deployment to LocalStack..."

# Check if LocalStack is running
echo "Checking LocalStack status..."
if ! curl -s http://localhost:4566/_localstack/health > /dev/null; then
    echo "❌ LocalStack is not running or not accessible on localhost:4566"
    echo "Please start LocalStack with: localstack start -d"
    exit 1
fi

echo "✅ LocalStack is running"

# Check if awslocal is installed
if ! command -v awslocal &> /dev/null; then
    echo "⚠️  awslocal CLI not found. Installing..."
    pip install awscli-local
fi

# Set LocalStack endpoint
export AWS_ENDPOINT_URL=http://localhost:4566
export AWS_DEFAULT_REGION=us-east-1
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test

# Build the SAM application
echo "🔨 Building SAM application..."
sam build

# Deploy to LocalStack
echo "🚢 Deploying to LocalStack..."
sam deploy \
    --config-file samconfig-localstack.toml \
    --config-env localstack \
    --no-confirm-changeset \
    --no-fail-on-empty-changeset

echo "✅ Deployment completed!"

# Get the API Gateway endpoint
echo "📋 Getting API Gateway endpoint..."
API_ID=$(awslocal apigateway get-rest-apis --query 'items[0].id' --output text)
if [ "$API_ID" != "None" ] && [ "$API_ID" != "" ]; then
    echo "🌐 API Gateway endpoint: http://localhost:4566/restapis/$API_ID/local/_user_request_"
    echo "🔍 GraphQL endpoint: http://localhost:4566/restapis/$API_ID/local/_user_request_/graphql"
    echo "❤️  Health check: http://localhost:4566/restapis/$API_ID/local/_user_request_/health"
else
    echo "⚠️  Could not determine API Gateway endpoint"
fi

# Create DynamoDB table in LocalStack
echo "📊 Setting up DynamoDB table in LocalStack..."
awslocal dynamodb create-table \
    --table-name DriverPosition \
    --attribute-definitions \
        AttributeName=IdRuta,AttributeType=S \
        AttributeName=IdDriver,AttributeType=S \
    --key-schema \
        AttributeName=IdRuta,KeyType=HASH \
        AttributeName=IdDriver,KeyType=RANGE \
    --billing-mode PAY_PER_REQUEST \
    --region us-east-1 || echo "Table might already exist"

# Insert sample data (matching the simple 4-column schema)
echo "📝 Inserting sample data..."
awslocal dynamodb put-item \
    --table-name DriverPosition \
    --item '{
        "IdRuta": {"S": "ROUTE_NYC_001"},
        "IdDriver": {"S": "DRIVER_JOHN"},
        "Longitude": {"N": "-74.0060"},
        "Latitude": {"N": "40.7128"}
    }' || echo "Sample data insertion failed"

awslocal dynamodb put-item \
    --table-name DriverPosition \
    --item '{
        "IdRuta": {"S": "ROUTE_NYC_001"},
        "IdDriver": {"S": "DRIVER_MARY"},
        "Longitude": {"N": "-74.0110"},
        "Latitude": {"N": "40.7180"}
    }' || echo "Sample data insertion failed"

awslocal dynamodb put-item \
    --table-name DriverPosition \
    --item '{
        "IdRuta": {"S": "ROUTE_NYC_002"},
        "IdDriver": {"S": "DRIVER_BOB"},
        "Longitude": {"N": "-74.0200"},
        "Latitude": {"N": "40.7200"}
    }' || echo "Sample data insertion failed"

echo "🎉 All done! Your Lambda function is now deployed to LocalStack."
echo ""
echo "You can test it with:"
echo "curl -X POST http://localhost:4566/restapis/$API_ID/local/_user_request_/graphql \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"query\": \"query { helloWorld { message timestamp } }\"}'"
