using ApiGateway;
using ApiGateway.HealthChecks;
using dotenv.net;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using dotenv;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

DotEnv.Load();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "myapp-logs-{0:yyyy:MM:dd}"
    })
    .CreateLogger();

builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services
    .AddOcelot(builder.Configuration)
    .AddSingletonDefinedAggregator<RegisterAggregator>();

builder.Services.AddHttpClient("DownstreamClient");
builder.Services.AddHealthChecks()
    .AddCheck<DownstreamHealthCheck>("Downstream Health Check");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80);
    options.ListenAnyIP(8181);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<CorrelationIdMiddleware>();

// app.MapHealthChecks("/health");

app.UseRouting();

app.MapWhen(ctx => ctx.Connection.LocalPort == 8181, appBuilder =>
{
    appBuilder.UseRouting();

    appBuilder.UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/health");
    });
});

await app.UseOcelot();
app.Run();
