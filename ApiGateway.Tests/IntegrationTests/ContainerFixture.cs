using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ApiGateway.Tests.IntegrationTests
{
    public class ContainerFixture : IAsyncLifetime
    {
        private readonly INetwork _network;
        private readonly IContainer _sqlServer;
        private readonly IContainer _authService;
        private readonly IContainer _userProfileService;
        private readonly IContainer _postsService;
        private readonly IContainer _apiGateway;
        private readonly string _dockerHubUsername;

        public HttpClient HttpClient { get; private set; } = default!;

        public ContainerFixture()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<ContainerFixture>();
            var configuration = configBuilder.Build();

            _dockerHubUsername = configuration["MySecrets:DockerHubUsernameLocal"];
            if (string.IsNullOrWhiteSpace(_dockerHubUsername))
            {
                throw new ArgumentException("DockerHubUsernameLocal is not configured properly.");
            }

            var _networkName = "integration-tests" + Guid.NewGuid();
            _network = new NetworkBuilder()
            .WithName(_networkName)
            .Build();

            // SQL Server
            _sqlServer = new ContainerBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SA_PASSWORD", configuration["SqlServer:Password"] ?? "Str0ngPass123!")
                .WithPortBinding(1433, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("db")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
                .Build();

            // AuthService
            _authService = new ContainerBuilder()
                .WithImage($"{_dockerHubUsername}/mysocialnetwork-auth-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("auth")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();

            // UserProfileService
            _userProfileService = new ContainerBuilder()
                .WithImage($"{_dockerHubUsername}/mysocialnetwork-user-profile-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("user")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();

            // PostsService
            _postsService = new ContainerBuilder()
                .WithImage($"{_dockerHubUsername}/mysocialnetwork-post-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("post")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();

            // ApiGateway
            _apiGateway = new ContainerBuilder()
                .WithImage($"{_dockerHubUsername}/mysocialnetwork-api-gateway-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("api")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _network.CreateAsync();
            await _sqlServer.StartAsync();
            await _authService.StartAsync();
            await _userProfileService.StartAsync();
            await _postsService.StartAsync();
            await _apiGateway.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(15));

            var apiPort = _apiGateway.GetMappedPublicPort(80);
            var baseUrl = $"http://localhost:{apiPort}";
            HttpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task DisposeAsync()
        {
            HttpClient.Dispose();
            await _apiGateway.StopAsync();
            await _userProfileService.StopAsync();
            await _postsService.StopAsync();
            await _authService.StopAsync();
            await _sqlServer.StopAsync();
            await _network.DeleteAsync();
        }
    }
}