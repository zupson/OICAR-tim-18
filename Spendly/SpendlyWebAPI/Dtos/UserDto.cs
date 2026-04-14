using System.ComponentModel.DataAnnotations;

namespace SpendlyWebAPI.Dtos
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "First should be at least 3 characters long!")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Last should be at least 3  characters long!")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format!")]
        [StringLength(200)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username should be at least 3 characters long!")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password should be at least 8 characters long!")]
        public required string Password { get; set; }
    }

    public class EditUserDto : IValidatableObject
    {
        public int Id { get; set; }
        [StringLength(100, MinimumLength = 3, ErrorMessage = "First should be at least 3 characters long!")]
        public string? FirstName { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Last should be at least 3  characters long!")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format!")]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username should be at least 3 characters long!")]
        public string? Username { get; set; }

        public string? Password { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Password) && Password.Length < 8)
            {
                yield return new ValidationResult("Password must be minimum 8 characters long.");
                //yield osigurava da NE MORAMO:->
                //kreirati privremeni popis (listu), sakupljati sve greške u tu listu, i na kraju je vratiti.
            }
        }
    }

    public class LoginUserDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username should be at least 3 characters long!")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password should be should be at least 8 characters long!")]
        public required string Password { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username should be at least 3 characters long!")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password should be at least 8 characters long!")]
        public required string Password { get; set; }
    }


    //za prikaz usera
    //Sadrži samo podatke za prikaz korisnika, bez osjetljivih informacija.
    //Također koristi non-nullable tipove s inicijalizacijom, što je preporučeno.
    //Nema[Required] atributa, što je ispravno jer se response DTO ne validira
    public class ResponseUserDto
    {
        public int Id { get; set; }
        [Display(Name = "First name")]
        public string FirstName { get; set; } = default!;
        [Display(Name = "Last name")]
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;

        public string Username { get; set; } = default!;
        public IList<ResponseRoleDto> Roles { get; set; } = new List<ResponseRoleDto>();
    }

    //public class ResponseLoginUserDto
    //{
    //    public int Id { get; set; }
    //    public string FirstName { get; set; } = default!;
    //    public string LastName { get; set; } = default!;
    //    public string Email { get; set; } = default!;
    //    public string Username { get; set; } = default!;
    //    public List<ResponseRoleDto> Roles { get; set; } = new List<ResponseRoleDto>();
    //}
}
