using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Business.Services;
using TaskAspNet.Data.Interfaces;
using TaskAspNet.Data.Context;

namespace TaskAspNet.Tests;

public class ProjectServiceTests : BaseTest
{
    private readonly ProjectService _service;
    private readonly Mock<INotificationService> _notificationsMock;
    private readonly Mock<IClientService> _clientServiceMock;

    public ProjectServiceTests()
    {
        var projectRepo = ServiceProvider.GetRequiredService<IProjectRepository>();
        var clientRepo = ServiceProvider.GetRequiredService<IClientRepository>();
        var statusRepo = ServiceProvider.GetRequiredService<IProjectStatusRepository>();
        var memberRepo = ServiceProvider.GetRequiredService<IMemberRepository>();

        _notificationsMock = new Mock<INotificationService>();
        _clientServiceMock = new Mock<IClientService>();
        _clientServiceMock
            .Setup(s => s.EnsureClientAsync(It.IsAny<int?>(), It.IsAny<string?>()))
            .ReturnsAsync((int? id, string? name) => id ?? 1);

        var appCtx = ServiceProvider.GetRequiredService<AppDbContext>();
        var unitOfWork = new FakeUnitOfWork(appCtx);

        _service = new ProjectService(projectRepo, clientRepo, statusRepo, _notificationsMock.Object, memberRepo, _clientServiceMock.Object, unitOfWork);
    }

    [Fact]
    public async Task GetAllProjectsAsync_ShouldReturnSeededProject()
    {
        var projects = await _service.GetAllProjectsAsync();
        Assert.Single(projects);
        Assert.Equal("Test Project 1", projects.First().Name);
    }

    [Fact]
    public async Task AddProjectAsync_ShouldThrow_WhenInvalidStatus()
    {
        var dto = new ProjectDto { Name = "P", StatusId = 999, Client = null, StartDate = DateTime.Today, Budget = 0 };
        await Assert.ThrowsAsync<Exception>(() => _service.AddProjectAsync(dto));
    }

    [Fact]
    public async Task AddProjectAsync_ShouldAddProject_AndNotify()
    {
        var dto = new ProjectDto
        {
            Name = "P2",
            Description = "Desc",
            StatusId = 1,
            Client = new ClientDto { ClientName = "C" },
            StartDate = DateTime.Today,
            Budget = 100m
        };

        var result = await _service.AddProjectAsync(dto);

        Assert.Equal("P2", result.Name);
        _notificationsMock.Verify(n =>
            n.NotifyProjectCreatedAsync(It.IsAny<int>(), "P2", It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteProjectAsync_ShouldRemoveProject()
    {
        var initial = await _service.GetAllProjectsAsync();
        Assert.Single(initial);

        var deleted = await _service.DeleteProjectAsync(1);
        Assert.Equal(1, deleted.Id);

        var post = await _service.GetAllProjectsAsync();
        Assert.Empty(post);
    }
}
