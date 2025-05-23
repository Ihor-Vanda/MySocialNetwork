using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ApiGateway.Tests.IntegrationTests.DTOs;
using Xunit;

namespace ApiGateway.Tests.IntegrationTests
{
    [Collection("TestsCollection")]
    public class AuthServiceIntegrationTests
    {
        private readonly ContainerFixture _fixture;
        private readonly HttpClient _client;

        public AuthServiceIntegrationTests(ContainerFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.HttpClient;
        }

        [Fact]
        public async Task Registration_ReturnsCreatedStatus()
        {
            var requestData = new
            {
                Email = "authtest@example.com",
                Password = "Password123!",
                Login = "AuthTest",
                BirthDate = "1990-01-01T00:00:00Z"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", requestData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsOkAndToken()
        {
            var registrationData = new
            {
                Email = "authlogin@example.com",
                Password = "Password123!",
                Login = "LoginTest",
                BirthDate = "1990-01-01T00:00:00Z"
            };
            var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registrationData);
            regResponse.EnsureSuccessStatusCode();

            var loginData = new
            {
                Email = "authlogin@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginResponse);
            Assert.False(string.IsNullOrWhiteSpace(loginResponse!.Token));
        }
    }
}
