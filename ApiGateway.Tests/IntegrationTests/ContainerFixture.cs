using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace ApiGateway.Tests.IntegrationTests
{
    [Collection("Docker Network Collection")]
    public class ContainerFixture : IAsyncLifetime
    {
        private readonly IContainer _sqlServer;
        private readonly IContainer _authService;
        private readonly IContainer _userProfileService;
        private readonly IContainer _postsService;
        private readonly IContainer _apiGateway;
        public HttpClient HttpClient { get; private set; } = default!;

        public ContainerFixture(SharedNetworkFixture networkFixture)
        {
            var _networkName = networkFixture.NetworkName;

            // SQL Server
            _sqlServer = new ContainerBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SA_PASSWORD", "Str0ngPass123!")
                .WithPortBinding(1433, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("sql-server")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
                .Build();

            // AuthService
            _authService = new ContainerBuilder()
                .WithImage("mysocialnetwork-auth-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("auth-service")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();

            // UserProfileService
            _userProfileService = new ContainerBuilder()
                .WithImage("mysocialnetwork-user-profile-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("user-profile-service")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();

            // PostsService
            _postsService = new ContainerBuilder()
                .WithImage("mysocialnetwork-post-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("post-service")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();

            // ApiGateway 
            _apiGateway = new ContainerBuilder()
                .WithImage("mysocialnetwork-api-gateway-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("api-gateway-service")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();
        }

        public async Task InitializeAsync()
        {
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
        }
    }
}
