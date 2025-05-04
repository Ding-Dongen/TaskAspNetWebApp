
using Microsoft.AspNetCore.Mvc;
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.ViewModel;

public interface IProjectIndexVmFactory
{
    Task<ProjectIndexViewModel> BuildIndexVmAsync(ProjectDto invalidCreate);
    Task RepopulateIndexListsAsync(ProjectDto dto, Controller controller);
}
