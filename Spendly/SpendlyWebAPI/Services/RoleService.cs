using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;

namespace SpendlyWebAPI.Services
{
    public class RoleService : ISqlRepository<ResponseRoleDto, CreateRoleDto, EditRoleDto>
    {
        private readonly SpendlyContext _context;
        public RoleService(SpendlyContext context)
        {
            _context = context;
        }

        public async Task<ResponseRoleDto> CreateAsync(CreateRoleDto dto)
        {
            var newRole = new Role
            {
                Name = dto.Name
            };

            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();

            return new ResponseRoleDto
            {
                Id = newRole.Id,
                Name = dto.Name,
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditAsync(int id, EditRoleDto dto)
        {
            throw new NotImplementedException();
        }


        public async Task<IEnumerable<ResponseRoleDto>> GetAllAsync()
        {
            return await _context.Roles
                .Select(r => new ResponseRoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                }).ToListAsync();
        }

        public async Task<ResponseRoleDto?> GetByIdAsync(int id)
        {
            return await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new ResponseRoleDto
                {
                    Id = r.Id,
                    Name = r.Name
                }).FirstOrDefaultAsync();
        }
    }
}