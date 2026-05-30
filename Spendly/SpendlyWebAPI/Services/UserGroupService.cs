using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class UserGroupService : IUserGroup<ResponseUserGroupDto>
    {
        private readonly SpendlyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserGroupService(SpendlyDbContext context, IHttpContextAccessor httpContextAccessor)
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
            int userId = GetCurrentUserIdFromJwt();
            return await _context.UserGroups
                .Include(x => x.User)
                .Include(x => x.Group)
                .Where(uG => uG.UserId == userId)
                .Select(uG => new ResponseUserGroupDto
                {
                    Id = uG.Id,
                    UserId = uG.UserId,
                    Username = uG.User.Username,
                    GroupId = uG.GroupId,
                    GroupName = uG.Group.Name,
                    IsPersonal = uG.Group.IsPersonal,
                    Role = uG.Role,
                    JoinedAt = uG.JoinedAt,
                }).ToListAsync();
        }

        public async Task<IEnumerable<ResponseUserGroupDto>> GetMembersByGroup(int groupId)
        {
            return await _context.UserGroups
               .Include(x => x.User)
               .Include(x => x.Group)
               .Where(uG => uG.GroupId == groupId)
               .Select(uG => new ResponseUserGroupDto
               {
                   Id = uG.Id,
                   UserId = uG.UserId,
                   Username = uG.User.Username,
                   GroupId = uG.GroupId,
                   GroupName = uG.Group.Name,
                   IsPersonal = uG.Group.IsPersonal,
                   Role = uG.Role,
                   JoinedAt = uG.JoinedAt,
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
                    Username = uG.User.Username,
                    GroupId = uG.GroupId,
                    GroupName = uG.Group.Name,
                    IsPersonal = uG.Group.IsPersonal,
                    Role = uG.Role,
                    JoinedAt = uG.JoinedAt,
                }).FirstOrDefaultAsync();
        }

        public async Task<ResponseUserGroupDto> CreateAsync(int groupId)
        {
            int userId = GetCurrentUserIdFromJwt();

            bool alreadyMember = await _context.UserGroups.AnyAsync(ug => ug.UserId == userId && ug.GroupId == groupId);
            if (alreadyMember) throw new InvalidOperationException("User is already a member of this group.");

            var userGroup = new UserGroup
            {
                UserId = userId,
                GroupId = groupId,
                JoinedAt = DateTime.UtcNow
            };

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();

            return new ResponseUserGroupDto
            {
                Id = userGroup.Id,
                UserId = userGroup.User.Id,
                Username = userGroup.User.Username,
                GroupId = userGroup.GroupId,
                GroupName = userGroup.Group.Name,
                IsPersonal = userGroup.Group.IsPersonal,
                Role = userGroup.Role,
                JoinedAt = userGroup.JoinedAt,
            };
        }

        public async Task<bool> RemoveMemberAsync(int userGroupId)
        {
            int requesterId = GetCurrentUserIdFromJwt();

            var target = await _context.UserGroups
                .Include(ug => ug.Group)
                .FirstOrDefaultAsync(ug => ug.Id == userGroupId);

            if (target == null) return false;

            var requesterMembership = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == requesterId && ug.GroupId == target.GroupId);

            if (requesterMembership == null)
                throw new UnauthorizedAccessException("Niste član ove grupe.");

            if (requesterMembership.Role != (int)Enums.Role.Owner)
                throw new UnauthorizedAccessException("Samo vlasnik grupe može uklanjati članove.");

            if (target.Role == (int)Enums.Role.Owner)
                throw new InvalidOperationException("Vlasnika nije moguće ukloniti iz vlastite grupe.");

            _context.UserGroups.Remove(target);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}