using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiGateway.Tests.IntegrationTests
{
    public class RegisterIntegrationTests : IClassFixture<WebApplicationFactory<ApiGatewayEntryPoint>>
    {
        private readonly HttpClient _client;

        public RegisterIntegrationTests(WebApplicationFactory<ApiGatewayEntryPoint> factory)
        {
            _client = factory.CreateClient();
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

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", requestData);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);


            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("test@example.com", content);
        }
    }
}