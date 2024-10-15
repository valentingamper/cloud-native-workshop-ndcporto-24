using System.Text;
using Dometrain.Api.Shared;
using Dometrain.Api.Shared.ErrorHandling;
using Dometrain.Api.Shared.Identity;
using Dometrain.Api.Shared.OpenApi;
using Dometrain.Monolith.Api.Courses;
using Dometrain.Monolith.Api.Database;
using Dometrain.Monolith.Api.Enrollments;
using Dometrain.Monolith.Api.Identity;
using Dometrain.Monolith.Api.Orders;
using Dometrain.Monolith.Api.Students;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddApiDefaults(config);

builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

builder.Services.Configure<IdentitySettings>(builder.Configuration.GetSection(IdentitySettings.SettingsKey));

builder.AddNpgsqlDataSource("dometrain");
builder.AddAzureCosmosClient("cosmosdb");
builder.AddRedisClient("redis");

builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

builder.Services.AddSingleton<IPasswordHasher<Student>, PasswordHasher<Student>>();
builder.Services.AddSingleton<IIdentityService, IdentityService>();

builder.Services.AddSingleton<IStudentService, StudentService>();
builder.Services.AddSingleton<IStudentRepository, StudentRepository>();

builder.Services.AddSingleton<ICourseService, CourseService>();
builder.Services.AddSingleton<CourseRepository>();
builder.Services.AddSingleton<ICourseRepository>(x =>
    new CachedCourseRepository(x.GetRequiredService<CourseRepository>(), x.GetRequiredService<IConnectionMultiplexer>()));

builder.Services.AddSingleton<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();

builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IOrderService, OrderService>();

builder.Services.AddMassTransit(s =>
{
    s.AddConsumers(typeof(Program).Assembly);
    s.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(config["ConnectionStrings:rabbitmq"]!));
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();


app.MapDefaultEndpoints();

app.MapApiDefaults();

app.MapIdentityEndpoints();
app.MapStudentEndpoints();
app.MapCourseEndpoints();
app.MapEnrollmentEndpoints();
app.MapOrderEndpoints();

var db = app.Services.GetRequiredService<DbInitializer>();
await db.InitializeAsync();

app.Run();
