using System.Text;
using ApiGateway;
using dotenv.net;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserProfileService.Consumers;
using UserProfileService.Repo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "myapp-logs-{0:yyyy:MM:dd}"
    })
    .CreateLogger();

builder.Host.UseSerilog();


//Read .env
if (File.Exists("./.env"))
{
    DotEnv.Load();
}

var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");

Log.Information($"DB_PASSWORD is {(string.IsNullOrWhiteSpace(dbPassword) ? "not set" : "set")}");
Log.Information($"DB_USER is {(string.IsNullOrWhiteSpace(dbUser) ? "not set" : "set")}");
Log.Information($"DB_HOST is {(string.IsNullOrWhiteSpace(dbHost) ? "not set" : "set")}");

if (dbPassword == null || dbUser == null || dbHost == null)
{
    var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    dbPassword = configuration["DB_PASSWORD"];
    dbUser = configuration["DB_USER"];
    dbHost = configuration["DB_HOST"];
}

if (dbPassword == null || dbUser == null || dbHost == null)
{
    throw new ArgumentException("DB connection is not configured properly");
}

var connectionString = $"Host={dbHost};Port=5432;Database=UserServiceDB;Username={dbUser};Password={dbPassword}";
builder.Services.AddDbContext<UserProfileDbContext>(option =>
    option.UseNpgsql(connectionString));

//RabbitMQ
var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
var rabbitMqUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");

Log.Information($"RABBITMQ_HOST is {(string.IsNullOrWhiteSpace(rabbitMqHost) ? "not set" : "set")}");
Log.Information($"RABBITMQ_USERNAME is {(string.IsNullOrWhiteSpace(rabbitMqUsername) ? "not set" : "set")}");
Log.Information($"RABBITMQ_PASSWORD is {(string.IsNullOrWhiteSpace(rabbitMqPassword) ? "not set" : "set")}");

if (rabbitMqHost == null || rabbitMqUsername == null || rabbitMqPassword == null)
{
    var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    rabbitMqHost = configuration["RABBITMQ_HOST"];
    rabbitMqUsername = configuration["RABBITMQ_USERNAME"];
    rabbitMqPassword = configuration["RABBITMQ_PASSWORD"];
}

if (rabbitMqHost == null || rabbitMqUsername == null || rabbitMqPassword == null)
{
    throw new ArgumentException("RabbitMq connection settings are not configured properly.");
}

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUsername);
            h.Password(rabbitMqPassword);
        });

        cfg.ReceiveEndpoint("user-created", e =>
        {
            e.ConfigureConsumer<UserCreatedConsumer>(context);
        });
    });
});

//HealthCheck
builder.Services.AddHealthChecks()
    .AddDbContextCheck<UserProfileDbContext>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserProfileDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
