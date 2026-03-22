using FocusSpace.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainTask = FocusSpace.Domain.Entities.Task;

namespace FocusSpace.Api.Controllers
{
  
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

   
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin panel accessed");

            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveTasks = await _context.Tasks.CountAsync(),
                ActiveSessions = await _context.Sessions.Where(s => s.Status == FocusSpace.Domain.Enums.SessionStatus.Ongoing).CountAsync(),
                BlockedUsers = await _context.Users.Where(u => u.IsBlocked).CountAsync()
            };

            ViewBag.TotalUsers = stats.TotalUsers;
            ViewBag.ActiveTasks = stats.ActiveTasks;
            ViewBag.ActiveSessions = stats.ActiveSessions;
            ViewBag.BlockedUsers = stats.BlockedUsers;

            return View();
        }

       
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Fetching users list for admin panel");

            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.IsBlocked,
                    u.CreatedAt,
                    TaskCount = u.Tasks.Count,
                    SessionCount = u.Sessions.Count
                })
                .ToListAsync();

            return Json(users);
        }

       
        [HttpPost]
        public async Task<IActionResult> BlockUser(int id)
        {
            _logger.LogInformation("Attempting to block user {UserId}", id);

            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.IsBlocked = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogWarning("User {UserId} ({Email}) has been blocked", user.Id, user.Email);

            return Ok(new { message = $"User {user.Email} has been blocked" });
        }

        
        [HttpPost]
        public async Task<IActionResult> UnblockUser(int id)
        {
            _logger.LogInformation("Attempting to unblock user {UserId}", id);

            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.IsBlocked = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogWarning("User {UserId} ({Email}) has been unblocked", user.Id, user.Email);

            return Ok(new { message = $"User {user.Email} has been unblocked" });
        }
    }
}
