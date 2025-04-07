using System;
using System.Net.Http;
using System.Threading.Tasks;
using dotenv.net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using DotNet.Testcontainers.Configurations;
using Serilog;

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
        private readonly IContainer _broker;

        public HttpClient HttpClient { get; private set; } = default!;

        public ContainerFixture()
        {
            if (File.Exists("./.env"))
            {
                DotEnv.Load();
            }

            var dockerHubUsername = Environment.GetEnvironmentVariable("DOCKER_HUB_USERNAME");
            var sqlServerPassword = Environment.GetEnvironmentVariable("SQL_SERVER_PASSWORD");

            var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            var rabbitMqUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
            var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");

            if (rabbitMqHost == null || rabbitMqUsername == null || rabbitMqPassword == null
                || dockerHubUsername == null || sqlServerPassword == null)
            {
                var configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();

                dockerHubUsername = configuration["DOCKER_HUB_USERNAME"];
                sqlServerPassword = configuration["SQL_SERVER_PASSWORD"];
                rabbitMqHost = configuration["RABBITMQ_HOST"];
                rabbitMqUsername = configuration["RABBITMQ_USERNAME"];
                rabbitMqPassword = configuration["RABBITMQ_PASSWORD"];
            }

            if (dockerHubUsername == null)
            {
                throw new ArgumentException("Docker Hub username is not configured properly");
            }

            if (sqlServerPassword == null)
            {
                throw new ArgumentException("SQL Server Password is not configured properly");
            }

            if (rabbitMqHost == null || rabbitMqUsername == null || rabbitMqPassword == null)
            {
                throw new ArgumentException("RabbitMq connection settings are not configured properly");
            }


            var _networkName = "integration-tests";
            _network = new NetworkBuilder()
            .WithName(_networkName)
            .Build();

            // SQL Server
            _sqlServer = new ContainerBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SA_PASSWORD", sqlServerPassword)
                .WithPortBinding(1433, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("db")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
                .Build();

            // RabbitMQ
            _broker = new ContainerBuilder()
                .WithImage("rabbitmq:3-management")
                .WithPortBinding(5672, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases(rabbitMqHost)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
                .Build();

            // AuthService
            _authService = new ContainerBuilder()
                .WithImage($"{dockerHubUsername}/mysocialnetwork-auth-service:latest")
                .WithEnvironment("RABBITMQ_HOST", rabbitMqHost)
                .WithEnvironment("RABBITMQ_USERNAME", rabbitMqUsername)
                .WithEnvironment("RABBITMQ_PASSWORD", rabbitMqPassword)
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("auth")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .DependsOn(_sqlServer)
                .DependsOn(_broker)
                .Build();

            // UserProfileService
            _userProfileService = new ContainerBuilder()
                .WithImage($"{dockerHubUsername}/mysocialnetwork-user-profile-service:latest")
                .WithEnvironment("RABBITMQ_HOST", rabbitMqHost)
                .WithEnvironment("RABBITMQ_USERNAME", rabbitMqUsername)
                .WithEnvironment("RABBITMQ_PASSWORD", rabbitMqPassword)
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("user")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .DependsOn(_sqlServer)
                .DependsOn(_broker)
                .Build();

            // PostsService
            _postsService = new ContainerBuilder()
                .WithImage($"{dockerHubUsername}/mysocialnetwork-post-service:latest")
                .WithEnvironment("RABBITMQ_HOST", rabbitMqHost)
                .WithEnvironment("RABBITMQ_USERNAME", rabbitMqUsername)
                .WithEnvironment("RABBITMQ_PASSWORD", rabbitMqPassword)
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("post")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .DependsOn(_sqlServer)
                .DependsOn(_broker)
                .Build();

            // ApiGateway
            _apiGateway = new ContainerBuilder()
                .WithImage($"{dockerHubUsername}/mysocialnetwork-api-gateway-service:latest")
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("api")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .DependsOn(_authService)
                .DependsOn(_userProfileService)
                .DependsOn(_postsService)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _network.CreateAsync();
            await _sqlServer.StartAsync();
            await _broker.StartAsync();
            await _authService.StartAsync();
            await _userProfileService.StartAsync();
            await _postsService.StartAsync();
            await _apiGateway.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(20));

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
            await _broker.StopAsync();
            await _network.DeleteAsync();
        }
    }
}