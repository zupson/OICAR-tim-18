using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateGroupDto
    {
        [MaxLength(100)]
        public required string Name { get; set; } 
        public bool IsPersonal { get; set; }
    }
    public class EditGroupDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }
    }
    public class ResponseGroupDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsPersonal { get; set; }
    }
}