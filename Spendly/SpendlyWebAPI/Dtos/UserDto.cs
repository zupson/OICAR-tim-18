using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class RegisterUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 8)]
        public string Password { get; set; }
    }

    public class EditUserDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? FirstName { get; set; }

        [StringLength(100, MinimumLength = 3)]
        public string? LastName { get; set; }

        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string? Username { get; set; }

        [StringLength(50, MinimumLength = 8)]
        public string? Password { get; set; }
    }

    public class LoginUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 8)]
        public string Password { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 8)]
        public string Password { get; set; }
    }


    public class ResponseUserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public List<ResponseUserGroupDto> Groups { get; set; } = new();
    }
}