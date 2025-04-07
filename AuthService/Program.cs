using System.Text;
using AuthService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using ApiGateway;
using MassTransit;
using dotenv;
using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Serilog
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

DotEnv.Load();
var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();

// var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
// var rabbitMqUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
// var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");

// rabbitMqHost ??= configuration["RABBITMQ_HOST"];
// rabbitMqUsername ??= configuration["RABBITMQ_USERNAME"];
// rabbitMqUsername ??= configuration["RABBITMQ_PASSWORD"];

// if (rabbitMqUsername == null || rabbitMqPassword == null || rabbitMqHost == null)
// {
//     Log.Logger.Fatal("Can't get connection settings for rabbitMq");
//     throw new ArgumentException("RabbitMq connection settings is not configured properly.");
// }

builder.Services.AddDbContext<AuthDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AuthDbContext>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("Jwt") ?? throw new Exception("Can't get jwt settings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? throw new Exception("Can't get jwt secret key"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

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
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
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
