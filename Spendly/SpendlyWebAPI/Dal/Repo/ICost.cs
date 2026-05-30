namespace SpendlyWebAPI.Dal.Repo
{
    public interface ICost<TResponseDto, TCreateDto, TEditDto>
    {
        Task<IEnumerable<TResponseDto>> GetAllAsync();
        Task<TResponseDto?> GetByIdAsync(int id);
        Task<TResponseDto> CreateAsync(TCreateDto dto, int id);
        Task<bool> EditAsync(int id, TEditDto dto);
        Task<bool> DeleteAsync(int id);
    }
} 