using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class RevenueService : ISqlRepository<ResponseRevenueDto, CreateRevenueDto, EditRevenueDto>
    {
        private readonly SpendlyContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RevenueService(SpendlyContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserIdFromJwt()
            => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ResponseRevenueDto> CreateAsync(CreateRevenueDto dto)
        {
            var revenue = new Revenue
            {
                Amount = dto.Amount,
                TransactionDate = dto.TransactionDate,
                Notes = dto.Notes,
                CurrencyId = dto.CurrencyId,
                RevenueTypeId = dto.RevenueTypeId,
                UserId = GetCurrentUserIdFromJwt(),
                IsDeleted = false
            };

            if (dto.GroupIds is { Count: > 0 })
            {
                revenue.Groups = await _context.Groups
                    .Where(g => dto.GroupIds.Contains(g.Id))
                    .ToListAsync();
            }

            _context.Revenues.Add(revenue);
            await _context.SaveChangesAsync();

            return await _context.Revenues
                .Where(r => r.Id == revenue.Id)
                .Select(r => new ResponseRevenueDto
                {
                    Id = r.Id,
                    Amount = r.Amount,
                    TransactionDate = r.TransactionDate,
                    Notes = r.Notes,
                    UserId = r.UserId,
                    CurrencyId = r.CurrencyId,
                    CurrencyCode = r.Currency.Code,
                    CurrencyName = r.Currency.Name,
                    RevenueTypeId = r.RevenueTypeId,
                    RevenueTypeName = r.RevenueType.Name,
                    Groups = r.Groups.Select(g => g.Name).ToList()
                })
                .FirstAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var revenue = await _context.Revenues
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == GetCurrentUserIdFromJwt() && !r.IsDeleted);

            if (revenue is null) return false;

            revenue.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EditAsync(int id, EditRevenueDto dto)
        {
            var revenue = await _context.Revenues
           .Where(r => r.Id == id && r.UserId == GetCurrentUserIdFromJwt() && !r.IsDeleted)
           .Include(r => r.Groups)
           .FirstOrDefaultAsync();

            if (revenue is null) return false;

            revenue.Amount = dto.Amount;
            revenue.TransactionDate = dto.TransactionDate;
            revenue.Notes = dto.Notes;
            revenue.CurrencyId = dto.CurrencyId;
            revenue.RevenueTypeId = dto.RevenueTypeId;

            if (dto.GroupIds is not null)
            {
                revenue.Groups = await _context.Groups
                    .Where(g => dto.GroupIds.Contains(g.Id))
                    .ToListAsync();
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ResponseRevenueDto>> GetAllAsync()
        {
            return await _context.Revenues
            .Where(r => r.UserId == GetCurrentUserIdFromJwt() && !r.IsDeleted)
            .Include(r => r.Currency)
            .Include(r => r.RevenueType)
            .Include(r => r.Groups)
            .Select(r => new ResponseRevenueDto
            {
                Id = r.Id,
                Amount = r.Amount,
                TransactionDate = r.TransactionDate,
                Notes = r.Notes,
                UserId = r.UserId,
                CurrencyId = r.CurrencyId,
                CurrencyCode = r.Currency.Code,
                CurrencyName = r.Currency.Name,
                RevenueTypeId = r.RevenueTypeId,
                RevenueTypeName = r.RevenueType.Name,
                Groups = r.Groups.Select(g => g.Name).ToList()
            })
            .ToListAsync();
        }

        public async Task<ResponseRevenueDto?> GetByIdAsync(int id)
        {
            return await _context.Revenues
                .Where(r => r.UserId == GetCurrentUserIdFromJwt() && !r.IsDeleted)
                .Include(r => r.Currency)
                .Include(r => r.RevenueType)
                .Include(r => r.Groups)
                .Select(r => new ResponseRevenueDto
                {
                    Id = r.Id,
                    Amount = r.Amount,
                    TransactionDate = r.TransactionDate,
                    Notes = r.Notes,
                    UserId = r.UserId,
                    CurrencyId = r.CurrencyId,
                    CurrencyCode = r.Currency.Code,
                    CurrencyName = r.Currency.Name,
                    RevenueTypeId = r.RevenueTypeId,
                    RevenueTypeName = r.RevenueType.Name,
                    Groups = r.Groups.Select(g => g.Name).ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}
