var builder = DistributedApplication.CreateBuilder(args);

var mainDbUsername = builder.AddParameter("postgres-username");
var mainDbPassword = builder.AddParameter("postgres-password");

var mainDb = builder.AddPostgres("main-db", mainDbUsername, mainDbPassword)
    .WithDataVolume()
    .AddDatabase("dometrain");

builder.AddProject<Projects.Dometrain_Monolith_Api>("dometrain-api")
    .WithReference(mainDb);

builder.Build().Run();
