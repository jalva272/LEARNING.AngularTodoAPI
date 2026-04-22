using Microsoft.AspNetCore.Mvc; // contains classes and attributes for building API controllers, such as ControllerBase, ApiController, HttpGet, HttpPost, etc.
using Microsoft.IdentityModel.Tokens; // contains classes for working with JWT tokens, such as SymmetricSecurityKey, TokenValidationParameters, etc.
using System.IdentityModel.Tokens.Jwt; // contains classes for creating and validating JWT tokens, such as JwtSecurityToken, JwtSecurityTokenHandler, etc.
using System.Security.Claims; // contains classes for working with claims-based identity, such as Claim, ClaimsIdentity, etc.
using System.Text; // contains classes for working with text encoding, such as Encoding.UTF8.GetBytes() which we use to convert our secret key string into a byte array for signing JWTs

namespace AngularTodoAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController: ControllerBase
    {
        private readonly IConfiguration _config; // allows access to configuration settings from appsettings.json, such as our JWT settings
        
        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // DEMO LOGIN (replace later with DB)
            if (request.Username != "admin" || request.Password != "password")
                return Unauthorized();

            var token = GenerateJwtToken(request.Username);
            return Ok(new { token });
        }

        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, username)
        };

            var token = new JwtSecurityToken(
                jwtSettings["Issuer"],
                jwtSettings["Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(jwtSettings["ExpiresInMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

public record LoginRequest(string Username, string Password);