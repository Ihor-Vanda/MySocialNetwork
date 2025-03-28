using ApiGateway.HealthChecks;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
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
