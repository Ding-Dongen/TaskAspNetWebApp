using TaskAspNet.Business.Factories;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Data.Interfaces;
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Helper;

namespace TaskAspNet.Business.Services;

public class MemberService(IMemberRepository memberRepository, INotificationService notificationService, IUnitOfWork unitOfWork) : IMemberService
{
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IUnitOfWork _uow = unitOfWork;


    public async Task<MemberDto> AddMemberAsync(MemberDto memberDto)
    {
        var entity = await _uow.ExecuteAsync(async () =>
        {
            var member = MemberFactory.CreateEntity(memberDto);
            await _memberRepository.AddAsync(member);
            return member;                      
        });

        try
        {
            var fullName = $"{entity.FirstName} {entity.LastName}";
            await _notificationService.NotifyMemberCreatedAsync(memberId: entity.Id, memberName: fullName, createdByUserId: memberDto.UserId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotifyMemberCreatedAsync failed: {ex}");
        }

        return MemberFactory.CreateDto(entity);
    }

    public async Task<IEnumerable<MemberDto>> GetAllMembersAsync()
    {
        var members = await _memberRepository.GetMembersWithJobTitleAsync();
        return members.Select(MemberFactory.CreateDto).ToList();
    }

    public async Task<IEnumerable<MemberDto>> GetMembersByIdAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        return member is not null ? [MemberFactory.CreateDto(member)] : [];
    }


    public async Task<MemberDto> UpdateMemberAsync(int id, MemberDto memberDto)
    {
        var entity = await _uow.ExecuteAsync(async () =>
        {
            var member = await _memberRepository.GetMemberByIdWithDetailsAsync(id)
                         ?? throw new Exception("Member not found.");

            MemberUpdateFactory.UpdateEntity(member, memberDto);
            await _memberRepository.UpdateAsync(member);  
            return member;
        });

        try
        {
            var name = $"{entity.FirstName} {entity.LastName}";
            await _notificationService.NotifyMemberUpdatedAsync(memberId: entity.Id, memberName: name, updatedByUserId: memberDto.UserId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotifyMemberUpdatedAsync failed: {ex}");
        }

        return MemberFactory.CreateDto(entity);
    }

    public async Task<MemberDto> DeleteMemberAsync(int id)
    {
        MemberDto? deleted = null;

        await _uow.ExecuteAsync(async () =>
        {
            var member = await _memberRepository.GetByIdAsync(id)
                         ?? throw new Exception("Member not found.");

            await _memberRepository.DeleteAsync(member);
            deleted = MemberFactory.CreateDto(member);
        });

        return deleted!;
    }

    public async Task<List<MemberDto>> SearchMembersAsync(string searchTerm)
    {
        var entities = await _memberRepository.SearchMembersAsync(searchTerm);

        var dtos = entities.Select(m => new MemberDto
        {
            Id = m.Id,
            FirstName = m.FirstName,
            LastName = m.LastName,
            Email = m.Email,
            ImageData = new UploadSelectImgDto
            {
                CurrentImage = m.ProfileImageUrl
            }
        }).ToList();

        return dtos;
    }

    public async Task<List<JobTitleDto>> GetAllJobTitlesAsync()
    {
        var jobTitleEntities = await _memberRepository.GetAllJobTitlesAsync();
        return jobTitleEntities.Select(e => new JobTitleDto
        {
            Id = e.Id,
            Title = e.Title
        }).ToList();
    }

    public async Task<MemberDto> GetMemberByUserIdAsync(string userId)
    {
        var member = await _memberRepository.GetMemberByUserIdAsync(userId);
        return member != null ? MemberFactory.CreateDto(member) : null!;
    }

    public async Task<MemberDto?> GetMemberByIdAsync(int id)
    {
        var member = await _memberRepository.GetMemberByIdWithDetailsAsync(id);
        return member != null ? MemberFactory.CreateDto(member) : null;
    }

}
