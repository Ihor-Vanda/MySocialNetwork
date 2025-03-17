using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Xunit;

namespace ApiGateway.Tests.IntegrationTests
{
    public class SharedNetworkFixture : IAsyncLifetime
    {
        private readonly INetwork network;
        public string NetworkName { get; } = "shared-test-network";

        public SharedNetworkFixture()
        {
            network = new NetworkBuilder()
                .WithName(NetworkName)
                .Build();
        }

        public async Task InitializeAsync()
        {
            try
            {
                await network.CreateAsync();
            }
            catch (Exception) { }
        }

        public async Task DisposeAsync()
        {
            await network.DeleteAsync();
        }
    }
}
