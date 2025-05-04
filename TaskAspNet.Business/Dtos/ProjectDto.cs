
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TaskAspNet.Business.Dtos;

public sealed class ProjectDto
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Required(ErrorMessage = "Client is required")]
    public int ClientId { get; set; }

    [ValidateNever]
    public ClientDto? Client { get; set; }

    [Required]
    public int StatusId { get; set; }

    [ValidateNever]
    public ProjectStatusDto? Status { get; set; }

    [Required, Range(0, double.MaxValue, ErrorMessage = "Budget must be positive")]
    public decimal Budget { get; set; }

    public UploadSelectImgDto ImageData { get; set; } = new();

    public List<int> SelectedMemberIds { get; set; } = [];
    [ValidateNever]
    public List<MemberDto> Members { get; set; } = [];

    [ValidateNever]
    public string? CreatedByUserId { get; set; }
}

