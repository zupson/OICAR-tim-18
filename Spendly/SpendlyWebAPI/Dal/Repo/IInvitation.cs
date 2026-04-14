namespace SpendlyWebAPI.Dal.Repo
{
    public interface IInvitation<TResponseDto, TCreateDto>
    {
        Task<IEnumerable<TResponseDto>> GetAllAsync();
        Task<TResponseDto?> GetByIdAsync(int id);
        Task<TResponseDto> CreateAsync(TCreateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
