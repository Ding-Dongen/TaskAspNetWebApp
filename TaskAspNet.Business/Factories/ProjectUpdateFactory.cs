using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Services;
using TaskAspNet.Data.Entities;

namespace TaskAspNet.Business.Factories;
public static class ProjectUpdateFactory
{
    public static void UpdateEntity(ProjectEntity existing, ProjectDto dto, int finalClientId)
    {
        existing.ClientId = finalClientId;
        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.StartDate = dto.StartDate;
        existing.EndDate = dto.EndDate;
        existing.StatusId = dto.StatusId;
        existing.Budget = dto.Budget;

        if (dto.ImageData is { })
        {
            var path = ProjectServiceExtensions.NormaliseImagePath(
                           dto.ImageData.CurrentImage ?? dto.ImageData.SelectedImage);

            if (!string.IsNullOrWhiteSpace(path))
                existing.ImageUrl = path;
        }

        dto.SelectedMemberIds ??= new List<int>();

        existing.ProjectMembers.RemoveAll(pm => !dto.SelectedMemberIds.Contains(pm.MemberId));

        foreach (var id in dto.SelectedMemberIds)
        {
            if (!existing.ProjectMembers.Any(pm => pm.MemberId == id))
            {
                existing.ProjectMembers.Add(new ProjectMemberEntity
                {
                    ProjectId = existing.Id,
                    MemberId = id
                });
            }
        }
    }
}
