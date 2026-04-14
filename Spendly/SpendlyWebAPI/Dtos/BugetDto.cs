using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateBugetDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public required decimal Amount { get; set; }

        [Range(2026, 2100)]
        public required int Year { get; set; }

        [Range(1, 12)]
        public required int Month { get; set; }
        
        public required int CurrencyId { get; set; }

        public required int UserGroupId { get; set; }
    }
    public class EditBugetDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Range(2000, 2100)]
        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        public int CurrencyId { get; set; }
    }
    public class ResponseBudgetDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int UserGroupId { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencyName { get; set; }
    }
}