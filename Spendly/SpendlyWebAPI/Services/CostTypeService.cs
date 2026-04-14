using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;

namespace SpendlyWebAPI.Services
{
    public class CostTypeService : ISqlRepository<ResponseCostTypeDto, CreateCostTypeDto, EditCostTypeDto>
    {
        private readonly SpendlyContext _context;

        public CostTypeService(SpendlyContext context)
        {
            _context = context;
        }

        public async Task<ResponseCostTypeDto> CreateAsync(CreateCostTypeDto dto)
        {
            var costType = new CostType
            {
                Name = dto.Name,
            };

            _context.CostTypes.Add(costType);
            await _context.SaveChangesAsync();

            return new ResponseCostTypeDto
            {
                Id = costType.Id,
                Name = costType.Name,
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var costType = await _context.CostTypes.FindAsync(id);
            if (costType == null) return false;
            _context.CostTypes.Remove(costType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditAsync(int id, EditCostTypeDto dto)
        {
            var costType = await _context.CostTypes.FindAsync(id);
            if (costType == null) return false;

            costType.Name = dto.Name;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ResponseCostTypeDto>> GetAllAsync()
        {
            return await _context.CostTypes
                .Select(s => new ResponseCostTypeDto
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToListAsync();
        }

        public async Task<ResponseCostTypeDto?> GetByIdAsync(int id)
        {
            var costType = await _context.CostTypes.FindAsync(id);
            if (costType == null) return null;

            return new ResponseCostTypeDto
            {
                Id = costType.Id,
                Name = costType.Name
            };
        }
    }
}
