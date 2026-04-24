using SpendlyWebAPI.Dtos;

namespace SpendlyWebAPI.Dal.Repo
{
    public interface IBudget<TResponseDto, TCreateDto, TEditDto>
    {
        Task<IEnumerable<TResponseDto>> GetAllAsync();
        Task<TResponseDto?> GetByIdAsync(int id);
        Task<TResponseDto> CreateAsync(int userGroupId, TCreateDto dto);
        Task<bool> EditAsync(int id, TEditDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
