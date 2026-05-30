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
            var cost = new Cost
            {
                Amount = dto.Amount,
                TransactionDate = dto.TransactionDate,
                Notes = dto.Notes,
                UserId = GetCurrentUserIdFromJwt(),
                Currency = (int)dto.Currency,
                CostTypeId = dto.CostTypeId,
                GroupId = groupId
            };

            _context.Costs.Add(cost);
            await _context.SaveChangesAsync();

            return new ResponseCostDto
            {
                Amount = dto.Amount,
                TransactionDate = dto.TransactionDate,
                Notes = dto.Notes,
                UserId = GetCurrentUserIdFromJwt(),
                Currency = dto.Currency,
                CostTypeId = dto.CostTypeId
            };
        }

       

        public async Task<bool> DeleteAsync(int id)
        {
            var cost = await _context.Costs.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (cost == null) return false;

            cost.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EditAsync(int id, EditCostDto dto)
        {
            var cost = await _context.Costs.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (cost == null) return false;

            cost.Amount = dto.Amount;
            cost.Currency = (int)dto.Currency;
            cost.CostTypeId = dto.CostTypeId;
            cost.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ResponseCostDto>> GetAllAsync()
        {
            return await _context.Costs
            .Where(c => !c.IsDeleted)
            .Select(c => new ResponseCostDto
            {
                Id = c.Id,
                Amount = c.Amount,
                TransactionDate = c.TransactionDate,
                Notes = c.Notes,
                CostTypeId = c.CostTypeId,
                Currency = (Currency)c.Currency
            })
            .ToListAsync();
        }

        public async Task<ResponseCostDto?> GetByIdAsync(int id)
        {
            return await _context.Costs
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new ResponseCostDto
            {
                Id = c.Id,
                Amount = c.Amount,
                TransactionDate = c.TransactionDate,
                Notes = c.Notes,
                CostTypeId = c.CostTypeId,
                Currency = (Currency)c.Currency
            })
            .FirstOrDefaultAsync();
        }
    }
}
