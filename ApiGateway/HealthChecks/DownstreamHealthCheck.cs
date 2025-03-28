using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiGateway.HealthChecks;

public class DownstreamHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    public DownstreamHealthCheck(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("http://localhost:80/internal-health/auth", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return HealthCheckResult.Unhealthy("Auth service is unhealthy.");
        }

        response = await _httpClient.GetAsync("http://localhost:80/internal-health/post", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return HealthCheckResult.Unhealthy("Post service is unhealthy.");
        }

        response = await _httpClient.GetAsync("http://localhost:80/internal-health/user", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return HealthCheckResult.Unhealthy("User service is unhealthy.");
        }
        return HealthCheckResult.Healthy("Downstream services are healthy.");
    }
}