using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeneratorService.Core.User.Repositories;
using GeneratorService.Core.User.Repositories.Models;
using GeneratorService.Core.User.Services;
using Moq;
using Xunit;

namespace GeneratorService.Tests;

public class UserContentServiceTests
{
    private readonly Mock<IUserContentRepository> _repositoryMock;
    private readonly UserContentService _service;

    public UserContentServiceTests()
    {
        _repositoryMock = new Mock<IUserContentRepository>();
        _service = new UserContentService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetContentAsync_ShouldReturnContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedContent = new UserContentModel { Id = Guid.NewGuid(), UserId = userId, ContentName = "Test" };
        _repositoryMock.Setup(x => x.GetByContentNameAndUserIdAsync("Test", userId)).ReturnsAsync(expectedContent);

        // Act
        var result = await _service.GetContentAsync(userId, "Test");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedContent.Id, result.Id);
    }

    [Fact]
    public async Task GetAllUserContentAsync_ShouldReturnList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedList = new List<UserContentModel> { new UserContentModel { Id = Guid.NewGuid(), UserId = userId } };
        _repositoryMock.Setup(x => x.GetAllByUserIdAsync(userId)).ReturnsAsync(expectedList);

        // Act
        var result = await _service.GetAllUserContentAsync(userId);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task CreateContentAsync_ShouldReturnFalse_WhenContentExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByContentNameAndUserIdAsync("Test", userId))
            .ReturnsAsync(new UserContentModel());

        // Act
        var result = await _service.CreateContentAsync(userId, "Test", "Code");

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserContentModel>()), Times.Never);
    }

    [Fact]
    public async Task CreateContentAsync_ShouldReturnTrue_WhenContentIsNew()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByContentNameAndUserIdAsync("Test", userId))
            .ReturnsAsync((UserContentModel?)null);
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<UserContentModel>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreateContentAsync(userId, "Test", "Code");

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(x => x.CreateAsync(It.Is<UserContentModel>(c => c.UserId == userId && c.ContentName == "Test" && c.ContentCode == "Code")), Times.Once);
    }

    [Fact]
    public async Task DeleteContentAsync_ShouldCallRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.DeleteAsync(userId, "Test")).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteContentAsync(userId, "Test");

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(x => x.DeleteAsync(userId, "Test"), Times.Once);
    }

    [Fact]
    public async Task UpdateContentAsync_ShouldReturnFalse_WhenContentDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByContentNameAndUserIdAsync("Test", userId)).ReturnsAsync((UserContentModel?)null);

        // Act
        var result = await _service.UpdateContentAsync(userId, "Test", "NewCode");

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UserContentModel>()), Times.Never);
    }

    [Fact]
    public async Task UpdateContentAsync_ShouldReturnTrue_WhenContentExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existing = new UserContentModel { Id = Guid.NewGuid(), UserId = userId, ContentName = "Test", ContentCode = "OldCode" };
        _repositoryMock.Setup(x => x.GetByContentNameAndUserIdAsync("Test", userId)).ReturnsAsync(existing);
        _repositoryMock.Setup(x => x.UpdateAsync(existing)).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateContentAsync(userId, "Test", "NewCode");

        // Assert
        Assert.True(result);
        Assert.Equal("NewCode", existing.ContentCode);
        _repositoryMock.Verify(x => x.UpdateAsync(existing), Times.Once);
    }
}
