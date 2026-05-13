using AngularTodoAPI.Data; // contains the TodoContext class for accessing the User table in the database
using Microsoft.AspNetCore.Mvc; // contains classes and attributes for building API controllers, such as ControllerBase, ApiController, HttpGet, HttpPost, etc.
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // contains classes for working with JWT tokens, such as SymmetricSecurityKey, TokenValidationParameters, etc.
using System.IdentityModel.Tokens.Jwt; // contains classes for creating and validating JWT tokens, such as JwtSecurityToken, JwtSecurityTokenHandler, etc.
using System.Security.Claims; // contains classes for working with claims-based identity, such as Claim, ClaimsIdentity, etc.
using System.Security.Cryptography; // contains classes for cryptographic operations, such as HMACSHA512 which we use for hashing passwords and verifying password hashes
using System.Text; // contains classes for working with text encoding, such as Encoding.UTF8.GetBytes() which we use to convert our secret key string into a byte array for signing JWTs

namespace AngularTodoAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config; // allows access to configuration settings from appsettings.json, such as our JWT settings
        private readonly TodoContext _db; // allows access to the database

        public AuthController(IConfiguration config, TodoContext context)
        {
            _config = config;
            _db = context;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] LoginRequest req)
        {
            if (string.IsNullOrEmpty(req?.username) || string.IsNullOrEmpty(req?.password))
                return BadRequest(new { message = "username and password are required" });

            var users = await _db.Users
                .FromSqlRaw("EXEC dbo.User_GetUser @p0", req.username)
                .AsNoTracking()
                .ToListAsync();

            var existing = users.FirstOrDefault(); // execute the query and get the first user (if any)
            if (existing is not null)
                return Conflict(new { message = "username already exists" });

            // create salt and hash
            using var hmac = new HMACSHA512();
            var saltBytes = hmac.Key;
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(req.password));

            var salt = Convert.ToBase64String(saltBytes);
            var hashedPassword = Convert.ToBase64String(hashBytes);

            // call stored procedure to insert user
            await _db.Database.ExecuteSqlRawAsync(
                "EXEC dbo.User_CreateUser @p0, @p1, @p2",
                req.username, hashedPassword, salt
            );

            return Ok(new { message = "user created" });
        }

        //[HttpPost("login")] // POST api/auth/login
        //public IActionResult Login([FromBody] LoginRequest request)
        //{
        //    //!!! DEMO LOGIN (replace later with DB)
        //    if (request.username != "admin" || request.password != "password")
        //        return Unauthorized();

        //    var token = GenerateJwtToken(request.username);
        //    return Ok(new { token });
        //}

        [HttpPost("login")] // POST api/auth/login
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            // check if username and password are passed in the request body from Angular login form
            if (string.IsNullOrEmpty(req?.username) || string.IsNullOrEmpty(req?.password))
            {
                return Unauthorized();
            }

            var query = _db.Users.FromSqlRaw("EXEC dbo.User_GetUser @p0", req.username).AsNoTracking();
            var users = await query.ToListAsync();
            var user = users.FirstOrDefault();

            if (user is null) return Unauthorized();

            // TESTING: print user found in database to console
            Console.WriteLine("User found: " + user.UserName);
            /*
             * TESTING PASSWORD VERIFICATION
             * username: testuser
             * password: P@ssw0rd!
            */

            byte[] saltBytes;
            byte[] storedHashBytes;
            try
            {
                saltBytes = Convert.FromBase64String(user.PasswordSalt);
                storedHashBytes = Convert.FromBase64String(user.PasswordHash);
            }
            catch
            {
                return Unauthorized();
            }

            using var hmac = new HMACSHA512(saltBytes);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(req.password));
            if (!computedHash.SequenceEqual(storedHashBytes)) return Unauthorized();

            var token = GenerateJwtToken(user.UserName);

            return Ok(new { token });
        }

        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] { new Claim(ClaimTypes.Name, username) };

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

public record LoginRequest(string username, string password);