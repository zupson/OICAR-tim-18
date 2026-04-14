using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;

namespace SpendlyWebAPI.Services
{
    public class CurrencyService : ISqlRepository<ResponseCurrencyDto, CreateCurrencyDto, EditCurrencyDto>
    {
        private readonly SpendlyContext _context;

        public CurrencyService(SpendlyContext context)
        {
            _context = context;
        }

        public async Task<ResponseCurrencyDto> CreateAsync(CreateCurrencyDto dto)
        {
            var currency = new Currency
            {
                Name = dto.Name,
                Code = dto.Code,
            };

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();

            return new ResponseCurrencyDto
            {
                Id = currency.Id,
                Name = currency.Name,
                Code = currency.Code,
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency == null) return false;

            _context.Currencies.Remove(currency);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditAsync(int id, EditCurrencyDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ResponseCurrencyDto>> GetAllAsync()
        {
            return await _context.Currencies
            .Select(c => new ResponseCurrencyDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code
            })
            .ToListAsync();
        }

        public async Task<ResponseCurrencyDto?> GetByIdAsync(int id)
        {
            return await _context.Currencies
                .Where(c => c.Id == id)
                .Select(c => new ResponseCurrencyDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code
            }).FirstOrDefaultAsync();
        }
    }
}