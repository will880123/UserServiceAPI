using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserServiceAPI.Data;
using UserServiceAPI.Models;

namespace UserServiceAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Fetching all users.");

            try
            {
                var users = await _context.Users.ToListAsync();
                _logger.LogInformation("Fetched {UserCount} users.", users.Count);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return HandleException("fetching all users", ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            _logger.LogInformation("Fetching user with ID: {UserId}", id);

            try
            {
                var user = await FindUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { Message = $"User with ID {id} not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return HandleException($"fetching user with ID {id}", ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            _logger.LogInformation("Creating a new user.");

            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created user with ID: {UserId}", user.Id);

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return HandleException("creating a new user", ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", id);

            try
            {
                var user = await FindUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { Message = $"User with ID {id} not found." });
                }

                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated user with ID: {UserId}", id);

                return Ok(user);
            }
            catch (Exception ex)
            {
                return HandleException($"updating user with ID {id}", ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);

            try
            {
                var user = await FindUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { Message = $"User with ID {id} not found." });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted user with ID: {UserId}", id);

                return Ok(new { Message = $"Successfully deleted user with ID: {id}" });
            }
            catch (Exception ex)
            {
                return HandleException($"deleting user with ID {id}", ex);
            }
        }

        // 私有方法：查找用戶
        private async Task<User> FindUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        // 私有方法：統一處理例外
        private IActionResult HandleException(string action, Exception ex)
        {
            _logger.LogError(ex, "Error occurred while {Action}.", action);
            return StatusCode(500, new { Message = "An unexpected error occurred.", Detail = ex.Message });
        }
    }
}
