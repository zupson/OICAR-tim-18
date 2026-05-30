namespace SpendlyWebAPI.Dtos
{
    public class ResponseUserGroupDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}