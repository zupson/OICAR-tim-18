using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class BudgetService : IBudget<ResponseBudgetDto, CreateBugdetDto, EditBudgetDto>
    {
        private readonly SpendlyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BudgetService(SpendlyDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserIdFromJwt()
           => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task EnsureOwnerOfUserGroupAsync(int userGroupId)
        {
            int userId = GetCurrentUserIdFromJwt();

            var ug = await _context.UserGroups
                .FirstOrDefaultAsync(x => x.Id == userGroupId);

            if (ug == null)
                throw new KeyNotFoundException("Grupa nije pronađena.");

            var requesterMembership = await _context.UserGroups
                .FirstOrDefaultAsync(x => x.UserId == userId && x.GroupId == ug.GroupId);

            if (requesterMembership == null || requesterMembership.Role != (int)Enums.Role.Owner)
                throw new UnauthorizedAccessException("Samo vlasnik grupe može mijenjati budžet.");
        }

        private async Task EnsureOwnerOfBudgetAsync(int budgetId)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == budgetId && !b.IsDeleted);

            if (budget == null)
                throw new KeyNotFoundException("Budžet nije pronađen.");

            await EnsureOwnerOfUserGroupAsync(budget.UserGroupId);
        }

        public async Task<ResponseBudgetDto> CreateAsync(int userGroupId, CreateBugdetDto dto)
        {
            await EnsureOwnerOfUserGroupAsync(userGroupId);

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

            return await _context.Budgets
                .Include(b => b.UserGroup)
                .Where(b => b.Id == budget.Id)
                .Select(b => new ResponseBudgetDto
                {
                    Id = b.Id,
                    Amount = b.Amount,
                    Year = b.Year,
                    Month = b.Month,
                    UserGroupId = b.UserGroupId,
                    GroupId = b.UserGroup.GroupId,
                    Currency = (Enums.Currency)b.Currency
                })
                .FirstAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await EnsureOwnerOfBudgetAsync(id);

            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            if (budget == null) return false;

            budget.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditAsync(int id, EditBudgetDto dto)
        {
            await EnsureOwnerOfBudgetAsync(id);

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
            int userId = GetCurrentUserIdFromJwt();

            var myGroupIds = await _context.UserGroups
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GroupId)
                .ToListAsync();

            return await _context.Budgets
                .Include(b => b.UserGroup)
                .Where(b => !b.IsDeleted && myGroupIds.Contains(b.UserGroup.GroupId))
                .Select(b => new ResponseBudgetDto
                {
                    Id = b.Id,
                    Amount = b.Amount,
                    Year = b.Year,
                    Month = b.Month,
                    UserGroupId = b.UserGroupId,
                    GroupId = b.UserGroup.GroupId,
                    Currency = (Enums.Currency)b.Currency
                })
                .ToListAsync();
        }

        public async Task<ResponseBudgetDto?> GetByIdAsync(int id)
        {
            var budget = await _context.Budgets
                .Include(b => b.UserGroup)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
            if (budget == null) return null;

            return new ResponseBudgetDto
            {
                Id = budget.Id,
                Amount = budget.Amount,
                Year = budget.Year,
                Month = budget.Month,
                UserGroupId = budget.UserGroupId,
                GroupId = budget.UserGroup.GroupId,
                Currency = (Enums.Currency)budget.Currency
            };
        }
    }
}
