
using Microsoft.AspNetCore.Mvc;
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Business.ViewModel;
using TaskAspNet.Web.Interfaces;
using X.PagedList.Extensions;

namespace TaskAspNet.Web.Controllers;

public class ProjectController(IProjectService projectService,
                         IClientService clientService,
                         IImageService imageService,
                         ILogger<ProjectController> logger,
                         IProjectIndexVmFactory vmFactory) : Controller
{
    private readonly IProjectService _projectSvc = projectService;
    private readonly IClientService _clientSvc = clientService;
    private readonly IImageService _imageSvc = imageService;
    private readonly ILogger<ProjectController> _log = logger;
    private readonly IProjectIndexVmFactory _vmFactory = vmFactory;


    [HttpGet]
    public async Task<IActionResult> Index(string status = "All", int page = 1, int pageSize = 20)
    {
        try
        {
            var all = (await _projectSvc.GetAllProjectsAsync()).ToList();
            var filtered = status switch
            {
                "Started" => all.Where(p => p.StatusId == 1).ToList(),
                "Completed" => all.Where(p => p.StatusId == 2).ToList(),
                _ => all
            };

            var viewModel = new ProjectIndexViewModel
            {
                AllProjects = all,
                FilteredProjects = filtered,
                SelectedStatus = status,
                PagedProject = filtered.ToPagedList(page, pageSize),
                CreateProject = new ProjectDto
                {
                    ImageData = new UploadSelectImgDto
                    {
                        PredefinedImages = _imageSvc.GetPredefined("predefined").ToList()
                    }
                }
            };

            ViewData["Clients"] = await _clientSvc.GetAllClientsAsync();
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error loading Project Index.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectDto dto)
    {
        _log.LogInformation("Create: {@Dto}", dto);

        if (!ModelState.IsValid)
        {
            _log.LogWarning("Invalid ModelState on Create");
            await RepopulateIndexListsAsync(dto);
            return View("Index", await BuildIndexVmAsync(dto));
        }

        dto.ImageData.CurrentImage =
            await _imageSvc.UploadAsync(dto.ImageData.UploadedImage, "projects")
            ?? (dto.ImageData.SelectedImage is { Length: > 0 }
                    ? $"/images/predefined/{dto.ImageData.SelectedImage}"
                    : dto.ImageData.CurrentImage ?? "/images/default.png");

        try
        {
            await _projectSvc.AddProjectAsync(dto);
            TempData["SuccessMessage"] = "Project created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Create failed");
            ModelState.AddModelError("", "Could not save project.");
            await RepopulateIndexListsAsync(dto);
            return View("Index", await BuildIndexVmAsync(dto));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _projectSvc.DeleteProjectAsync(id);
            TempData[deleted == null ? "ErrorMessage" : "SuccessMessage"] =
                deleted == null ? "Project not found or already deleted."
                                : $"Deleted project: {deleted.Name}";
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error deleting project {ProjectId}.", id);
            TempData["ErrorMessage"] = "Could not delete project.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var dto = (await _projectSvc.GetProjectsByIdAsync(id)).FirstOrDefault();
            if (dto == null) return NotFound();

            dto.ImageData ??= new UploadSelectImgDto();
            dto.ImageData.PredefinedImages = _imageSvc.GetPredefined("predefined").ToList();

            ViewData["Clients"] = await _clientSvc.GetAllClientsAsync();
            return PartialView("~/Views/Shared/Partials/Components/Project/_CreateEditProject.cshtml", dto);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error loading Edit view for project {ProjectId}.", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProjectDto dto)
    {
        _log.LogInformation("Editing Project ID: {ProjectId}", dto.Id);

        if (!ModelState.IsValid)
            return View("~/Views/Shared/Partials/Components/Project/_CreateEditProject.cshtml", dto);

        dto.ImageData.CurrentImage =
            await _imageSvc.UploadAsync(dto.ImageData.UploadedImage, "projects")
            ?? (dto.ImageData.SelectedImage is { Length: > 0 }
                    ? $"/images/predefined/{dto.ImageData.SelectedImage}"
                    : dto.ImageData.CurrentImage ?? "/images/default.png");

        try
        {
            var updated = await _projectSvc.UpdateProjectAsync(dto.Id, dto);
            if (updated == null)
            {
                ModelState.AddModelError("", "Could not update project.");
                return View("~/Views/Shared/Partials/Components/Project/_CreateEditProject.cshtml", dto);
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error updating project {ProjectId}.", dto.Id);
            ModelState.AddModelError("", "An unexpected error occurred while updating the project.");
            return View("~/Views/Shared/Partials/Components/Project/_CreateEditProject.cshtml", dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateMembers(UpdateProjectMemberDto dto)
    {
        dto.MemberIds ??= new List<int>();

        if (dto.ProjectId <= 0)
        {
            TempData["ErrorMessage"] = "Invalid project.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _projectSvc.UpdateProjectMembersAsync(dto.ProjectId, dto.MemberIds);
            TempData["SuccessMessage"] = "Project members updated successfully!";
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error updating members for project {ProjectId}.", dto.ProjectId);
            TempData["ErrorMessage"] = "Could not update project members.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<ProjectIndexViewModel> BuildIndexVmAsync(ProjectDto invalidCreate)
    {
        var all = (await _projectSvc.GetAllProjectsAsync()).ToList();

        return new ProjectIndexViewModel
        {
            AllProjects = all,
            FilteredProjects = all,
            SelectedStatus = "All",
            CreateProject = invalidCreate
        };
    }

    private async Task RepopulateIndexListsAsync(ProjectDto dto)
    {
        dto.ImageData ??= new UploadSelectImgDto();
        dto.ImageData.PredefinedImages = _imageSvc.GetPredefined("predefined").ToList();
        ViewData["Clients"] = await _clientSvc.GetAllClientsAsync();
    }
}