using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateCostTypeDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
    }
    public class EditCostTypeDto
    {
        [Required]
        [MaxLength(150)]
        public string? Name { get; set; }
    }
    public class ResponseCostTypeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
