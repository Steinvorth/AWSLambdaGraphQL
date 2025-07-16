using HelloWorld.GraphQL.Queries;
using HelloWorld.GraphQL.Mutations;
using HelloWorld.GraphQL.Subscriptions;
using HelloWorld.Services;
using HelloWorld.Configuration;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Services.AddLogging();

// Configure DynamoDB
builder.Services.AddSingleton<IAmazonDynamoDB>(provider =>
{
    var configuration = provider.GetService<IConfiguration>();
    return DynamoDbConfiguration.CreateDynamoDbClient(configuration);
});

// Add DynamoDB service
builder.Services.AddScoped<DriverPositionService>();

// Add GraphQL services with dependency injection
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

// Add CORS for Apollo Studio/Apollo Explorer
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Validate DynamoDB connection on startup
using (var scope = app.Services.CreateScope())
{
    var dynamoClient = scope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Validating DynamoDB connection...");
    var isConnected = await DynamoDbConfiguration.ValidateConnectionAsync(dynamoClient);
    
    if (isConnected)
    {
        logger.LogInformation("✓ DynamoDB connection validated successfully");
    }
    else
    {
        logger.LogWarning("⚠️ DynamoDB connection issues detected");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseWebSockets();
app.UseCors("AllowAll");

// Map GraphQL endpoint
app.MapGraphQL();

// Add a simple health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Add a root endpoint that redirects to GraphQL
app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();
