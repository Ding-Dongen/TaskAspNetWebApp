using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Business.Services;
using TaskAspNet.Data.Context;

namespace TaskAspNet.Tests;

public class ClientServiceTests : BaseTest
{
    private readonly ClientService _service;
    private readonly Mock<INotificationService> _notificationsMock;

    public ClientServiceTests()
    {
        var clientRepo = ServiceProvider.GetRequiredService<IClientRepository>();
        _notificationsMock = new Mock<INotificationService>();

        var appCtx = ServiceProvider.GetRequiredService<AppDbContext>();
        var unitOfWork = new FakeUnitOfWork(appCtx);

        _service = new ClientService(clientRepo, _notificationsMock.Object, unitOfWork);
    }

    [Fact]
    public async Task GetAllClientsAsync_ShouldReturnSeededClients()
    {
        var clients = await _service.GetAllClientsAsync();
        Assert.NotEmpty(clients);
        Assert.Equal(2, clients.Count);
    }

    [Fact]
    public async Task GetClientByIdAsync_ShouldReturnClient_WhenExists()
    {
        var client = await _service.GetClientByIdAsync(1);
        Assert.NotNull(client);
        Assert.Equal(1, client.Id);
    }

    [Fact]
    public async Task GetClientByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var client = await _service.GetClientByIdAsync(999);
        Assert.Null(client);
    }

    [Fact]
    public async Task CreateClientAsync_ShouldAddClient_AndNotify()
    {
        var dto = new ClientDto { ClientName = "New Client" };
        await _service.CreateClientAsync(dto);

        var all = await _service.GetAllClientsAsync();
        Assert.Contains(all, c => c.ClientName == "New Client");

        _notificationsMock.Verify(n => n.NotifyClientCreatedAsync("New Client"), Times.Once);
    }

    [Fact]
    public async Task UpdateClientAsync_ShouldModifyClient_AndNotify()
    {
        var existing = await _service.GetClientByIdAsync(1);
        existing.ClientName = "Updated Name";

        await _service.UpdateClientAsync(existing);

        var updated = await _service.GetClientByIdAsync(1);
        Assert.Equal("Updated Name", updated.ClientName);

        _notificationsMock.Verify(n => n.NotifyClientUpdatedAsync("Updated Name"), Times.Once);
    }

    [Fact]
    public async Task DeleteClientAsync_ShouldRemoveClient_WhenExists()
    {
        await _service.DeleteClientAsync(1);
        var client = await _service.GetClientByIdAsync(1);
        Assert.Null(client);
    }
}
