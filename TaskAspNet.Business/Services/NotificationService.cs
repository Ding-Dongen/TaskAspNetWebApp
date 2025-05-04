using TaskAspNet.Data.Entities;
using TaskAspNet.Data.Interfaces;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Web.Interfaces;
using Microsoft.AspNetCore.Identity;
using TaskAspNet.Data.EntityIdentity;
using TaskAspNet.Business.Helper;

namespace TaskAspNet.Business.Services;

// Base from gpt4.5 but modified by me to fit the project
// It does return all unread notifications for the specified user.
// gives notifications to the user (role‑based) as well as read items when requested.
// Set a single flag for isRead.
// It also marks all notifications as read for a user.
// Creates a new notification and sends it to the user via SignalR.
// It also provides notifys to users about project creation, updates, and member changes as well as clients, created or updated.
// Also dismisses notifications for a user(all as well as a singel one.
public class NotificationService(
    INotificationRepository notificationRepository,
    INotificationHubService notificationHubService,
    INotificationTypeRepository notificationTypeRepository,
    UserManager<AppUser> userManager,
    IUnitOfWork unitOfWork) : INotificationService
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly INotificationTypeRepository _notificationTypeRepository = notificationTypeRepository; 
    private readonly INotificationHubService _notificationHubService = notificationHubService;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IUnitOfWork _uow = unitOfWork;

    public async Task<List<NotificationEntity>> GetUnreadNotificationsAsync(string userId)
    {
        return await _notificationRepository.GetUnreadNotificationsAsync(userId);
    }

    public async Task<List<NotificationEntity>> GetNotificationsForUserAsync(string userId, bool includeRead = false)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
           
            return new List<NotificationEntity>();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return await _notificationRepository.GetNotificationsForUserAndRolesAsync(userId, roles, includeRead);
    }



    public async Task MarkAsReadAsync(int notificationId)
    {
        await _uow.ExecuteAsync(() => _notificationRepository.MarkAsReadAsync(notificationId));
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _uow.ExecuteAsync(() => _notificationRepository.MarkAllAsReadAsync(userId));
    }


    public async Task CreateNotificationAsync(string title, string message, int notificationTypeId, string? createdByUserId = null, string? targetUserId = null, string? relatedEntityId = null, string? relatedEntityType = null)
    {
        var notification = new NotificationEntity
        {
            Title = title,
            Message = message,
            NotificationTypeId = notificationTypeId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            CreatedByUserId = createdByUserId,
            TargetUserId = targetUserId,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType
        };

        await _uow.ExecuteAsync(async () =>
        {
            await _notificationRepository.AddAsync(notification);
        });

        await _notificationHubService.SendNotificationAsync(notification);
    }

    public async Task NotifyProjectCreatedAsync(int projectId, string projectName, string createdByUserId)
    {
        var typeEntity = await _notificationTypeRepository.GetByNameAsync("ProjectCreated");
        if (typeEntity == null)
        {
            throw new Exception("No NotificationType found with Name='ProjectCreated'. Seed it in the DB first.");
        }

        await CreateNotificationAsync(
            "",
            $"{projectName} added",
            typeEntity.Id, 
            createdByUserId,
            targetUserId: null,
            relatedEntityId: projectId.ToString(),
            relatedEntityType: "Project"
        );
    }

    public async Task NotifyProjectUpdatedAsync(int projectId, string projectName)
    {
        var typeEntity = await _notificationTypeRepository.GetByNameAsync("ProjectUpdated");
        if (typeEntity == null)
        {
            throw new Exception("No NotificationType found with Name='ProjectUpdated'.");
        }

        await CreateNotificationAsync(
            "",
            $"{projectName} has been updated",
            typeEntity.Id,
            createdByUserId: null,        
            targetUserId: null,
            relatedEntityId: projectId.ToString(),
            relatedEntityType: "Project"
        );
    }

    public async Task NotifyMemberAddedToProjectAsync(int projectId, string projectName, string memberName, string? addedByUserId = null)
    {
        var typeEntity = await _notificationTypeRepository.GetByNameAsync("MemberAddedToProject");
        if (typeEntity == null)
        {
            throw new Exception("No NotificationType found with Name='MemberAddedToProject'.");
        }

        await CreateNotificationAsync(
            "",
            $"{memberName} has been added to project {projectName}",
            typeEntity.Id,
            addedByUserId,
            targetUserId: null,
            relatedEntityId: projectId.ToString(),
            relatedEntityType: "Project"
        );
    }

    public async Task NotifyMemberRemovedFromProjectAsync(int projectId, string projectName, string memberName, string? removedByUserId = null)
    {
        var typeEntity = await _notificationTypeRepository.GetByNameAsync("MemberRemovedFromProject");
        if (typeEntity == null)
        {
            throw new Exception("No NotificationType found with Name='MemberRemovedFromProject'.");
        }

        await CreateNotificationAsync(
            "Member Removed from Project",
            $"{memberName} has been removed from {projectName}",
            typeEntity.Id,
            removedByUserId,
            targetUserId: null,
            relatedEntityId: projectId.ToString(),
            relatedEntityType: "Project"
        );
    }

    public async Task NotifyMemberCreatedAsync(int memberId, string memberName, string createdByUserId)
    {
        
        var typeEntity = await _notificationTypeRepository.GetByNameAsync("MemberCreated");
        if (typeEntity == null)
            throw new Exception("No NotificationType found with Name='MemberCreated'. Please seed it into the DB.");

        await CreateNotificationAsync(
            title: "",
            message: $"{memberName} has been created",
            notificationTypeId: typeEntity.Id,
            createdByUserId: createdByUserId,
            targetUserId: null, 
            relatedEntityId: memberId.ToString(),
            relatedEntityType: "Member"
        );
    }

    public async Task NotifyMemberUpdatedAsync(int memberId, string memberName, string updatedByUserId)
    {
        
        var typeEntity = await _notificationTypeRepository.GetByNameAsync("MemberUpdated");
        if (typeEntity == null)
            throw new Exception("No NotificationType found with Name='MemberUpdated'. Please seed it into the DB.");

        await CreateNotificationAsync(
            title: "",
            message: $"{memberName} has been updated",
            notificationTypeId: typeEntity.Id,
            createdByUserId: updatedByUserId,
            targetUserId: null, 
            relatedEntityId: memberId.ToString(),
            relatedEntityType: "Member"
        );
    }

    public async Task NotifyClientCreatedAsync(string clientName)
    {
        var typeEntity = await _notificationTypeRepository.GetByNameAsync("ClientCreated");
        if (typeEntity == null)
            throw new Exception("No NotificationType found with Name='ClientCreated'. Please seed it into the DB.");

        await CreateNotificationAsync(
            title: "",
            message: $"'{clientName} has been created",
            notificationTypeId: typeEntity.Id,
            createdByUserId: null,
            targetUserId: null,
            relatedEntityId: null,
            relatedEntityType: "Client"
        );
    }

    public async Task NotifyClientUpdatedAsync(string clientName)
    {
        var typeEntity = await _notificationTypeRepository.GetByNameAsync("ClientUpdated");
        if (typeEntity == null)
            throw new Exception("No NotificationType found with Name='ClientUpdated'. Please seed it into the DB.");
        await CreateNotificationAsync(
            title: "",
            message: $"{clientName} has been updated",
            notificationTypeId: typeEntity.Id,
            createdByUserId: null,
            targetUserId: null,
            relatedEntityId: null,
            relatedEntityType: "Client"
        );
    }



    public async Task DismissNotificationForUserAsync(int notificationId, string userId)
    {
        await _uow.ExecuteAsync(() => _notificationRepository.DismissNotificationAsync(notificationId, userId));
    }

    public async Task DismissAllNotificationsForUserAsync(string userId)
    {
        await _uow.ExecuteAsync(() => _notificationRepository.DismissAllNotificationsForUserAsync(userId));
    }
}
