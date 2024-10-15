using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureProvisioning();

var mainDbUsername = builder.AddParameter("postgres-username");
var mainDbPassword = builder.AddParameter("postgres-password");

var mainDb = builder.AddPostgres("main-db", mainDbUsername, mainDbPassword, port: 5432)
    .WithPgAdmin()
    .WithDataVolume()
    .AddDatabase("dometrain");

var cartDb = builder.AddAzureCosmosDB("cosmosdb")
    .AddDatabase("cartdb");

var redis = builder.AddRedis("redis")
    .WithRedisCommander();

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var mainApi = builder.AddProject<Projects.Dometrain_Monolith_Api>("dometrain-api")
    .WithReference(mainDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReplicas(1);

builder.AddProject<Projects.Dometrain_Cart_Api>("dometrain-cart-api")
    .WithReference(cartDb)
    .WithReference(redis)
    .WithEnvironment("MainApi__BaseUrl", mainApi.GetEndpoint("http"));

builder.Build().Run();
