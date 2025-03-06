using Microsoft.AspNetCore.Mvc;
using UserProfileService.Controllers;
using UserProfileService.Models;

namespace UserProfileService.Tests;

public class UserProfilesControllerTests
{
    [Fact]
    public async Task Get_ReturnsNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.GetInMemoryContext();
        var controller = new UserProfilesController(context);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await controller.Get(nonExistingId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_AddsProfile_AndReturnsCreatedAtAction()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.GetInMemoryContext();
        var controller = new UserProfilesController(context);
        var newProfile = new UserProfile
        {
            FirstName = "John",
            LastName = "Doe",
            BirthDate = new DateTime(1990, 1, 1),
            Bio = "Hello, world!",
            ProfilePictureUrl = "http://example.com/photo.jpg"
        };

        // Act
        var result = await controller.Create(newProfile);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var createdProfile = Assert.IsType<UserProfile>(createdResult.Value);

        // Assert
        Assert.NotEqual(Guid.Empty, createdProfile.Id);
        Assert.Equal("John", createdProfile.FirstName);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.GetInMemoryContext();
        var controller = new UserProfilesController(context);
        var existingProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Smith",
            BirthDate = new DateTime(1985, 5, 5),
            Bio = "Initial bio"
        };

        context.UserProfiles.Add(existingProfile);
        await context.SaveChangesAsync();

        // Act
        var updatedProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Johnson",
            BirthDate = existingProfile.BirthDate,
            Bio = "Updated bio"
        };

        var result = await controller.Update(existingProfile.Id, updatedProfile);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.GetInMemoryContext();
        var controller = new UserProfilesController(context);
        var existingProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Smith",
            BirthDate = new DateTime(1985, 5, 5),
            Bio = "Initial bio"
        };

        context.UserProfiles.Add(existingProfile);
        await context.SaveChangesAsync();

        // Act
        var updatedProfile = new UserProfile
        {
            Id = existingProfile.Id,
            FirstName = "Alice",
            LastName = "Johnson",
            BirthDate = existingProfile.BirthDate,
            Bio = "Updated bio"
        };

        var result = await controller.Update(existingProfile.Id, updatedProfile);

        // Assert
        Assert.IsType<NoContentResult>(result);

        var getResult = await controller.Get(existingProfile.Id);
        var okResult = Assert.IsType<OkObjectResult>(getResult);
        var fetchedProfile = Assert.IsType<UserProfile>(okResult.Value);
        Assert.Equal("Johnson", fetchedProfile.LastName);
        Assert.Equal("Updated bio", fetchedProfile.Bio);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.GetInMemoryContext();
        var controller = new UserProfilesController(context);
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
        using var context = InMemoryDbContextFactory.GetInMemoryContext();
        var controller = new UserProfilesController(context);
        var newProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            FirstName = "Bob",
            LastName = "Marley",
            BirthDate = new DateTime(1975, 2, 6),
            Bio = "Bio text"
        };

        context.UserProfiles.Add(newProfile);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.Delete(newProfile.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);

        var deletedProfile = await context.UserProfiles.FindAsync(newProfile.Id);
        Assert.Null(deletedProfile);
    }
}
