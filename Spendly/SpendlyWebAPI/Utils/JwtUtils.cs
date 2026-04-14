using System.Security.Claims;

namespace SpendlyWebAPI.Utils
{
    public class JwtUtils
    {        
        public static int GetUserId(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var idClaim = user.FindFirst("id")?.Value
                          ?? throw new Exception("JWT ne sadrži claim 'id'");

            return int.Parse(idClaim);
        }        
    }
}