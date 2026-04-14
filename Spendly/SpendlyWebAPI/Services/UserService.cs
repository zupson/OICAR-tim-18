using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpendlyWebAPI.Constants;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Models;
using SpendlyWebAPI.Security;
using System;

namespace SpendlyWebAPI.Services
{
    public class UserService : IAuthentication<ResponseUserDto>
    {
        private readonly SpendlyContext _context;
        private readonly IConfiguration _configuration;
        private readonly ISqlRepository<ResponseRoleDto, CreateRoleDto, EditRoleDto> _role;
        public UserService(SpendlyContext context, IConfiguration configuration, ISqlRepository<ResponseRoleDto, CreateRoleDto, EditRoleDto> role)
        {
            _context = context;
            _configuration = configuration;
            _role = role;
        }

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

            user.FistName = dto.FirstName;
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
                    FirstName = u.FistName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Username = u.Username
                }).ToListAsync();
        }

        public async Task<ResponseUserDto?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new ResponseUserDto
                {
                    Id = u.Id,
                    FirstName = u.FistName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Username = u.Username
                }).FirstOrDefaultAsync();
        }

        public async Task<(ResponseUserDto user, string token)> LoginAsync(LoginUserDto dto)
        {
            var salt = PasswordHashProvider.GetSalt();
            string hash = PasswordHashProvider.GetHash(dto.Password, salt);

            var user = await _context.Users
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(x => x.Username == dto.Username);

            if (user == null)
                throw new KeyNotFoundException("Wrong password");

            var b64hash = PasswordHashProvider.GetHash(dto.Password, user.PasswordSalt);
            if (b64hash != user.PasswordHash)
                throw new ArgumentException("Wrong password");

            string? secureKey = _configuration[key: "JWT:SecureKey"];

            var roleFromDb = await _role.GetAllAsync();

            var jwt = JwtProvider.CreateToken(secureKey, 60, roleFromDb, dto.Username);


            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            ResponseUserDto responseUserDto = new ResponseUserDto
            {
                Id = currentUser.Id,
                FirstName = currentUser.FistName,
                LastName = currentUser.LastName,
                Email = currentUser.Email,
                Username = currentUser.Username,
                Roles = currentUser.Roles.Select(r => new ResponseRoleDto
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            };

            return (responseUserDto, jwt);
        }

        public async Task<bool> PasswordChangeAsync(ChangePasswordDto dto)
        {
            string trimmedUsername = dto.Username.Trim();
            var existedUsername = await _context.Users.FirstOrDefaultAsync(u=> u.Username.Equals(trimmedUsername));

            if (existedUsername == null || existedUsername.IsDeleted)
                return false;

            existedUsername.PasswordSalt = PasswordHashProvider.GetSalt();
            existedUsername.PasswordHash = PasswordHashProvider.GetHash(dto.Password, existedUsername.PasswordSalt);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(ResponseUserDto user, string token)> RegisterAsync(RegisterUserDto dto)
        {
            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash(dto.Password, salt);

            var newUser = new User
            {
                FistName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Username = dto.Username,
                
            };
            newUser.PasswordHash = hash;
            newUser.PasswordSalt = salt;

            var userRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == Roles.User);

            newUser.Roles.Add(userRole);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();


            var rolesFromDb = await _role.GetAllAsync();
            var secureKey = _configuration[key: "JWT:SecureKey"];
            if (secureKey == null)
                throw new Exception("Something went wrong with token");

            string JWT = JwtProvider.CreateToken(secureKey,60,rolesFromDb,dto.Username);

            var responsePersonDto =new ResponseUserDto
            {
                Id = newUser.Id,
                FirstName = newUser.FistName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Username = newUser.Username,
                Roles = newUser.Roles.Select(r => new ResponseRoleDto
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            };

            return (responsePersonDto, JWT);
        }
    }
}