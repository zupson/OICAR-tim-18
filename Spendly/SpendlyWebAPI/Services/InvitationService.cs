using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Enums;
using SpendlyWebAPI.Models;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class InvitationService : IInvitation<ResponseInvitationDto, CreateInvitationDto>
    {
        private readonly SpendlyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        public InvitationService(SpendlyDbContext context, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }
        private int GetCurrentUserIdFromJwt()
            => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<ResponseInvitationDto> CreateAsync(CreateInvitationDto dto, int groupId)
        {
            var existing = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Email == dto.Email
                               && !i.IsDeleted
                               && i.ExpiredAt > DateTime.UtcNow
                               && i.ClaimedAt == null);

            if (existing != null)
                throw new InvalidOperationException("Aktivni invite za taj email već postoji.");

            var invitation = new Invitation
            {
                Email = dto.Email,
                Token = Guid.NewGuid().ToString(),
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                IsDeleted = false,
                GroupId = groupId
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            await _emailService.SendInviteAsync(dto.Email, invitation.Token);

            return await _context.Invitations
                .Where(i => i.Id == invitation.Id)
                .Select(i => new ResponseInvitationDto
                {
                    Id = i.Id,
                    Email = i.Email,
                    ClaimedAt = i.ClaimedAt,
                    ExpiredAt = i.ExpiredAt,
                    GroupId = i.GroupId,
                    GroupName = i.Group.Name,
                    CreatedByUserId = GetCurrentUserIdFromJwt(),
                })
                .FirstAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invitation = await _context.Invitations
               .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (invitation is null) return false;

            invitation.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ResponseInvitationDto>> GetAllAsync()
        {
            return await _context.Invitations
                .Where(i => !i.IsDeleted)
                .Select(i => new ResponseInvitationDto
                {
                    Id = i.Id,
                    Email = i.Email,
                    ClaimedAt = i.ClaimedAt,
                    ExpiredAt = i.ExpiredAt,
                    GroupId = i.GroupId,
                    GroupName = i.Group.Name,
                    CreatedByUserId = GetCurrentUserIdFromJwt(),
                })
                .ToListAsync();
        }

        public async Task<ResponseInvitationDto?> GetByIdAsync(int id)
        {
            return await _context.Invitations
                .Include(i => i.Group)
                .Where(i => i.Id == id && !i.IsDeleted)
                .Select(i => new ResponseInvitationDto
                {
                    Id = i.Id,
                    Email = i.Email,
                    ClaimedAt = i.ClaimedAt,
                    ExpiredAt = i.ExpiredAt,
                    GroupId = i.GroupId,
                    GroupName = i.Group.Name,
                    CreatedByUserId = GetCurrentUserIdFromJwt(),
                })
                .FirstOrDefaultAsync();
        }

        
        public async Task<ResponseUserGroupDto?> ClaimAsync(string token)
        {
            var userId = GetCurrentUserIdFromJwt();
            var user = await _context.Users.FindAsync(userId);

            var invitation = await _context.Invitations
                .Include(i => i.Group)
                .FirstOrDefaultAsync(i => i.Token == token && !i.IsDeleted);

            if (invitation == null)
                throw new InvalidOperationException("Invalid token.");

            if (!string.Equals(invitation.Email, user!.Email, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This invitation was not sent to your account.");

            if (invitation.ExpiredAt < DateTime.UtcNow)
                throw new InvalidOperationException("Invitation expired.");

            if (invitation.ClaimedAt != null)
                throw new InvalidOperationException("Invitation already used.");

            await VerifyMembersInGroupAsync(invitation, userId);

            var userGroup = new UserGroup
            {
                UserId = userId,
                GroupId = invitation.GroupId,
                JoinedAt = DateTime.UtcNow,
                Role = (int)Role.User,
            };

            _context.UserGroups.Add(userGroup);
            invitation.ClaimedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ResponseUserGroupDto
            {
                UserId = userGroup.UserId,
                GroupId = userGroup.GroupId,
                GroupName = invitation.Group.Name
            };
        }

        private async Task VerifyMembersInGroupAsync(Invitation invitation, int userId)
        {
            var alreadyMember = await _context.UserGroups
                            .AnyAsync(ug => ug.UserId == userId && ug.GroupId == invitation.GroupId);

            if (alreadyMember) throw new InvalidOperationException("User already in group.");
        }
    }
}