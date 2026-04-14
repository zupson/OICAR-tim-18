using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateRevenueTypeDto
    {
        [MaxLength(150)]
        public required string Name { get; set; }
    }
    public class EditRevenueTypeDto
    {
        [MaxLength(150)]
        public string? Name { get; set; }
    }
    public class ResponseRevenueTypeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
