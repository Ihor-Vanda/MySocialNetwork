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

DotEnv.Load();

// Налаштування Serilog
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

var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
var rabbitMqUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
if (string.IsNullOrWhiteSpace(rabbitMqHost) || string.IsNullOrWhiteSpace(rabbitMqUsername) || string.IsNullOrWhiteSpace(rabbitMqPassword))
{
    throw new ArgumentException("RabbitMq connection is not configured properly.");
}

builder.Services.AddDbContext<PostDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<PostDbContext>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUsername);
            h.Password(rabbitMqPassword);
        });
    });
});

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