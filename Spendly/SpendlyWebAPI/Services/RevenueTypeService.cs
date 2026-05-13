using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class RevenueTypeService : ISqlRepository<ResponseRevenueTypeDto, CreateRevenueTypeDto, EditRevenueTypeDto>
    {
        private readonly SpendlyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RevenueTypeService(SpendlyDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
           => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));


        public async Task<ResponseRevenueTypeDto> CreateAsync(CreateRevenueTypeDto dto)
        {
            var revenueType = new RevenueType
            {
                Name = dto.Name,
                GroupId = dto.GroupId
            };
            _context.RevenueTypes.Add(revenueType);
            await _context.SaveChangesAsync();

            return new ResponseRevenueTypeDto
            {
                Id = revenueType.Id,
                Name = revenueType.Name,
                GroupId = dto.GroupId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var revenueType = await _context.RevenueTypes.FindAsync(id);
            if (revenueType == null) return false;
            
            int userId = GetCurrentUserId();
            bool isInGroup = await _context.UserGroups
                .AnyAsync(ug => ug.GroupId == revenueType.GroupId && ug.UserId == userId);

            if (!isInGroup)
                throw new UnauthorizedAccessException("Nemate pristup ovoj kategoriji.");



            _context.RevenueTypes.Remove(revenueType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditAsync(int id, EditRevenueTypeDto dto)
        {
            var revenueType = await _context.RevenueTypes.FindAsync(id);
            if (revenueType == null) return false;

            int userId = GetCurrentUserId();
            bool isInGroup = await _context.UserGroups
                .AnyAsync(ug => ug.GroupId == revenueType.GroupId && ug.UserId == userId);

            if (!isInGroup)
                throw new UnauthorizedAccessException("Nemate pristup ovoj kategoriji.");


            revenueType.Name = dto.Name;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ResponseRevenueTypeDto>> GetAllAsync(int? id = null)
        {
            return await _context.RevenueTypes
                .Where(rt=>rt.GroupId==id)
                 .Select(r => new ResponseRevenueTypeDto
                 {
                     Id = r.Id,
                     Name = r.Name,
                     GroupId = r.GroupId,
                 }).ToListAsync();
        }

        public async Task<ResponseRevenueTypeDto?> GetByIdAsync(int id)
        {
            return await _context.RevenueTypes
                .Where(u => u.Id == id)
                .Select(u => new ResponseRevenueTypeDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    GroupId=u.GroupId,
                }).FirstOrDefaultAsync();
        }
    }
}