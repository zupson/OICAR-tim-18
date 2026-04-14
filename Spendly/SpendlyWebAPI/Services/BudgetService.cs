using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;

namespace SpendlyWebAPI.Services
{
    public class BudgetService : ISqlRepository<ResponseBudgetDto, CreateBugetDto, EditBugetDto>
    {
        private readonly SpendlyContext _context;
        public BudgetService(SpendlyContext context)
        {
            _context = context;
        }

        public async Task<ResponseBudgetDto> CreateAsync(CreateBugetDto dto)
        {
            var budget = new Budget
            {
                Amount = dto.Amount,
                Year = dto.Year,
                Month = dto.Month,
                UserGroupId = dto.UserGroupId,
                CurrencyId = dto.CurrencyId
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            var currency = await _context.Currencies.FindAsync(dto.CurrencyId);

            return new ResponseBudgetDto
            {
                Id = budget.Id,
                Amount = budget.Amount,
                Year = budget.Year,
                Month = budget.Month,
                UserGroupId = budget.UserGroupId,
                CurrencyId = budget.CurrencyId,
                CurrencyCode = currency!.Code,
                CurrencyName = currency!.Name
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            if (budget == null) return false;

            budget.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditAsync(int id, EditBugetDto dto)
        {
            var budget = await _context.Budgets
                .Include(b => b.Currency)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            if (budget == null) return false;

            budget.Amount = dto.Amount;
            budget.Year = dto.Year;
            budget.Month = dto.Month;
            budget.CurrencyId = dto.CurrencyId;

            await _context.SaveChangesAsync();

            if (budget.Currency.Id != dto.CurrencyId)
                await _context.Entry(budget).Reference(b => b.Currency).LoadAsync();

            return true;
        }

        public async Task<IEnumerable<ResponseBudgetDto>> GetAllAsync()
        {
            return await _context.Budgets
                .Where(b => !b.IsDeleted)
                .Include(b => b.Currency)
                .Select(b => new ResponseBudgetDto
                {
                    Id = b.Id,
                    Amount = b.Amount,
                    Year = b.Year,
                    Month = b.Month,
                    UserGroupId = b.UserGroupId,
                    CurrencyId = b.CurrencyId,
                    CurrencyCode = b.Currency.Code,
                    CurrencyName = b.Currency.Name
                })
                .ToListAsync();
        }

        public async Task<ResponseBudgetDto?> GetByIdAsync(int id)
        {
            var budget = await _context.Budgets
                .Include(b => b.Currency)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            if (budget == null) return null;

            return new ResponseBudgetDto
            {
                Id = budget.Id,
                Amount = budget.Amount,
                Year = budget.Year,
                Month = budget.Month,
                UserGroupId = budget.UserGroupId,
                CurrencyId = budget.CurrencyId,
                CurrencyCode = budget.Currency.Code,
                CurrencyName = budget.Currency.Name
            };
        }
    }
}