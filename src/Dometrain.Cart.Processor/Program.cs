
using Dometrain.Cart.Processor;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<ChangeFeedProcessorService>();

builder.AddAzureCosmosClient("cosmosdb");

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
