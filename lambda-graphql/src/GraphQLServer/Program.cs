using HelloWorld.GraphQL.Queries;
using HelloWorld.GraphQL.Mutations;

var builder = WebApplication.CreateBuilder(args);

// Add GraphQL services
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");

// Map GraphQL endpoint
app.MapGraphQL();

// Add a simple health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Add a root endpoint that redirects to GraphQL
app.MapGet("/", () => Results.Redirect("/graphql"));

app.Run();
