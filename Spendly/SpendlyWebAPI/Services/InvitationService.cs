using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class InvitationService : IInvitation<ResponseInvitationDto, CreateInvitationDto>
    {
        private readonly SpendlyContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public InvitationService(SpendlyContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        private int GetCurrentUserIdFromJwt()
            => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);


        public async Task<ResponseInvitationDto> CreateAsync(CreateInvitationDto dto)
        {
            //var invitation = new Invitation
            //{
            //    Email = dto.Email,
            //    GroupId = dto.GroupId,
            //    Token = Guid.NewGuid().ToString(),
            //    ExpiredAt = DateTime.UtcNow.AddDays(7),
            //    CreatedByUserId = GetCurrentUserIdFromJwt(),
            //    IsDeleted = false
            //};

            //_context.Invitations.Add(invitation);
            //await _context.SaveChangesAsync();

            //return await _context.Invitations
            //    .Where(i => i.Id == invitation.Id)
            //    .Select(i => new ResponseInvitationDto
            //    {
            //        Id = i.Id,
            //        Email = i.Email,
            //        ClaimedAt = i.ClaimedAt,
            //        ExpiredAt = i.ExpiredAt,
            //        GroupId = i.GroupId,
            //        GroupName = i.Group.Name,
            //        CreatedByUserId = i.CreatedByUserId,
            //        CreatedByUserName = i.CreatedByUser.Username
            //    })
            //    .FirstAsync();

            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invitation = await _context.Invitations
               .FirstOrDefaultAsync(i => i.Id == id && i.CreatedByUserId == GetCurrentUserIdFromJwt() && !i.IsDeleted);

            if (invitation is null) return false;

            invitation.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ResponseInvitationDto>> GetAllAsync()
        {
            return await _context.Invitations
                .Where(i => i.CreatedByUserId == GetCurrentUserIdFromJwt() && !i.IsDeleted)
                .Select(i => new ResponseInvitationDto
                {
                    Id = i.Id,
                    Email = i.Email,
                    ClaimedAt = i.ClaimedAt,
                    ExpiredAt = i.ExpiredAt,
                    GroupId = i.GroupId,
                    GroupName = i.Group.Name,
                    CreatedByUserId = i.CreatedByUserId,
                    CreatedByUserName = i.CreatedByUser.Username
                })
                .ToListAsync();
        }

        public async Task<ResponseInvitationDto?> GetByIdAsync(int id)
        {
            return await _context.Invitations
                .Where(i => i.Id == id && i.CreatedByUserId == GetCurrentUserIdFromJwt() && !i.IsDeleted)
                .Select(i => new ResponseInvitationDto
                {
                    Id = i.Id,
                    Email = i.Email,
                    ClaimedAt = i.ClaimedAt,
                    ExpiredAt = i.ExpiredAt,
                    GroupId = i.GroupId,
                    GroupName = i.Group.Name,
                    CreatedByUserId = i.CreatedByUserId,
                    CreatedByUserName = i.CreatedByUser.Username
                })
                .FirstOrDefaultAsync();
        }
    }
}