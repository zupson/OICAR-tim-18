using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class UserGroupService : IUserGroup<ResponseUserGroupDto, CreateUserGroupDto>
    {
        private readonly SpendlyContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserGroupService(SpendlyContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        private int GetCurrentUserIdFromJwt()
           => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);


        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.UserGroups.FindAsync(id);
            if (entity == null) return false;

            _context.UserGroups.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ResponseUserGroupDto>> GetAllAsync()
        {
            return await _context.UserGroups
                .Include(x => x.User)
                .Include(x => x.Group)
                .Select(uG => new ResponseUserGroupDto
                {
                    Id = uG.Id,
                    UserId = uG.UserId,
                    UserName = uG.User.Username,
                    GroupId = uG.GroupId,
                    GroupName = uG.Group.Name,
                    JoinedAt = uG.JoinedAt,
                    InvitationId = uG.InvitationId,
                }).ToListAsync();
        }

        public async Task<ResponseUserGroupDto?> GetByIdAsync(int id)
        {
            return await _context.UserGroups
                .Include(uG => uG.User)
                .Include(uG => uG.Group)
                .Where(uG => uG.Id == id)
                .Select(uG => new ResponseUserGroupDto
                {
                    Id = uG.Id,
                    UserId = uG.UserId,
                    UserName = uG.User.Username,
                    GroupId = uG.GroupId,
                    GroupName = uG.Group.Name,
                    JoinedAt = uG.JoinedAt,
                    InvitationId = uG.InvitationId
                }).FirstOrDefaultAsync();
        }

        public async Task<ResponseUserGroupDto> CreateAsync(CreateUserGroupDto dto)
        {
            var userGroup = new UserGroup
            {
                UserId = GetCurrentUserIdFromJwt(),
                GroupId = dto.GroupId,
                JoinedAt = DateTime.UtcNow,
                InvitationId = dto.InvitationId
            };

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();

            return new ResponseUserGroupDto
            {
                Id = userGroup.Id,
                UserId = GetCurrentUserIdFromJwt(),
                UserName = userGroup.User.Username,
                GroupId= userGroup.GroupId,
                GroupName = userGroup.Group.Name,
                JoinedAt = userGroup.JoinedAt,
                InvitationId = dto.InvitationId
            };
        }
    }
}