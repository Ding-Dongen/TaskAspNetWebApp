using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.ViewModel;

namespace TaskAspNet.Web.Interfaces;

public interface IMemberIndexVmFactory
{
    Task<MemberIndexViewModel> BuildAsync(int page = 1, int pageSize = 12);
    Task<MemberDto> BuildBlankDtoAsync(string fullName, string email, string userId);
    Task RehydrateLookupsAsync(MemberDto dto);
}
