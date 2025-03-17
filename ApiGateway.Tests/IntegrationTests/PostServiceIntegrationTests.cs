using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ApiGateway.Tests.IntegrationTests.DTOs;
using Xunit;

namespace ApiGateway.Tests.IntegrationTests
{
    [Collection("Docker Network Collection")]
    public class PostServiceIntegrationTests : IClassFixture<ContainerFixture>, IClassFixture<SharedNetworkFixture>
    {
        private readonly HttpClient _client;

        public PostServiceIntegrationTests(ContainerFixture fixture)
        {
            _client = fixture.HttpClient;
        }

        [Fact]
        public async Task CreatePost_ReturnsCreatedStatus()
        {
            var requestData = new
            {
                AuthorId = Guid.NewGuid(),
                Title = "Test Post",
                Content = "This is a test post."
            };

            var response = await _client.PostAsJsonAsync("/api/posts", requestData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task ListPosts_ReturnsOkAndList()
        {
            var response = await _client.GetAsync("/api/posts");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
            Assert.NotNull(posts);
        }

        [Fact]
        public async Task UpdatePost_ReturnsCreatedStatus()
        {
            var requestData = new
            {
                AuthorId = Guid.NewGuid(),
                Title = "Test Post",
                Content = "This is a test post."
            };

            var response = await _client.PostAsJsonAsync("/api/posts", requestData);
            response.EnsureSuccessStatusCode();
            var createdPost = await response.Content.ReadFromJsonAsync<PostResponse>();

            var updateRequestData = new
            {
                Title = "Update Test Post",
                Content = "This is a test post."
            };

            var updateResponse = await _client.PutAsJsonAsync($"/api/posts/{createdPost.Id}", updateRequestData);

            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        }

        [Fact]
        public async Task DeletePost_ReturnNoContentStatus()
        {
            var requestData = new
            {
                AuthorId = Guid.NewGuid(),
                Title = "Test Post",
                Content = "This is a test post."
            };

            var response = await _client.PostAsJsonAsync("/api/posts", requestData);
            response.EnsureSuccessStatusCode();
            var createdPost = await response.Content.ReadFromJsonAsync<PostResponse>();

            var deleteRequest = await _client.DeleteAsync($"/api/posts/{createdPost.Id}");

            Assert.Equal(HttpStatusCode.NoContent, deleteRequest.StatusCode);
        }
    }
}
