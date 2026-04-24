using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;

namespace SpendlyWebAPI.Services
{
    public class RevenueTypeService : ISqlRepository<ResponseRevenueTypeDto, CreateRevenueTypeDto, EditRevenueTypeDto>
    {
        private readonly SpendlyDbContext _context;

        public RevenueTypeService(SpendlyDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseRevenueTypeDto> CreateAsync(CreateRevenueTypeDto dto)
        {
            var revenueType = new RevenueType
            {
                Name = dto.Name,
            };
            _context.RevenueTypes.Add(revenueType);
            await _context.SaveChangesAsync();

            return new ResponseRevenueTypeDto
            {
                Id = revenueType.Id,
                Name = revenueType.Name
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var revenueType = await _context.RevenueTypes.FindAsync(id);
            if (revenueType == null) return false;

            _context.RevenueTypes.Remove(revenueType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditAsync(int id, EditRevenueTypeDto dto)
        {
            var revenueType = await _context.RevenueTypes.FindAsync(id);
            if (revenueType == null) return false;

            revenueType.Name = dto.Name;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ResponseRevenueTypeDto>> GetAllAsync()
        {
            return await _context.RevenueTypes
                 .Select(r => new ResponseRevenueTypeDto
                 {
                     Id = r.Id,
                     Name = r.Name,
                 }).ToListAsync();
        }

        public async Task<ResponseRevenueTypeDto?> GetByIdAsync(int id)
        {
            return await _context.RevenueTypes
                .Where(u => u.Id == id)
                .Select(u => new ResponseRevenueTypeDto
                {
                    Id = u.Id,
                    Name = u.Name
                }).FirstOrDefaultAsync();
        }
    }
}