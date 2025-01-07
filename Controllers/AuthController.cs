using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<UsersController> _logger;

        public AuthController(IConfiguration config, ILogger<UsersController> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// 登入以獲得 JWT
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto login)
        {
            _logger.LogInformation("Received login request for user: {Username}", login.Username);

            try
            {
                if (login.Username == "admin" && login.Password == "1234")
                {
                    var token = GenerateJwtToken(login.Username);
                    _logger.LogInformation("Login successful for user: {Username}.", login.Username);

                    return Ok(new { Token = token });
                }

                _logger.LogWarning("Invalid credentials for user: {Username}.", login.Username);
                return Unauthorized("Invalid credentials.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for user: {Username}.", login.Username);
                return StatusCode(500, new { Message = "An unexpected error occurred.", Detail = ex.Message });
            }
        }

        private string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogError("JWT Key is missing in configuration.");
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class UserLoginDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
