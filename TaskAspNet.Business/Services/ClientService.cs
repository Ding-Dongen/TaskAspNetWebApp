
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Factories;
using TaskAspNet.Business.Helper;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Data.Entities;

namespace TaskAspNet.Business.Services;

public class ClientService(IClientRepository clientRepository, INotificationService notifications, IUnitOfWork unitOfWork) : IClientService
{
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly INotificationService _notifications = notifications;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<List<ClientDto>> GetAllClientsAsync()
    {
        var entities = await _clientRepository.GetAllWithDetailsAsync();
        return ClientFactory.ToDtoList(entities);
    }

    public async Task<ClientDto?> GetClientByIdAsync(int id)
    {
        var entity = await _clientRepository.GetByIdWithDetailsAsync(id);
        return entity == null ? null : ClientFactory.ToDto(entity);
    }


    public async Task CreateClientAsync(ClientDto dto)
    {
        var client = await _unitOfWork.ExecuteAsync(async () =>
        {
            var entity = ClientFactory.FromDto(dto);
            await _clientRepository.AddAsync(entity);
            return entity;                             
        });

        try
        {
            await _notifications.NotifyClientCreatedAsync(client.ClientName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotifyClientCreatedAsync failed: {ex}");
        }
    }


    public async Task UpdateClientAsync(ClientDto dto)
    {
        var client = await _unitOfWork.ExecuteAsync(async () =>
        {
            var entity = await _clientRepository.GetByIdWithDetailsAsync(dto.Id)
                         ?? throw new Exception("Client not found.");

            ClientUpdateFactory.Update(entity, dto);
            await _clientRepository.UpdateAsync(entity);
            return entity;
        });

        try
        {
            await _notifications.NotifyClientUpdatedAsync(client.ClientName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotifyClientUpdatedAsync failed: {ex}");
        }
    }


    public async Task DeleteClientAsync(int id)
    {

        await _unitOfWork.ExecuteAsync(async () =>
        {
            var client = await _clientRepository.GetByIdWithDetailsAsync(id);
            if (client == null)
            {
                throw new Exception("Client not found.");
            }

            await _clientRepository.DeleteAsync(client);

        });
    }

    public async Task<int> EnsureClientAsync(int? clientId, string? clientName)
    {
        if (clientId is > 0)
        {
            var existing = await _clientRepository.GetByIdAsync(clientId.Value)
                           ?? throw new Exception($"Client with Id {clientId} not found.");
            return existing.Id;
        }

        if (string.IsNullOrWhiteSpace(clientName))
            throw new Exception("Client name is required.");

        var byName = await _clientRepository.GetByNameAsync(clientName);
        if (byName != null) return byName.Id;

        var created = await _clientRepository.AddAsync(
                          new ClientEntity { ClientName = clientName });

        return created.Id;
    }


}
