using ApiGateway;
using dotenv.net;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PostService.Repo;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

// if (File.Exists("./.env"))
// {
//     DotEnv.Load();
// }

var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
var rabbitMqUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");

Console.WriteLine($"RABBITMQ_HOST: {rabbitMqHost}");
Console.WriteLine($"RABBITMQ_USERNAME: {rabbitMqUsername}");
Console.WriteLine($"RABBITMQ_PASSWORD is {(string.IsNullOrWhiteSpace(rabbitMqPassword) ? "not set" : "set")}");

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

builder.Services.AddDbContext<PostDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<PostDbContext>();

// builder.Services.AddMassTransit(x =>
// {
//     x.UsingRabbitMq((context, cfg) =>
//     {
//         cfg.Host("broker", "/", h =>
//         {
//             h.Username("guest");
//             h.Password("guest");
//         });
//     });
// });

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PostDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<CorrelationIdMiddleware>(args);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();