using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ApiGateway.Tests.IntegrationTests.DTOs;
using Xunit;

namespace ApiGateway.Tests.IntegrationTests
{
    [Collection("Docker Network Collection")]
    public class UserProfileServiceIntegrationTests : IClassFixture<ContainerFixture>, IClassFixture<SharedNetworkFixture>
    {
        private readonly HttpClient _client;

        public UserProfileServiceIntegrationTests(ContainerFixture fixture)
        {
            _client = fixture.HttpClient;
        }

        [Fact]
        public async Task CreateProfile_ReturnsCreatedStatus()
        {
            var requestData = new
            {
                Id = Guid.NewGuid(),
                Login = "login",
                FirstName = "first",
                LastName = "last",
                BirthDate = "1990-01-01T00:00:00Z"
            };


            var response = await _client.PostAsJsonAsync("/api/profiles", requestData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdProfile = await response.Content.ReadFromJsonAsync<ProfileResponse>();
            Assert.NotNull(createdProfile);
            Assert.Equal(requestData.Id, createdProfile!.Id);
        }

        [Fact]
        public async Task GetProfile_ReturnsOk()
        {
            var requestData = new
            {
                Id = Guid.NewGuid(),
                Login = "login",
                FirstName = "first",
                LastName = "last",
                BirthDate = "1990-01-01T00:00:00Z"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/profiles", requestData);
            if (createResponse.StatusCode == HttpStatusCode.Created)
            {
                var createdProfile = await createResponse.Content.ReadFromJsonAsync<ProfileResponse>();

                var getResponse = await _client.GetAsync($"/api/profiles/{createdProfile!.Id}");
                Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            }
            else
            {
                Assert.Fail("Failed to create profile");
            }
        }

        [Fact]
        public async Task UpdateProfile_ReturnsOk()
        {
            var requestData = new
            {
                Id = Guid.NewGuid(),
                Login = "login",
                FirstName = "first",
                LastName = "last",
                BirthDate = "1990-01-01T00:00:00Z"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/profiles", requestData);
            if (createResponse.StatusCode == HttpStatusCode.Created)
            {
                var createdProfile = await createResponse.Content.ReadFromJsonAsync<ProfileResponse>();

                var updateRequestData = new
                {
                    Login = "newLogin",
                    FirstName = "first",
                    lastName = "last",
                    BirthDate = "1990-01-01T00:00:00Z"
                };



                var updatedRequest = await _client.PutAsJsonAsync($"/api/profiles/{createdProfile!.Id}", updateRequestData);
                Assert.Equal(HttpStatusCode.NoContent, updatedRequest.StatusCode);
            }
            else
            {
                Assert.Fail("Failed to create profile");
            }
        }

        [Fact]
        public async Task DeleteProfile_ReturnsOk()
        {
            var requestData = new
            {
                Id = Guid.NewGuid(),
                Login = "login",
                FirstName = "first",
                LastName = "last",
                BirthDate = "1990-01-01T00:00:00Z"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/profiles", requestData);
            if (createResponse.StatusCode == HttpStatusCode.Created)
            {
                var createdProfile = await createResponse.Content.ReadFromJsonAsync<ProfileResponse>();

                var deleteRequest = await _client.DeleteAsync($"/api/profiles/{createdProfile!.Id}");
                Assert.Equal(HttpStatusCode.NoContent, deleteRequest.StatusCode);
            }
            else
            {
                Assert.Fail("Failed to create profile");
            }
        }
    }
}
