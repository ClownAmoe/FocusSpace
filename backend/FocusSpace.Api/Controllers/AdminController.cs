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
            if (currentUser.Id == id)
                return BadRequest(new { message = "Cannot delete yourself." });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to delete user." });

            _logger.LogWarning("User {UserId} ({Email}) deleted by {Admin}",
                user.Id, user.Email, User.Identity?.Name);

            return Ok(new { message = $"User {user.Email} has been deleted." });
        }
    }
}