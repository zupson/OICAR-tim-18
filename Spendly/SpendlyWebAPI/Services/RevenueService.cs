using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class RevenueService : IRevenue<ResponseRevenueDto, CreateRevenueDto, EditRevenueDto>
    {
        private readonly SpendlyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RevenueService(SpendlyDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserIdFromJwt()
            => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ResponseRevenueDto> CreateAsync(CreateRevenueDto dto, int groupId)
        {
            var userId = GetCurrentUserIdFromJwt();
            

            var revenue = new Revenue
            {
                Amount = dto.Amount,
                TransactionDate = dto.TransactionDate,
                Notes = dto.Notes,
                Currency = (int)dto.Currency,
                RevenueTypeId = dto.RevenueTypeId,
                UserId = userId,
                GroupId = groupId,
                IsDeleted = false
            };          

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
                    Currency = (Enums.Currency)r.Currency,

                    RevenueTypeId = r.RevenueTypeId,
                    RevenueTypeName = r.RevenueType.Name,
                    GroupId = r.Group.Id,
                    GroupName = r.Group.Name
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
           .Include(r => r.Group)
           .FirstOrDefaultAsync();

            if (revenue is null) return false;

            revenue.Amount = dto.Amount;
            revenue.TransactionDate = dto.TransactionDate;
            revenue.Notes = dto.Notes;
            revenue.Currency = (int)dto.Currency;
            revenue.RevenueTypeId = dto.RevenueTypeId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ResponseRevenueDto>> GetAllAsync()
        {
            return await _context.Revenues
            .Where(r => r.UserId == GetCurrentUserIdFromJwt() && !r.IsDeleted)
            .Include(r => r.RevenueType)
            .Include(r => r.Group)
            .Select(r => new ResponseRevenueDto
            {
                Id = r.Id,
                Amount = r.Amount,
                TransactionDate = r.TransactionDate,
                Notes = r.Notes,
                UserId = r.UserId,
                Currency = (Enums.Currency)r.Currency,

                RevenueTypeId = r.RevenueTypeId,
                RevenueTypeName = r.RevenueType.Name,
                GroupId = r.GroupId,
                GroupName = r.Group.Name
            })
            .ToListAsync();
        }

        public async Task<ResponseRevenueDto?> GetByIdAsync(int id)
        {
            return await _context.Revenues
                .Where(r => r.Id == id && r.UserId == GetCurrentUserIdFromJwt() && !r.IsDeleted)
                .Include(r => r.RevenueType)
                .Include(r => r.Group)
                .Select(r => new ResponseRevenueDto
                {
                    Id = r.Id,
                    Amount = r.Amount,
                    TransactionDate = r.TransactionDate,
                    Notes = r.Notes,
                    UserId = r.UserId,
                    Currency = (Enums.Currency)r.Currency,

                    RevenueTypeId = r.RevenueTypeId,
                    RevenueTypeName = r.RevenueType.Name,
                    GroupId = r.GroupId,
                    GroupName = r.Group.Name
                })
                .FirstOrDefaultAsync();
        }
    }
}
