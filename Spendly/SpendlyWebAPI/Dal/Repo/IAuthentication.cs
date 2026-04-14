using SpendlyWebAPI.Dtos;

namespace SpendlyWebAPI.Dal.Repo
{
    public interface IAuthentication<TResponseDto>
    {
        Task<IEnumerable<TResponseDto>> GetAllAsync();
        Task<TResponseDto?> GetByIdAsync(int id);
        Task<(TResponseDto user, string token)> RegisterAsync(RegisterUserDto dto);
        Task<(TResponseDto user, string token)> LoginAsync(LoginUserDto dto);
        Task<bool> PasswordChangeAsync(ChangePasswordDto dto);
        Task<bool> EditAsync(int id, EditUserDto dto);
        Task<bool> DeleteAsync(int id);
    }
}