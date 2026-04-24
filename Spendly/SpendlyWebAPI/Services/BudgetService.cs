using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;

namespace SpendlyWebAPI.Services
{
    public class BudgetService : IBudget<ResponseBudgetDto, CreateBugdetDto, EditBudgetDto>
    {
        private readonly SpendlyDbContext _context;
        public BudgetService(SpendlyDbContext context)
        {
            _context = context;
        }


        public async Task<ResponseBudgetDto> CreateAsync(int userGroupId, CreateBugdetDto dto)
        {
            var budget = new Budget
            {
                Amount = dto.Amount,
                Year = dto.Year,
                Month = dto.Month,
                Currency = (int)dto.Currency,
                UserGroupId = userGroupId,
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return new ResponseBudgetDto
            {
                Id = budget.Id,
                Amount = budget.Amount,
                Year = budget.Year,
                Month = budget.Month,
                UserGroupId = budget.UserGroupId,
                Currency = (Enums.Currency)budget.Currency
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

        public async Task<bool> EditAsync(int id, EditBudgetDto dto)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            if (budget == null) return false;

            budget.Amount = dto.Amount;
            budget.Year = dto.Year;
            budget.Month = dto.Month;
            budget.Currency = (int)dto.Currency;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ResponseBudgetDto>> GetAllAsync()
        {
            return await _context.Budgets
                .Where(b => !b.IsDeleted)
                .Select(b => new ResponseBudgetDto
                {
                    Id = b.Id,
                    Amount = b.Amount,
                    Year = b.Year,
                    Month = b.Month,
                    UserGroupId = b.UserGroupId,
                    Currency = (Enums.Currency)b.Currency
                })
                .ToListAsync();
        }

        public async Task<ResponseBudgetDto?> GetByIdAsync(int id)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            if (budget == null) return null;

            return new ResponseBudgetDto
            {
                Id = budget.Id,
                Amount = budget.Amount,
                Year = budget.Year,
                Month = budget.Month,
                UserGroupId = budget.UserGroupId,
                Currency = (Enums.Currency)budget.Currency
            };
        }
    }
}