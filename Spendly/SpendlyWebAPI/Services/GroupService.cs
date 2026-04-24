using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;

namespace SpendlyWebAPI.Services
{
    public class GroupService : ISqlRepository<ResponseGroupDto, CreateGroupDto, EditGroupDto>
    {
        private readonly SpendlyDbContext _context;

        public GroupService(SpendlyDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseGroupDto> CreateAsync(CreateGroupDto dto)
        {
            var group = new Group
            {
                Name = dto.Name,
                IsPersonal = dto.IsPersonal,
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return new ResponseGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                IsPersonal = group.IsPersonal,
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if (group == null) return false;

            group.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditAsync(int id, EditGroupDto dto)
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if (group == null) return false;

            group.Name = dto.Name;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ResponseGroupDto>> GetAllAsync()
        {
            return await _context.Groups
            .Where(g => !g.IsDeleted)
            .Select(g => new ResponseGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                IsPersonal = g.IsPersonal
            })
            .ToListAsync();
        }

        public async Task<ResponseGroupDto?> GetByIdAsync(int id)
        {
            return await _context.Groups
            .Where(g => g.Id == id && !g.IsDeleted)
            .Select(g => new ResponseGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                IsPersonal = g.IsPersonal
            }).FirstOrDefaultAsync();
        }
    }
}