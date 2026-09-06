using System;
using System.Threading.Tasks;
using GeneratorService.Core.User.Repositories;
using GeneratorService.Core.User.Repositories.Models;
using GeneratorService.Core.User.Services;
using GeneratorService.Core.User.Services.Subservices;
using Moq;
using Xunit;

namespace GeneratorService.Tests;

public class UserProfileServiceTests
{
    private readonly Mock<IUserProfileRepository> _repositoryMock;
    private readonly Mock<IUserProfileNameGeneratorService> _nameGeneratorMock;
    private readonly UserProfileService _service;

    public UserProfileServiceTests()
    {
        _repositoryMock = new Mock<IUserProfileRepository>();
        _nameGeneratorMock = new Mock<IUserProfileNameGeneratorService>();
        _service = new UserProfileService(_repositoryMock.Object, _nameGeneratorMock.Object);
    }

    [Fact]
    public async Task CreateProfileAsync_ShouldCreateWithDefaultValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _nameGeneratorMock.Setup(x => x.GenerateRandomName()).Returns("GeneratedName");
        
        UserProfileModel? createdProfile = null;
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<UserProfileModel>()))
            .Callback<UserProfileModel>(p => createdProfile = p)
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateProfileAsync(userId);

        // Assert
        Assert.NotNull(createdProfile);
        Assert.Equal(userId, createdProfile.Id);
        Assert.Equal("GeneratedName", createdProfile.Name);
        Assert.Equal(10, createdProfile.Credits);
        Assert.Equal(SubscriptionTier.Free, createdProfile.SubscriptionTier);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserProfileModel>()), Times.Once);
    }

    [Fact]
    public async Task HasEnoughCreditsAsync_ShouldReturnTrue_WhenEnterprise()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new UserProfileModel { Id = userId, SubscriptionTier = SubscriptionTier.Enterprise, Credits = 0 });

        // Act
        var result = await _service.HasEnoughCreditsAsync(userId, 500);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasEnoughCreditsAsync_ShouldReturnTrue_WhenEnoughCredits()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new UserProfileModel { Id = userId, SubscriptionTier = SubscriptionTier.Free, Credits = 5 });

        // Act
        var result = await _service.HasEnoughCreditsAsync(userId, 5);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasEnoughCreditsAsync_ShouldReturnFalse_WhenNotEnoughCredits()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new UserProfileModel { Id = userId, SubscriptionTier = SubscriptionTier.Free, Credits = 4 });

        // Act
        var result = await _service.HasEnoughCreditsAsync(userId, 5);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeductCreditsAsync_ShouldDeduct_WhenFreeTier()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new UserProfileModel { Id = userId, SubscriptionTier = SubscriptionTier.Free, Credits = 10 };
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(profile);

        // Act
        await _service.DeductCreditsAsync(userId, 3);

        // Assert
        Assert.Equal(7, profile.Credits);
        _repositoryMock.Verify(x => x.UpdateAsync(profile), Times.Once);
    }

    [Fact]
    public async Task DeductCreditsAsync_ShouldNotDeduct_WhenEnterpriseTier()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new UserProfileModel { Id = userId, SubscriptionTier = SubscriptionTier.Enterprise, Credits = 10 };
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(profile);

        // Act
        await _service.DeductCreditsAsync(userId, 5);

        // Assert
        Assert.Equal(10, profile.Credits);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UserProfileModel>()), Times.Never);
    }

    [Fact]
    public async Task DeductCreditsAsync_ShouldThrow_WhenNotEnoughCredits()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new UserProfileModel { Id = userId, SubscriptionTier = SubscriptionTier.Free, Credits = 2 };
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(profile);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.DeductCreditsAsync(userId, 5));
    }

    [Fact]
    public async Task ChangeSubscriptionTierAsync_ShouldAddCredits_WhenChangingToPro()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new UserProfileModel { Id = userId, SubscriptionTier = SubscriptionTier.Free, Credits = 10 };
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(profile);

        // Act
        await _service.ChangeSubscriptionTierAsync(userId, SubscriptionTier.Pro);

        // Assert
        Assert.Equal(SubscriptionTier.Pro, profile.SubscriptionTier);
        Assert.Equal(110, profile.Credits);
        _repositoryMock.Verify(x => x.UpdateAsync(profile), Times.Once);
    }
    
    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new UserProfileModel { Id = userId, Name = "OldName", AvatarUrl = "OldAvatar" };
        _repositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(profile);

        // Act
        await _service.UpdateProfileAsync(userId, "NewName", "NewAvatar");

        // Assert
        Assert.Equal("NewName", profile.Name);
        Assert.Equal("NewAvatar", profile.AvatarUrl);
        _repositoryMock.Verify(x => x.UpdateAsync(profile), Times.Once);
    }
}
