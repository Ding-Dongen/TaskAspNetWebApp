using TaskAspNet.Business.Dtos;
using X.PagedList;

namespace TaskAspNet.Business.ViewModel;
public class ProjectIndexViewModel
{
    public List<ProjectDto> AllProjects { get; set; } = new();
    public ProjectDto CreateProject { get; set; } = new();
    public string SelectedStatus { get; set; } = "All";
    public List<ProjectDto> FilteredProjects { get; set; } = new();
    public List<ClientDto> Clients { get; set; } = new();

    public IPagedList<ProjectDto> PagedProject { get; set; } = null!;

}
