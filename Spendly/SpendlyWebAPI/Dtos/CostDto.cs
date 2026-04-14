using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateCostDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public required decimal Amount { get; set; }

        public required DateTime TransactionDate { get; set; }

        public string? Notes { get; set; }

        public required int CurrencyId { get; set; }

        public required int CostTypeId { get; set; }

        public List<int>? GroupIds { get; set; }
    }

    public class EditCostDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; }

        public string? Notes { get; set; }

        public int CurrencyId { get; set; }

        public int CostTypeId { get; set; }

        public List<int>? GroupIds { get; set; }
    }

    public class ResponseCostDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
        public int UserId { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencyName { get; set; }
        public int CostTypeId { get; set; }
        public string? CostTypeName { get; set; }
        public List<string> Groups { get; set; } = new();
    }
}