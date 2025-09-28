using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
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
                LastName = user.LastName
                // FullName is read only
            };

            // Generate a secure random password
            var password = GenerateRandomPassword(12);

            var result = await _userManager.CreateAsync(newUser, password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Optionally: Add a claim to require password change on first login
            await _userManager.AddClaimAsync(newUser, new System.Security.Claims.Claim("RequirePasswordChange", "true"));

            // Return the new user and password to the admin
            return Ok(new { user = newUser, password });
        }

		// Helper method to generate a secure random password
		private static string GenerateRandomPassword(int length)
		{
			const string alphanumerics = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
			const string nonAlphanumerics = "!@$?_";
			StringBuilder res = new StringBuilder();
			using (var rng = RandomNumberGenerator.Create())
			{
				byte[] uintBuffer = new byte[sizeof(uint)];

				// Ensure at least one non-alphanumeric character
				rng.GetBytes(uintBuffer);
				uint num = BitConverter.ToUInt32(uintBuffer, 0);
				res.Append(nonAlphanumerics[(int)(num % (uint)nonAlphanumerics.Length)]);

                // Ensure at least one digit
                rng.GetBytes(uintBuffer);
                num = BitConverter.ToUInt32(uintBuffer, 0);
                res.Append(alphanumerics[(int)(num % 10) + 52]); // 52-61 are digits in alphanumerics

				// Ensure at least one uppercase letter
                rng.GetBytes(uintBuffer);
                num = BitConverter.ToUInt32(uintBuffer, 0);
                res.Append(alphanumerics[(int)(num % 26)]); // 0-25 are uppercase letters in alphanumerics

				// Ensure at least one lowercase letter
                rng.GetBytes(uintBuffer);
                num = BitConverter.ToUInt32(uintBuffer, 0);
                res.Append(alphanumerics[(int)(num % 26) + 26]); // 26-51 are lowercase letters in alphanumerics    

				// Fill the rest with alphanumerics and non-alphanumerics
				string allValid = alphanumerics + nonAlphanumerics;
				for (int i = 1; i < length; i++)
				{
					rng.GetBytes(uintBuffer);
					num = BitConverter.ToUInt32(uintBuffer, 0);
					res.Append(allValid[(int)(num % (uint)allValid.Length)]);
				}
			}

			// Shuffle the password so the non-alphanumeric isn't always first
			return new string(res.ToString().OrderBy(_ => Guid.NewGuid()).ToArray());
		}
	}
}
