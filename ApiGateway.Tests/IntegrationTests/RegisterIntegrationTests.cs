using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Xunit;

namespace ApiGateway.Tests.IntegrationTests
{
    public class RegisterIntegrationTests : IAsyncLifetime
    {
        private readonly INetwork _network;
        private readonly DotNet.Testcontainers.Containers.IContainer _authService;
        private readonly DotNet.Testcontainers.Containers.IContainer _userprofileService;
        private readonly DotNet.Testcontainers.Containers.IContainer _apiGateway;
        private readonly DotNet.Testcontainers.Containers.IContainer _sqlServer;
        private HttpClient _httpClient;

        public RegisterIntegrationTests()
        {
            // Network
            _network = new NetworkBuilder()
                .WithName("network")
                .Build();

            // SQL Server
            _sqlServer = new ContainerBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SA_PASSWORD", "Str0ngPass123!")
                .WithPortBinding(1433, true)
                .WithNetwork("network")  // використання рядка, а не _network.Name
                .WithNetworkAliases("sql-server")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
                .Build();

            // AuthService
            _authService = new ContainerBuilder()
                .WithImage("mysocialnetwork-auth-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork("network")
                .WithNetworkAliases("auth-service")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();

            // UserProfileService
            _userprofileService = new ContainerBuilder()
                .WithImage("mysocialnetwork-user-profile-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork("network")
                .WithNetworkAliases("user-profile-service")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();

            // ApiGateway 
            _apiGateway = new ContainerBuilder()
                .WithImage("mysocialnetwork-api-gateway-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork("network")
                .WithNetworkAliases("api-gateway-service")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();
        }

        public async Task InitializeAsync()
        {

            await _network.CreateAsync();

            await _sqlServer.StartAsync();

            await _authService.StartAsync();
            await _userprofileService.StartAsync();
            await _apiGateway.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(15));

            var apiPort = _apiGateway.GetMappedPublicPort(80);
            var baseUrl = $"http://localhost:{apiPort}";
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task DisposeAsync()
        {
            _httpClient.Dispose();
            await _apiGateway.StopAsync();
            await _userprofileService.StopAsync();
            await _authService.StopAsync();
            await _sqlServer.StopAsync();
            await _network.DeleteAsync();
        }

        [Fact]
        public async Task RegisterEndpoint_ReturnsCreatedStatus()
        {
            var requestData = new
            {
                Email = "test@example.com",
                Password = "Password123!",
                Name = "Test User",
                BirthDate = "1990-01-01T00:00:00Z"
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", requestData);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("test@example.com", content);
        }
    }
}
