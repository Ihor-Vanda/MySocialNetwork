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
        private readonly IContainer _postres;
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

            //RabbitMQ
            var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            var rabbitMqUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
            var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");

            Log.Information($"RABBITMQ_HOST is {(string.IsNullOrWhiteSpace(rabbitMqHost) ? "not set" : "set")}");
            Log.Information($"RABBITMQ_USERNAME is {(string.IsNullOrWhiteSpace(rabbitMqUsername) ? "not set" : "set")}");
            Log.Information($"RABBITMQ_PASSWORD is {(string.IsNullOrWhiteSpace(rabbitMqPassword) ? "not set" : "set")}");

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

            //DB
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var dbUser = Environment.GetEnvironmentVariable("DB_USER");
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
            var dbNameAuth = Environment.GetEnvironmentVariable("AUTH_DB_NAME");
            var dbNameUser = Environment.GetEnvironmentVariable("USER_DB_NAME");
            var dbNamePost = Environment.GetEnvironmentVariable("POST_DB_NAME");

            if (dbPassword == null || dbUser == null || dbHost == null || dbPort == null || dbNameAuth == null || dbNameUser == null || dbNamePost == null || dbNamePost == null)
            {
                var configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();

                dbPassword = configuration["DB_PASSWORD"];
                dbUser = configuration["DB_USER"];
                dbHost = configuration["DB_HOST"];
                dbPort = configuration["DB_PORT"];
                dbNameAuth = configuration["AUTH_DB_NAME"];
                dbNameUser = configuration["USER_DB_NAME"];
                dbNamePost = configuration["POST_DB_NAME"];
            }

            if (dbPassword == null || dbUser == null || dbHost == null || dbPort == null || dbNameAuth == null || dbNameUser == null || dbNamePost == null || dbNamePost == null)
            {
                throw new ArgumentException("DB connection is not configured properly");
            }


            var _networkName = "integration-tests";
            _network = new NetworkBuilder()
            .WithName(_networkName)
            .Build();

            //Postres
            _postres = new ContainerBuilder()
                .WithImage("postgres:latest")
                .WithEnvironment("POSTGRES_PASSWORD", "Str0ngPass123!")
                .WithEnvironment("POSTGRES_USER", "MySocNet")
                .WithPortBinding(5432, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("db")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
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
                .WithEnvironment("AUTH_DB_NAME", dbNameAuth)
                .WithEnvironment("DB_PORT", dbPort)
                .WithEnvironment("DB_HOST", dbHost)
                .WithEnvironment("DB_USER", dbUser)
                .WithEnvironment("DB_PASSWORD", dbPassword)
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("auth")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .DependsOn(_postres)
                .DependsOn(_broker)
                .Build();

            // UserProfileService
            _userProfileService = new ContainerBuilder()
                .WithImage($"{dockerHubUsername}/mysocialnetwork-user-profile-service:latest")
                .WithEnvironment("RABBITMQ_HOST", rabbitMqHost)
                .WithEnvironment("RABBITMQ_USERNAME", rabbitMqUsername)
                .WithEnvironment("RABBITMQ_PASSWORD", rabbitMqPassword)
                .WithEnvironment("USER_DB_NAME", dbNameUser)
                .WithEnvironment("DB_PORT", dbPort)
                .WithEnvironment("DB_HOST", dbHost)
                .WithEnvironment("DB_USER", dbUser)
                .WithEnvironment("DB_PASSWORD", dbPassword)
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("user")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .DependsOn(_postres)
                .DependsOn(_broker)
                .Build();

            // PostsService
            _postsService = new ContainerBuilder()
                .WithImage($"{dockerHubUsername}/mysocialnetwork-post-service:latest")
                .WithEnvironment("RABBITMQ_HOST", rabbitMqHost)
                .WithEnvironment("RABBITMQ_USERNAME", rabbitMqUsername)
                .WithEnvironment("RABBITMQ_PASSWORD", rabbitMqPassword)
                .WithEnvironment("POST_DB_NAME", dbNamePost)
                .WithEnvironment("DB_PORT", dbPort)
                .WithEnvironment("DB_HOST", dbHost)
                .WithEnvironment("DB_USER", dbUser)
                .WithEnvironment("DB_PASSWORD", dbPassword)
                .WithPortBinding(80, true)
                .WithNetwork(_networkName)
                .WithNetworkAliases("post")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .DependsOn(_postres)
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
            await _postres.StartAsync();
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
            await _postres.StopAsync();
            await _broker.StopAsync();
            await _network.DeleteAsync();
        }
    }
}