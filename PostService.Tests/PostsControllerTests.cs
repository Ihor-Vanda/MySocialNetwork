using Microsoft.AspNetCore.Mvc;
using PostService.Controllers;
using PostService.DTOs;
using PostService.Models;

namespace PostService.Tests;

public class PostsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoPostsExist()
    {
        // Arrange
        using var context = InMemoryPostDbContextFactory.GetInMemoryContext();
        var controller = new PostsController(context);

        // Act
        var result = await controller.GetAll();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var posts = Assert.IsAssignableFrom<IEnumerable<Post>>(okResult.Value);

        // Assert
        Assert.Empty(posts);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        using var context = InMemoryPostDbContextFactory.GetInMemoryContext();
        var controller = new PostsController(context);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await controller.Get(nonExistingId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_AddsPost_AndReturnsCreatedAtAction()
    {
        // Arrange
        using var context = InMemoryPostDbContextFactory.GetInMemoryContext();
        var controller = new PostsController(context);
        var newPost = new Post
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test Post",
            Content = "This is a test post.",
            MediaUrl = "http://example.com/media.jpg"
        };

        // Act
        var result = await controller.Create(newPost);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var createdPost = Assert.IsType<Post>(createdResult.Value);

        // Assert
        Assert.NotEqual(Guid.Empty, createdPost.Id);
        Assert.Equal("Test Post", createdPost.Title);
        Assert.Equal("This is a test post.", createdPost.Content);
        Assert.NotEqual(default(DateTime), createdPost.CreatedAt);
    }

    [Fact]
    public async Task Update_ReturnsNotFoundRequest_WhenIdMismatch()
    {
        // Arrange
        using var context = InMemoryPostDbContextFactory.GetInMemoryContext();
        var controller = new PostsController(context);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Title = "Original Title",
            Content = "Original Content",
            CreatedAt = DateTime.UtcNow
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var updatedPost = new PostUpdateDTO
        {
            Title = "Updated Title",
            Content = "Updated Content",
            MediaUrl = ""
        };

        var result = await controller.Update(Guid.NewGuid(), updatedPost);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        using var context = InMemoryPostDbContextFactory.GetInMemoryContext();
        var controller = new PostsController(context);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Title = "Original Title",
            Content = "Original Content",
            CreatedAt = DateTime.UtcNow
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var updatedPost = new PostUpdateDTO
        {
            Title = "Updated Title",
            Content = "Updated Content",
            MediaUrl = "http://example.com/updated.jpg"
        };

        var result = await controller.Update(post.Id, updatedPost);

        // Assert
        Assert.IsType<NoContentResult>(result);

        var getResult = await controller.Get(post.Id);
        var okResult = Assert.IsType<OkObjectResult>(getResult);
        var fetchedPost = Assert.IsType<Post>(okResult.Value);
        Assert.Equal("Updated Title", fetchedPost.Title);
        Assert.Equal("Updated Content", fetchedPost.Content);
        Assert.Equal("http://example.com/updated.jpg", fetchedPost.MediaUrl);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        using var context = InMemoryPostDbContextFactory.GetInMemoryContext();
        var controller = new PostsController(context);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await controller.Delete(nonExistingId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        using var context = InMemoryPostDbContextFactory.GetInMemoryContext();
        var controller = new PostsController(context);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Title = "Post to Delete",
            Content = "Content",
            CreatedAt = DateTime.UtcNow
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.Delete(post.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);

        var deletedPost = await context.Posts.FindAsync(post.Id);
        Assert.Null(deletedPost);
    }
}
