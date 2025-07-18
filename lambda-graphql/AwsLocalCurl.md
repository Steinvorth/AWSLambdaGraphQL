curl -X GET "http://localhost:4566/restapis/5oldbcsfzy/Prod/_user_request_/health"

curl -X POST "http://localhost:4566/restapis/5oldbcsfzy/Prod/_user_request_/graphql" \
  -H "Content-Type: application/json" \
  -d '{"query":"{ __schema { types { name } } }"}'

curl -X POST "http://localhost:4566/restapis/5oldbcsfzy/Prod/_user_request_/graphql" \
  -H "Content-Type: application/json" \
  -d '{"query":"query { driverPositionsByRoute(idRuta: \"ROUTE_NYC_001\") { idDriver longitude latitude } }"}'

curl -X POST "http://localhost:4566/restapis/5oldbcsfzy/Prod/_user_request_/graphql" \
  -H "Content-Type: application/json" \
  -d '{"query":"query { allDriverPositions { idDriver idRuta longitude latitude } }"}'

curl -X POST "http://localhost:4566/restapis/5oldbcsfzy/Prod/_user_request_/graphql" \
  -H "Content-Type: application/json" \
  -d '{"query":"query { driverPosition(idRuta: \"ROUTE_NYC_001\", idDriver: \"DRIVER_JOHN\") { idDriver longitude latitude } }"}'

curl -X POST "http://localhost:4566/restapis/5oldbcsfzy/Prod/_user_request_/graphql" \
  -H "Content-Type: application/json" \
  -d '{"query":"query { driverPosition(idRuta: \"ROUTE_NYC_001\", idDriver: \"DRIVER_JOHN\") { idDriver longitude latitude } }"}'

when making changes to code: to be able to redeploy.

cd /Users/emi/Documents/vscode/lambda/lambda-graphql
sam build
sam deploy --config-file samconfig-localstack.toml --config-env localstack

