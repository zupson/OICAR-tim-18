using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateCostTypeDto
    {
        [MaxLength(150)]
        public required string Name { get; set; }
    }
    public class EditCostTypeDto
    {
        [MaxLength(150)]
        public string? Name { get; set; }
    }
    public class ResponseCostTypeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
