using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateRoleDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Role name name should be between 3 and 100 characters long!")]
        public required string Name { get; set; }
    }
    public class EditRoleDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Role name name should be between 3 and 100 characters long!")]
        public string Name { get; set; }
    }

    public class ResponseRoleDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }//ostavi nullable operator jer neznam sto cu dobiti iz baze moze se dogoditi null
    }
}