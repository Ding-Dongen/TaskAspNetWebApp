using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Factories;
using TaskAspNet.Business.Helper;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Data.Entities;
using TaskAspNet.Data.Interfaces;

namespace TaskAspNet.Business.Services;


public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly IClientRepository _clients;
    private readonly IProjectStatusRepository _statuses;
    private readonly INotificationService _notifications;
    private readonly IMemberRepository _members;
    private readonly IClientService _clientService;
    private readonly IUnitOfWork _uow;

    public ProjectService(
        IProjectRepository projectRepository,
        IClientRepository clientRepository,
        IProjectStatusRepository statusRepository,
        INotificationService notificationService,
        IMemberRepository memberRepository,
        IClientService clientService, 
        IUnitOfWork unitOfWork)
    {
        _projects = projectRepository;
        _clients = clientRepository;
        _statuses = statusRepository;
        _notifications = notificationService;
        _members = memberRepository;
        _clientService = clientService;
        _uow = unitOfWork;
    }




    public async Task<ProjectDto> AddProjectAsync(ProjectDto dto)
    {
        var entity = await _uow.ExecuteAsync(async () =>
        {
            if (!await _statuses.ExistsAsync(dto.StatusId))
                throw new Exception($"Status Id {dto.StatusId} is invalid.");

            dto.ClientId = await _clientService.EnsureClientAsync(dto.ClientId, dto.Client?.ClientName);

            var e = ProjectFactory.CreateEntity(dto);
            await _projects.AddAsync(e);
            return e;
        });

        try
        {
            await _notifications.NotifyProjectCreatedAsync(entity.Id, entity.Name, string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotifyProjectCreatedAsync failed: {ex}");
        }

        return ProjectFactory.CreateDto(entity);
    }


    public async Task<ProjectDto> UpdateProjectAsync(int id, ProjectDto dto)
    {
        var entity = await _uow.ExecuteAsync(async () =>
        {
            var project = await _projects.GetByIdAsync(id)
                         ?? throw new KeyNotFoundException("Project not found.");

            var clientId = await _clientService.EnsureClientAsync(dto.ClientId,
                                                                  dto.Client?.ClientName);

            ProjectUpdateFactory.UpdateEntity(project, dto, clientId);
            return project;                    
        });

        try
        {
            await _notifications.NotifyProjectUpdatedAsync(entity.Id, entity.Name);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotifyProjectUpdatedAsync failed: {ex}");
        }

        return ProjectFactory.CreateDto(entity);
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
    {
        var projects = await _projects.GetProjectsWithClientsAsync();
        return projects.Select(ProjectFactory.CreateDto).ToList();
    }

    public async Task<ProjectDto> DeleteProjectAsync(int id)
    {
        var entity = await _uow.ExecuteAsync(async () =>
        {
            var project = await _projects.GetByIdAsync(id)
                         ?? throw new Exception("Project does not exist.");

            await _projects.DeleteAsync(project);
            return project;                       
        });

        return ProjectFactory.CreateDto(entity);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByIdAsync(int id)
    {
        var entity = await _projects.GetByIdAsync(id);
        return entity != null
            ? new[] { ProjectFactory.CreateDto(entity) }
            : Enumerable.Empty<ProjectDto>();
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var entity = await _projects.GetProjectByIdAsync(id);
        return entity != null ? ProjectFactory.CreateDto(entity) : null;
    }

    public async Task<List<MemberDto>> GetProjectMembersAsync(int projectId)
    {
        var project = await _projects.GetProjectByIdAsync(projectId);
        return project == null
            ? new List<MemberDto>()
            : project.ProjectMembers.Select(pm => MemberFactory.CreateDto(pm.Member)).ToList();
    }

    // Base done by me but gtp4.5 done the rest
    // Getting the display names of members added and removed and set the projectname
    // Wrap the DB call in a transactional management
    // Retrieving the projects with current members
    // Creating project name for notifications 
    // Retrieving the member ID that is assigned to a prpject
    // Creating lists to add and remove members
    // Loops through the members to add and remove and looks up members name for notifications and adds the results to the DB
    // Last 2 loops is to send notifications for the members added and removed

    public async Task UpdateProjectMembersAsync(int projectId, List<int> memberIds)
    {
        var added = new List<string>();   
        var removed = new List<string>();
        string? projectName = null;

        await _uow.ExecuteAsync(async () =>
        {
            var project = await _projects.GetProjectByIdAsync(projectId)
                         ?? throw new KeyNotFoundException("Project not found");

            projectName = project.Name;

            var currentIds = project.ProjectMembers.Select(pm => pm.MemberId).ToList();
            var membersToAdd = memberIds.Except(currentIds).ToList();
            var membersToRemove = currentIds.Except(memberIds).ToList();

            foreach (var id in membersToAdd)
            {
                project.ProjectMembers.Add(new ProjectMemberEntity { ProjectId = projectId, MemberId = id });

                var m = await _members.GetByIdAsync(id);
                if (m != null) added.Add($"{m.FirstName} {m.LastName}");
            }

            foreach (var id in membersToRemove)
            {
                var m = await _members.GetByIdAsync(id);
                if (m != null) removed.Add($"{m.FirstName} {m.LastName}");
            }

            project.ProjectMembers.RemoveAll(pm => membersToRemove.Contains(pm.MemberId));
            await _projects.UpdateAsync(project);
        });

        foreach (var name in added)
            await _notifications.NotifyMemberAddedToProjectAsync(projectId, projectName!, name);

        foreach (var name in removed)
            await _notifications.NotifyMemberRemovedFromProjectAsync(projectId, projectName!, name);
    }


}

// Created with  chatGPT 4o
// Helper for images
// returns null if there is no image
// keeps data‑URIs unchanged
// turns any absolute URL to relative path
// leaves every other string untouched

internal static class ProjectServiceExtensions
{
    public static string? NormaliseImagePath(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) return raw;

        var idx = raw.IndexOf("/images/", StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? raw[idx..] : raw;
    }
}