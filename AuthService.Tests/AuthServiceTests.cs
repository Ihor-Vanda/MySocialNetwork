using Microsoft.AspNetCore.Identity;
using Moq;

namespace AuthService.Tests;

public class AuthServiceTests
{
    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AuthService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> RegisterUser(RegisterModel model)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            return result;
        }
    }

    [Fact]
    public async Task RegisterUser_WithValidModel_ReturnsSuccessResult()
    {
        // Arrange
        var registerModel = new RegisterModel { Email = "test@example.com", Password = "Password123!" };

        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        var userManagerMock = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var authService = new AuthService(userManagerMock.Object);

        // Act
        var result = await authService.RegisterUser(registerModel);

        // Assert
        Assert.True(result.Succeeded);
        userManagerMock.Verify(um => um.CreateAsync(It.Is<IdentityUser>(
            u => u.Email == registerModel.Email && u.UserName == registerModel.Email),
            registerModel.Password), Times.Once);
    }
}
