using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateCurrencyDto
    {
        [Required(ErrorMessage = "Currency name is required")]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Currency code is required and must be 3 characters long")]
        [MaxLength(3)]
        public required string Code { get; set; }
    }
    public class EditCurrencyDto
    {
        [Required(ErrorMessage = "Currency name is required")]
        [MaxLength(100)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Currency code is required and must be 3 characters long")]
        [MaxLength(3)]
        public string? Code { get; set; }
    }
    public class ResponseCurrencyDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
    }
}
