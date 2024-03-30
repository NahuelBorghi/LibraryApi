using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryApi.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly Models.LibraryApiContext _context;

        public AuthController(Models.LibraryApiContext context)
        {
            _context = context;
        }

        [HttpGet("api/Auth/Login")]
        public IActionResult Login()
        {
            return Ok(); // Assuming frontend handles login form
        }

        [HttpPost]
        [Route("api/Auth/Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (ModelState.IsValid)
            {
                if (IsValidUser(request.Email, request.Password))
                {
                    var tokenString = GenerateJwtToken(request.Email, IsAdmin(request.Email));
                    var cookieOptions = new CookieOptions
                    {
                        Path = "/",
                        Expires = DateTime.UtcNow.AddDays(30),
                        HttpOnly = true,
                        Secure = true // Consider environment-based configuration for Secure flag
                    };
                    Response.Cookies.Append("AuthToken", tokenString, cookieOptions);
                    return Ok();
                }
                return BadRequest("Invalid username or password");
            }

            return BadRequest(ModelState); // Handle validation errors
        }

        private string GenerateJwtToken(string username, bool isAdmin)
        {

            // **JWT**
            // **Clave alfanumérica para crear tokens**
            // **No es recomendable mantenerla aquí por motivos de seguridad**
            string key = "c0Ntr4T4M3pOrfAv0RqU1eRotr4bAjaRj4JaJA=";
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User") // Add role claim based on isAdmin check
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMonths(1), // Set token expiration (e.g., 60 minutes)
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);

            return tokenString;
        }

        // Implement methods for IsValidUser and IsAdmin (replace with your authentication logic)
        private bool IsValidUser(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == username && u.Password == password);
            return user != null;
        }

        private bool IsAdmin(string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == username);
            return user?.Role == "Admin";
        }
    }
}
