using Microsoft.IdentityModel.Tokens;
using SpendlyWebAPI.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SpendlyWebAPI.Security
{
    public class JwtProvider
    {
        public static string CreateToken(string secureKey, int expiration, IEnumerable<ResponseRoleDto> roles, string? subject = null)
        {
            // Get secret key bytes
            var tokenKey = Encoding.UTF8.GetBytes(secureKey);

            // Create a token descriptor (represents a token, kind of a "template" for token)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(expiration),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            if (!string.IsNullOrEmpty(subject))
            {
                List<Claim> claims = new() {
                    new Claim(ClaimTypes.Name, subject),
                    new Claim(JwtRegisteredClaimNames.Sub, subject),
                };

                var roelsResult = roles;//cekamo da se IEnumarable nalouda iz poziva metode
                var roleClaims = roelsResult.Select(r => new Claim(ClaimTypes.Role, r.Name));
                claims.AddRange(roleClaims);
                tokenDescriptor.Subject = new ClaimsIdentity(claims);
            }

            // Create token using that descriptor, serialize it and return it
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }
    }
}
