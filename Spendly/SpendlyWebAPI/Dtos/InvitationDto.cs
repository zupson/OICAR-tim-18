using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateInvitationDto
    {
        [Required]
        [EmailAddress]
        public  string Email { get; set; }
    }

    public class ResponseInvitationDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsClaimed => ClaimedAt.HasValue;
        public int GroupId { get; set; }
        public string? GroupName { get; set; } 
        public int CreatedByUserId { get; set; }
    }
}
