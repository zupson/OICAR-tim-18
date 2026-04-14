namespace SpendlyWebAPI.Dtos
{
    public class CreateUserGroupDto
    {
        public required int GroupId { get; set; }
        public int? InvitationId { get; set; }
    }

    public class ResponseUserGroupDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; } 
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public DateTime JoinedAt { get; set; }
        public int? InvitationId { get; set; }
    }
}