

using System.ComponentModel.DataAnnotations;

namespace TaskAspNet.Business.Dtos;

public class UpdateProjectMemberDto
{
        [Required]
        public int ProjectId { get; set; }

        
        public List<int> MemberIds { get; set; } = new();
}
