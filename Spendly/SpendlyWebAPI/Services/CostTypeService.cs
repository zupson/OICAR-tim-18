using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class CostTypeService : ISqlRepository<ResponseCostTypeDto, CreateCostTypeDto, EditCostTypeDto>
    {
        private readonly SpendlyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public CostTypeService(SpendlyDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
           => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));


        public async Task<ResponseCostTypeDto> CreateAsync(CreateCostTypeDto dto)
        {
            var costType = new CostType
            {
                Name = dto.Name,
                GroupId = dto.GroupId
            };

            _context.CostTypes.Add(costType);
            await _context.SaveChangesAsync();

            return new ResponseCostTypeDto
            {
                Id = costType.Id,
                Name = costType.Name,
                GroupId = dto.GroupId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var costType = await _context.CostTypes.FindAsync(id);
            if (costType == null) return false;
            int userId = GetCurrentUserId();
            bool isInGroup = await _context.UserGroups
                .AnyAsync(ug => ug.GroupId == costType.GroupId && ug.UserId == userId);

            if (!isInGroup)
                throw new UnauthorizedAccessException("Nemate pristup ovoj kategoriji.");


            _context.CostTypes.Remove(costType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditAsync(int id, EditCostTypeDto dto)
        {
            var costType = await _context.CostTypes.FindAsync(id);
            if (costType == null) return false;

            int userId = GetCurrentUserId();
            bool isInGroup = await _context.UserGroups
                .AnyAsync(ug => ug.GroupId == costType.GroupId && ug.UserId == userId);

            if (!isInGroup)
                throw new UnauthorizedAccessException("Nemate pristup ovoj kategoriji.");


            costType.Name = dto.Name;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ResponseCostTypeDto>> GetAllAsync(int? id = null)
        {
            return await _context.CostTypes
                .Where(ct => ct.GroupId == id)
                .Select(ct => new ResponseCostTypeDto
                {
                    Id = ct.Id,
                    Name = ct.Name,
                    GroupId = ct.GroupId
                }).ToListAsync();
        }

        public async Task<ResponseCostTypeDto?> GetByIdAsync(int id)
        {
            var costType = await _context.CostTypes.FindAsync(id);
            if (costType == null) return null;

            return new ResponseCostTypeDto
            {
                Id = costType.Id,
                Name = costType.Name,
                GroupId = costType.GroupId
            };
        }
    }
}