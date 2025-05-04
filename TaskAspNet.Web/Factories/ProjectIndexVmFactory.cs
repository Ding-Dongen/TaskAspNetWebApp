using Microsoft.AspNetCore.Mvc;
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Business.ViewModel;
using TaskAspNet.Web.Interfaces;

namespace TaskAspNet.Web.Factories
{
    public sealed class ProjectIndexVmFactory(
        IProjectService projectSvc,
        IClientService clientSvc,
        IImageService imageSvc) : IProjectIndexVmFactory
    {
        private readonly IProjectService _projectSvc = projectSvc;
        private readonly IClientService _clientSvc = clientSvc;
        private readonly IImageService _imageSvc = imageSvc;

        public async Task<ProjectIndexViewModel> BuildIndexVmAsync(ProjectDto invalidCreate)
        {
            var all = (await _projectSvc.GetAllProjectsAsync()).ToList();

            return new ProjectIndexViewModel
            {
                AllProjects = all,
                FilteredProjects = all,
                SelectedStatus = "All",
                CreateProject = invalidCreate,
                Clients = await _clientSvc.GetAllClientsAsync(),
            };
        }

        public async Task RepopulateIndexListsAsync(ProjectDto dto, Controller controller)
        {
            dto.ImageData ??= new UploadSelectImgDto();
            dto.ImageData.PredefinedImages = _imageSvc.GetPredefined("predefined").ToList();
            controller.ViewData["Clients"] = await _clientSvc.GetAllClientsAsync();
        }
    }
}
