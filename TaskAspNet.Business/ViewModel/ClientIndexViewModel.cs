
using TaskAspNet.Business.Dtos;
using X.PagedList;

namespace TaskAspNet.Business.ViewModel;

public class ClientIndexViewModel
{
    public List<ClientDto> AllMembers { get; set; } = new();
    public ClientDto CreateClient { get; set; } = new();

    public IPagedList<ClientDto> PagedClients { get; set; } = null!;


}
