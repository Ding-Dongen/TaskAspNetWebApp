using TaskAspNet.Business.Dtos;
using X.PagedList;

namespace TaskAspNet.Business.ViewModel;

public class MemberIndexViewModel
{
    public List<MemberDto> AllMembers { get; set; } = new();
    public MemberDto CreateMember { get; set; } = new();

    public IPagedList<MemberDto> PagedMembers { get; set; } = null!;

}
