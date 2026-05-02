using FocusSpace.Domain.Entities;
using FocusSpace.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainTask = FocusSpace.Domain.Entities.Task;

namespace FocusSpace.Api.Controllers
{
   
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            AppDbContext context,
            UserManager<User> userManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

      
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin panel accessed by {User}", User.Identity?.Name);

            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.ActiveTasks = await _context.Tasks.CountAsync();
            ViewBag.ActiveSessions = await _context.Sessions
                .CountAsync(s => s.Status == FocusSpace.Domain.Enums.SessionStatus.Ongoing);
            ViewBag.BlockedUsers = await _context.Users.CountAsync(u => u.IsBlocked);
            ViewBag.PendingApproval = await _context.Users
                .CountAsync(u => !u.IsApproved && u.EmailConfirmed);
            ViewBag.TotalPlanets = await _context.Planets.CountAsync();

            return View();
        }

       
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    u.Email,
                    u.IsBlocked,
                    u.IsApproved,
                    u.EmailConfirmed,
                    u.CreatedAt,
                    TaskCount = u.Tasks.Count,
                    SessionCount = u.Sessions.Count
                })
                .ToListAsync();

            return Json(users);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
                return NotFound(new { message = "User not found." });

            user.IsApproved = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to approve user." });

            _logger.LogInformation("User {UserId} ({Email}) approved by admin {Admin}",
                user.Id, user.Email, User.Identity?.Name);

            return Ok(new { message = $"User {user.Email} has been approved." });
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
                return NotFound(new { message = "User not found." });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
                return Unauthorized(new { message = "Current admin user not found." });

            if (currentUser.Id == id)
                return BadRequest(new { message = "Cannot block yourself." });

            user.IsBlocked = true;
            await _userManager.UpdateAsync(user);

            
            await _userManager.UpdateSecurityStampAsync(user);

            _logger.LogWarning("User {UserId} ({Email}) blocked by {Admin}",
                user.Id, user.Email, User.Identity?.Name);

            return Ok(new { message = $"User {user.Email} has been blocked." });
        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
                return NotFound(new { message = "User not found." });

            user.IsBlocked = false;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {UserId} ({Email}) unblocked by {Admin}",
                user.Id, user.Email, User.Identity?.Name);

            return Ok(new { message = $"User {user.Email} has been unblocked." });
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
                return NotFound(new { message = "User not found." });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
                return Unauthorized(new { message = "Current admin user not found." });

            if (currentUser.Id == id)
                return BadRequest(new { message = "Cannot promote yourself." });

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return BadRequest(new { message = "User is already an admin." });

            await _userManager.RemoveFromRoleAsync(user, "User");
            await _userManager.AddToRoleAsync(user, "Admin");
            user.Role = FocusSpace.Domain.Enums.UserRole.Admin;
            await _userManager.UpdateAsync(user);

            _logger.LogWarning("User {UserId} promoted to Admin by {Admin}",
                user.Id, User.Identity?.Name);

            return Ok(new { message = $"User {user.Email} has been promoted to Admin." });
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
                return NotFound(new { message = "User not found." });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
                return Unauthorized(new { message = "Current admin user not found." });

            if (currentUser.Id == id)
                return BadRequest(new { message = "Cannot delete yourself." });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to delete user." });

            _logger.LogWarning("User {UserId} ({Email}) deleted by {Admin}",
                user.Id, user.Email, User.Identity?.Name);

            return Ok(new { message = $"User {user.Email} has been deleted." });
        }

        [HttpGet]
        public async Task<IActionResult> GetPlanets()
        {
            var planets = await _context.Planets
                .OrderBy(p => p.OrderNumber)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.OrderNumber,
                    p.Description,
                    p.DistanceFromPrevious,
                    p.ImageUrl,
                    UsersCount = p.Users.Count
                })
                .ToListAsync();

            return Json(planets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePlanet([FromBody] CreatePlanetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Planet name is required." });

            if (request.OrderNumber <= 0)
                return BadRequest(new { message = "Order number must be greater than zero." });

            var normalizedName = request.Name.Trim();
            var nameExists = await _context.Planets.AnyAsync(p => p.Name.ToLower() == normalizedName.ToLower());
            if (nameExists)
                return BadRequest(new { message = "Planet with this name already exists." });

            var orderExists = await _context.Planets.AnyAsync(p => p.OrderNumber == request.OrderNumber);
            if (orderExists)
                return BadRequest(new { message = "Planet with this order number already exists." });

            var planet = new Planet
            {
                Name = normalizedName,
                OrderNumber = request.OrderNumber,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                DistanceFromPrevious = request.DistanceFromPrevious,
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim()
            };

            _context.Planets.Add(planet);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Planet {PlanetName} created by admin {Admin}",
                planet.Name, User.Identity?.Name);

            return Ok(new { message = $"Planet {planet.Name} created successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePlanet(int id)
        {
            var planet = await _context.Planets
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (planet is null)
                return NotFound(new { message = "Planet not found." });

            if (planet.Users.Any())
                return BadRequest(new { message = "Cannot delete planet that is assigned to users." });

            _context.Planets.Remove(planet);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Planet {PlanetName} deleted by admin {Admin}",
                planet.Name, User.Identity?.Name);

            return Ok(new { message = $"Planet {planet.Name} deleted." });
        }

        public sealed class CreatePlanetRequest
        {
            public string Name { get; set; } = string.Empty;
            public int OrderNumber { get; set; }
            public string? Description { get; set; }
            public TimeSpan? DistanceFromPrevious { get; set; }
            public string? ImageUrl { get; set; }
        }
    }
}