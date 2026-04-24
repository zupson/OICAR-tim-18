namespace SpendlyWebAPI.Dal.Repo
{
    public interface IUserGroup<TResponseDto>
    {
        Task<IEnumerable<TResponseDto>> GetAllAsync();
        Task<TResponseDto?> GetByIdAsync(int id);
        Task<TResponseDto> CreateAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}