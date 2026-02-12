using Api.Endpoints;
using Api.McpTools;
using Api.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IWeatherDataService, InMemoryWeatherDataService>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.AddGraphQL().AddTypes();

builder.Services
    .AddMcpServer(options =>
    {
        
    })
    .WithHttpTransport()
    .WithTools<WeatherDataTools>();
    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// REST endpoints available under /api
app.MapWeatherApi();

// GraphQL endpoint available at /graphql
app.MapGraphQL(path: "/graphql");

// MCP endpoint available at /mcp, which can be invoked by MCP clients to access the registered tools.
app.MapMcp(pattern: "/mcp");

app.RunWithGraphQLCommands(args);
