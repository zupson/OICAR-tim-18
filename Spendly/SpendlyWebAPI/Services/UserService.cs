using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Enums;
using SpendlyWebAPI.Models;
using SpendlyWebAPI.Security;
using System.Security.Claims;

namespace SpendlyWebAPI.Services
{
    public class UserService : IAuthentication<ResponseUserDto>
    {
        private readonly SpendlyDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(SpendlyDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrntUserId()
            => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null) return false;

            user.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EditAsync(int id, EditUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user == null) return false;

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.Username = dto.Username;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ResponseUserDto>> GetAllAsync()
        {
            return await _context.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new ResponseUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Username = u.Username
                }).ToListAsync();
        }

        public async Task<ResponseUserDto?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Where(u => u.Id == id && !u.IsDeleted)
                .Select(u => new ResponseUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Username = u.Username
                }).FirstOrDefaultAsync();
        }

        public async Task<(ResponseUserDto user, string token)> LoginAsync(LoginUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == dto.Username);
            if (user == null) throw new KeyNotFoundException("Wrong password");

            var userGroup = await _context.UserGroups
                .Where(ug => ug.UserId == user.Id)
                .OrderByDescending(ug => ug.Role)
                .FirstOrDefaultAsync();

            var role = userGroup?.Role == (int)Role.GeneralAdmin ? Role.GeneralAdmin : Role.User;


            var b64hash = PasswordHashProvider.GetHash(dto.Password, user.PasswordSalt);
            if (b64hash != user.PasswordHash) throw new ArgumentException("Wrong password");

            string? secureKey = _configuration[key: "JWT:SecureKey"];
            if (secureKey == null) throw new Exception("Something went wrong with token");

            var JWT = JwtProvider.CreateToken(
                secureKey,
                60,
                role,
                user.Id
            );


            ResponseUserDto responseUserDto = new ResponseUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
            };

            return (responseUserDto, JWT);
        }

        public async Task<bool> PasswordChangeAsync(ChangePasswordDto dto)
        {
            int userId = GetCurrntUserId();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return false;

            string currentPasswordHash = PasswordHashProvider.GetHash(dto.CurrentPassword, user.PasswordSalt);

            if (currentPasswordHash != user.PasswordHash) throw new Exception("Wrong current password");

            user.PasswordSalt = PasswordHashProvider.GetSalt();
            user.PasswordHash = PasswordHashProvider.GetHash(dto.Password, user.PasswordSalt);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(ResponseUserDto user, string token)> RegisterAsync(RegisterUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                throw new InvalidOperationException("Ovo korisničko ime je već zauzeto.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Ova e-mail adresa je već registrirana.");

            var newUser = CreateNewUserAccount(dto);
            await _context.SaveChangesAsync();

            var group = CreatePersonalGroupForNewUser(newUser);
            await _context.SaveChangesAsync();

            var userGroup = ConnectNewUserWithHisPersonalGroup(newUser, group);

            await _context.SaveChangesAsync();

            var secureKey = _configuration[key: "JWT:SecureKey"];
            if (secureKey == null) throw new Exception("Something went wrong with token");

            string JWT = JwtProvider.CreateToken(
                secureKey,
                60,
                Role.User,
                newUser.Id
            );

            var responsePersonDto = new ResponseUserDto
            {
                Id = newUser.Id,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Username = newUser.Username,
            };

            return (responsePersonDto, JWT);
        }

        private UserGroup ConnectNewUserWithHisPersonalGroup(User newUser, Group group)
        {
            var userGroup = new UserGroup
            {
                UserId = newUser.Id,
                GroupId = group.Id,
                Role = (int)Role.Owner,
                JoinedAt = DateTime.UtcNow
            };
            _context.UserGroups.Add(userGroup);
            return userGroup;
        }

        private Group CreatePersonalGroupForNewUser(User newUser)
        {
            var group = new Group
            {
                Name = $"{newUser.Username}'s group",
                IsPersonal = true
            };

            _context.Groups.Add(group);
            return group;
        }

        private User CreateNewUserAccount(RegisterUserDto dto)
        {
            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash(dto.Password, salt);

            var newUser = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Username = dto.Username,
            };
            newUser.PasswordHash = hash;
            newUser.PasswordSalt = salt;

            _context.Users.Add(newUser);
            return newUser;
        }
    }
}