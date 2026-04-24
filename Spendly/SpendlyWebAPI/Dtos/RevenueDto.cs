using SpendlyWebAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class CreateRevenueDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
        public Currency Currency { get; set; }
        public int RevenueTypeId { get; set; }
    }
    public class EditRevenueDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
        public Currency Currency { get; set; }
        public int RevenueTypeId { get; set; }
    }
    public class ResponseRevenueDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
        public int UserId { get; set; }
        public Currency Currency { get; set; }
        public int RevenueTypeId { get; set; }
        public string? RevenueTypeName { get; set; }
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
    }
}