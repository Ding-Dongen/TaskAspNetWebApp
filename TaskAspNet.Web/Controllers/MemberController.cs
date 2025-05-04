using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Data.EntityIdentity;
using TaskAspNet.Web.Interfaces;

namespace TaskAspNet.Web.Controllers;

[Route("Member")]
public sealed class MemberController(
        IApplicationService memberAppService,
        IMemberService memberService,
        IProjectService projectService,
        IWebHostEnvironment env,
        UserManager<AppUser> users,
        IMemberIndexVmFactory vmFactory,
        ILogger<MemberController> logger) : Controller
{
    private readonly IApplicationService _memberApp = memberAppService;
    private readonly IMemberService _memberSrv = memberService;
    private readonly IProjectService _projectSrv = projectService;
    private readonly IWebHostEnvironment _env = env;
    private readonly UserManager<AppUser> _users = users;
    private readonly IMemberIndexVmFactory _vmFactory = vmFactory;
    private readonly ILogger<MemberController> _logger = logger;

    [Authorize, HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
    {
        try
        {
            var vm = await _vmFactory.BuildAsync(page, pageSize);
            return View(vm); ;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Member Index page.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [Authorize, HttpGet("CreateMember")]
    public async Task<IActionResult> CreateMember(string fullName, string email, string userId)
    {
        try
        {
            var model = await _vmFactory.BuildBlankDtoAsync(fullName, email, userId);
            return View("~/Views/Shared/Partials/Components/Member/_CreateMemberModal.cshtml", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing CreateMember modal.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [Authorize, HttpPost("CreateMember"), ValidateAntiForgeryToken, AllowAnonymous]
    public async Task<IActionResult> CreateMember(MemberDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await _vmFactory.RehydrateLookupsAsync(dto);
                return PartialView("~/Views/Shared/Partials/Components/Member/_CreateEditMember.cshtml", dto);
            }

            var (ok, _, error) = await _memberApp.CreateMemberAsync(dto);
            if (!ok)
            {
                ModelState.AddModelError("", error ?? "Could not create member.");
                await _vmFactory.RehydrateLookupsAsync(dto);
                return PartialView("~/Views/Shared/Partials/Components/Member/_CreateEditMember.cshtml", dto);
            }

            return RedirectToAction("Index", "Project");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member.");
            ModelState.AddModelError("", "An unexpected error occurred while creating the member.");
            await _vmFactory.RehydrateLookupsAsync(dto);
            return PartialView("~/Views/Shared/Partials/Components/Member/_CreateEditMember.cshtml", dto);
        }
    }

    [Authorize, HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var dto = await _memberSrv.GetMemberByIdAsync(id);
            if (dto is null) return NotFound();

            if (string.IsNullOrWhiteSpace(dto.UserId) && !string.IsNullOrWhiteSpace(dto.Email))
            {
                dto.UserId = (await _users.FindByEmailAsync(dto.Email))?.Id;
            }

            await _vmFactory.RehydrateLookupsAsync(dto);
            return PartialView("~/Views/Shared/Partials/Components/Member/_CreateEditMember.cshtml", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Edit form for member {MemberId}.", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [Authorize, HttpPost("Edit/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MemberDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
            {
                dto.UserId = (await _users.FindByEmailAsync(dto.Email))?.Id;
                if (string.IsNullOrWhiteSpace(dto.UserId))
                {
                    ModelState.AddModelError("UserId", "Cannot determine user.");
                    await _vmFactory.RehydrateLookupsAsync(dto);
                    return PartialView("~/Views/Shared/Partials/Components/Member/_CreateEditMember.cshtml", dto);
                }
            }

            var (ok, _, error) = await _memberApp.UpdateMemberAsync(dto.Id, dto);
            if (!ok)
            {
                ModelState.AddModelError("", error ?? "Could not update member.");
                await _vmFactory.RehydrateLookupsAsync(dto);
                return PartialView("~/Views/Shared/Partials/Components/Member/_CreateEditMember.cshtml", dto);
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member {MemberId}.", dto.Id);
            ModelState.AddModelError("", "An unexpected error occurred while updating the member.");
            await _vmFactory.RehydrateLookupsAsync(dto);
            return PartialView("~/Views/Shared/Partials/Components/Member/_CreateEditMember.cshtml", dto);
        }
    }

    [Authorize(Roles = "Admin,SuperAdmin"), HttpPost("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var m = (await _memberSrv.GetMembersByIdAsync(id)).FirstOrDefault();
            if (m is null)
            {
                TempData["ErrorMessage"] = "Member not found.";
                return RedirectToAction(nameof(Index));
            }

            await _memberSrv.DeleteMemberAsync(id);

            if (await _users.FindByIdAsync(m.UserId) is { } u)
                await _users.DeleteAsync(u);

            TempData["SuccessMessage"] = $"Deleted {m.FirstName} {m.LastName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting member {MemberId}.", id);
            TempData["ErrorMessage"] = "Could not delete member.";
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin, SuperAdmin"), HttpGet("Search")]
    public async Task<IActionResult> Search(string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return Ok(new List<MemberDto>());

            var matches = await _memberSrv.SearchMembersAsync(term);
            return Ok(matches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching members with term '{Term}'.", term);
            return StatusCode(500, "An unexpected error occurred during search.");
        }
    }

    [HttpGet("GetMembers")]
    public async Task<IActionResult> GetMembers([FromQuery] int projectId)
    {
        try
        {
            if (projectId <= 0)
                return BadRequest(new { error = "Invalid project ID" });

            var members = await _projectSrv.GetProjectMembersAsync(projectId);
            if (members is null || !members.Any())
                return NotFound(new { error = "No members found for this project" });

            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching members for project {ProjectId}.", projectId);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var members = await _memberSrv.GetAllMembersAsync();
            if (members is null || !members.Any())
                return NotFound(new { error = "No members found." });

            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all members.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpGet("CompleteProfile")]
    [Authorize, AllowAnonymous]
    public async Task<IActionResult> CompleteProfile(string fullName, string email, string userId)
    {
        try
        {
            var model = await _vmFactory.BuildBlankDtoAsync(fullName, email, userId);
            return View("~/Views/Shared/Partials/Components/Member/_CreateMemberModal.cshtml", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading CompleteProfile view.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}