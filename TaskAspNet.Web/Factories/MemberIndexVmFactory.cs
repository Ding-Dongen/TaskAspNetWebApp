using Microsoft.AspNetCore.Mvc.Rendering;
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Business.ViewModel;
using TaskAspNet.Web.Interfaces;
using X.PagedList.Extensions;

namespace TaskAspNet.Web.Infrastructure.Factories;

public sealed class MemberIndexVmFactory : IMemberIndexVmFactory
{
    private readonly IMemberService _members;
    private readonly IWebHostEnvironment _env;

    public MemberIndexVmFactory(IMemberService members, IWebHostEnvironment env)
        => (_members, _env) = (members, env);

    public async Task<MemberIndexViewModel> BuildAsync(int page = 1, int pageSize = 20)
    {
        var all = (await _members.GetAllMembersAsync()).ToList();
        var titles = await _members.GetAllJobTitlesAsync();

        return new MemberIndexViewModel
        {
            PagedMembers = all.ToPagedList(page, pageSize),
            AllMembers = all,
            CreateMember = await BuildBlankDtoAsync(null, null, null)
        };
    }

    public async Task<MemberDto> BuildBlankDtoAsync(string? fullName, string? email, string? userId)
    {
        var dto = new MemberDto
        {
            Email = email,
            UserId = userId
        };

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            var parts = fullName.Split(' ', 2);
            dto.FirstName = parts.ElementAtOrDefault(0);
            dto.LastName = parts.ElementAtOrDefault(1);
        }

        await RehydrateLookupsAsync(dto);
        return dto;
    }

    public async Task RehydrateLookupsAsync(MemberDto dto)
    {
        var titles = await _members.GetAllJobTitlesAsync();
        dto.AvailableJobTitles = titles.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = t.Title
        }).ToList();

        var dir = Path.Combine(_env.WebRootPath, "images", "membericon");
        var imgs = Directory.GetFiles(dir)
                            .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                     || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                     || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                     || f.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                            .Select(Path.GetFileName)!
                            .ToList();

        dto.ImageData ??= new UploadSelectImgDto();
        dto.ImageData.PredefinedImages = imgs;
    }
}