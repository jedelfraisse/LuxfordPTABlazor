using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LuxfordPTAWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UsersController>? _logger;

        public UsersController(UserManager<ApplicationUser> userManager, ILogger<UsersController>? logger = null)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ApplicationUser>> Get()
        {
            // Return all users (you may want to restrict this to Admins/BoardMembers)
            return Ok(_userManager.Users.ToList());
        }

        /// <summary>
        /// Get users who can serve as event coordinators
        /// </summary>
        [HttpGet("coordinators")]
        [Authorize(Roles = "Admin,BoardMember")]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetCoordinators()
        {
            try
            {
                // Get all users who are in roles that can coordinate events
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                var boardMembers = await _userManager.GetUsersInRoleAsync("BoardMember");
                
                // Try to get EventCoordinator role users, but don't fail if role doesn't exist
                var eventCoordinators = new List<ApplicationUser>();
                try
                {
                    eventCoordinators = (await _userManager.GetUsersInRoleAsync("EventCoordinator")).ToList();
                }
                catch
                {
                    // EventCoordinator role may not exist yet
                }
                
                // Combine and deduplicate
                var allCoordinators = adminUsers
                    .Concat(boardMembers)
                    .Concat(eventCoordinators)
                    .GroupBy(u => u.Id)
                    .Select(g => g.First())
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToList();

                return Ok(allCoordinators);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting available coordinators");
                return BadRequest(new { Error = "Error loading available coordinators" });
            }
        }

        /// <summary>
        /// Pre-provisions a user account (e.g. a known board member) without a password.
        /// Login is always passwordless — the user signs in later with an emailed one-time code.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,BoardMember")]
        public async Task<ActionResult<object>> Create([FromBody] ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email is required in the create api.");

            var newUser = new ApplicationUser
            {
                UserName = user.Email,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = true
                // FullName is read only
            };

            var result = await _userManager.CreateAsync(newUser);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { user = newUser });
        }
	}
}
