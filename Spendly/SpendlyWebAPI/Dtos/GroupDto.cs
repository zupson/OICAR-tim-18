using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateGroupDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } 
        public bool IsPersonal { get; set; }
    }
    public class EditGroupDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
    public class ResponseGroupDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsPersonal { get; set; }
    }
}