using SpendlyWebAPI.Dtos;

namespace SpendlyWebAPI.Dal.Repo
{
    public interface IInvitation<TResponseDto, TCreateDto>
    {
        Task<IEnumerable<TResponseDto>> GetAllAsync();
        Task<TResponseDto?> GetByIdAsync(int id);
        Task<TResponseDto> CreateAsync(TCreateDto dto, int id);
        Task<bool> DeleteAsync(int id);
        Task<ResponseUserGroupDto?> ClaimAsync(string token);
    }
}
