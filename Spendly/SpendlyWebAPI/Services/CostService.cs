using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Enums;
using SpendlyWebAPI.Models;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class CostService : ICost<ResponseCostDto, CreateCostDto, EditCostDto>
    {
        private readonly SpendlyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CostService(SpendlyDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        private int GetCurrentUserIdFromJwt()
           => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));



        public async Task<ResponseCostDto> CreateAsync(CreateCostDto dto, int groupId)
        {
            var userId = GetCurrentUserIdFromJwt();

            var cost = new Cost
            {
                Amount = dto.Amount,
                TransactionDate = dto.TransactionDate,
                Notes = dto.Notes,
                UserId = userId,
                Currency = (int)dto.Currency,
                CostTypeId = dto.CostTypeId,
                GroupId = groupId,
                IsDeleted = false
            };

            _context.Costs.Add(cost);
            await _context.SaveChangesAsync();

            return await _context.Costs
                .Where(c => c.Id == cost.Id)
                .Select(c => new ResponseCostDto
                {
                    Id = c.Id,
                    Amount = c.Amount,
                    TransactionDate = c.TransactionDate,
                    Notes = c.Notes,
                    UserId = c.UserId,
                    Currency = (Currency)c.Currency,
                    CostTypeId = c.CostTypeId,
                    CostTypeName = c.CostType.Name,
                    GroupId = c.Group.Id,
                    GroupName = c.Group.Name
                })
                .FirstAsync();
        }



        public async Task<bool> DeleteAsync(int id)
        {
            var cost = await _context.Costs
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == GetCurrentUserIdFromJwt() && !c.IsDeleted);
            if (cost == null) return false;

            cost.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EditAsync(int id, EditCostDto dto)
        {
            var cost = await _context.Costs
                .Where(c => c.Id == id && c.UserId == GetCurrentUserIdFromJwt() && !c.IsDeleted)
                .Include(c => c.Group)
                .FirstOrDefaultAsync();

            if (cost == null) return false;

            cost.Amount = dto.Amount;
            cost.Currency = (int)dto.Currency;
            cost.CostTypeId = dto.CostTypeId;
            cost.Notes = dto.Notes;
            cost.TransactionDate = dto.TransactionDate;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ResponseCostDto>> GetAllAsync()
        {
            return await _context.Costs
            .Where(c => c.UserId == GetCurrentUserIdFromJwt() && !c.IsDeleted)
            .Include(c => c.CostType)
            .Include(c => c.Group)
            .Select(c => new ResponseCostDto
            {
                Id = c.Id,
                Amount = c.Amount,
                TransactionDate = c.TransactionDate,
                Notes = c.Notes,
                UserId = c.UserId,
                Currency = (Currency)c.Currency,
                CostTypeId = c.CostTypeId,
                CostTypeName = c.CostType.Name,
                GroupId = c.GroupId,
                GroupName = c.Group.Name
            })
            .ToListAsync();
        }

        // Returns the combined costs of EVERY member of the given group.
        // Used by the shared/family budget so one member's spending is visible to all.
        public async Task<IEnumerable<ResponseCostDto>> GetAllByGroupAsync(int groupId)
        {
            var userId = GetCurrentUserIdFromJwt();

            var isMember = await _context.UserGroups
                .AnyAsync(ug => ug.GroupId == groupId && ug.UserId == userId);
            if (!isMember)
                throw new UnauthorizedAccessException("Niste član ove grupe.");

            var memberIds = await _context.UserGroups
                .Where(ug => ug.GroupId == groupId)
                .Select(ug => ug.UserId)
                .Distinct()
                .ToListAsync();

            return await _context.Costs
                .Where(c => memberIds.Contains(c.UserId) && !c.IsDeleted)
                .Include(c => c.CostType)
                .Include(c => c.Group)
                .Select(c => new ResponseCostDto
                {
                    Id = c.Id,
                    Amount = c.Amount,
                    TransactionDate = c.TransactionDate,
                    Notes = c.Notes,
                    UserId = c.UserId,
                    Currency = (Currency)c.Currency,
                    CostTypeId = c.CostTypeId,
                    CostTypeName = c.CostType.Name,
                    GroupId = c.GroupId,
                    GroupName = c.Group.Name
                })
                .ToListAsync();
        }

        public async Task<ResponseCostDto?> GetByIdAsync(int id)
        {
            return await _context.Costs
                .Where(c => c.Id == id && c.UserId == GetCurrentUserIdFromJwt() && !c.IsDeleted)
                .Include(c => c.CostType)
                .Include(c => c.Group)
                .Select(c => new ResponseCostDto
                {
                    Id = c.Id,
                    Amount = c.Amount,
                    TransactionDate = c.TransactionDate,
                    Notes = c.Notes,
                    UserId = c.UserId,
                    Currency = (Currency)c.Currency,
                    CostTypeId = c.CostTypeId,
                    CostTypeName = c.CostType.Name,
                    GroupId = c.GroupId,
                    GroupName = c.Group.Name
                })
                .FirstOrDefaultAsync();
        }
    }
}
