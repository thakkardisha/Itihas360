using Itihas360.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Itihas360.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly Itihas360Context _context;

        public AuthenticateController(UserManager<IdentityUser> userManager, IConfiguration configuration, Itihas360Context context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // 1. Check if user already exists in Identity
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return BadRequest("User already exists!");
            }

            // 2. Define the Identity User
            IdentityUser identityUser = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email
            };

            // 3. Attempt to create user in AspNetUsers
            var result = await _userManager.CreateAsync(identityUser, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // 4. ROLE SEEDING: Ensure Roles exist in AspNetRoles
            // This is why your tables were empty; the roles must exist before assignment.
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 5. ROLE ASSIGNMENT: Assign role based on email domain
            if (model.Email.EndsWith("@itihas360.com"))
            {
                // This creates the entry in AspNetUserRoles
                await _userManager.AddToRoleAsync(identityUser, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(identityUser, "User");
            }

            // 6. CUSTOM TABLE SYNC: Add entry to your dbo.Users table
            // We do this LAST to ensure authentication is fully set up first
            var customUser = new User
            {
                Email = model.Email,
                FullName = model.Email.Split('@')[0],
                PasswordHash = "Identity_Managed", // Password is kept in Identity table
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            try
            {
                _context.Users.Add(customUser);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Even if custom table fails, the Identity user is already created.
                // In a production app, you might want to use a Transaction here.
                return StatusCode(500, "Identity user created, but failed to sync to custom Users table.");
            }

            return Ok(new { message = "User registered successfully with appropriate roles!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

                // Add each role to the claims
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var key = _configuration["Jwt:Key"] ?? "Aapki_Secret_Key_Jo_Badi_Honi_Chahiye_32_Chars";
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims, // Claims now include Roles
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }
    }
}