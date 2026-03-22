using FocusSpace.Application.DTOs;
using FocusSpace.Application.Interfaces;
using FocusSpace.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FocusSpace.Api.Controllers
{
    /// <summary>
    /// Task management — requires authenticated + approved users (role: User or Admin).
    /// </summary>
    [Authorize(Roles = "User,Admin")]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
            ITaskService taskService,
            UserManager<User> userManager,
            ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _userManager = userManager;
            _logger = logger;
        }

        // Helper — resolves the current user's integer ID from the claims principal.
        private async Task<int> GetCurrentUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id ?? throw new InvalidOperationException("Authenticated user not found in database.");
        }

        // GET /Tasks
        public async Task<IActionResult> Index()
        {
            var userId = await GetCurrentUserIdAsync();
            _logger.LogInformation("User {UserId} is viewing their task list", userId);

            var tasks = await _taskService.GetTasksByUserIdAsync(userId);
            return View(tasks);
        }

        // GET /Tasks/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = await GetCurrentUserIdAsync();
            _logger.LogInformation("User {UserId} requested details for task {TaskId}", userId, id);

            var task = await _taskService.GetTaskByIdAsync(id);
            if (task is null || task.UserId != userId)
                return NotFound();

            return View(task);
        }

        // GET /Tasks/Create
        public IActionResult Create() => View();

        // POST /Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            dto.UserId = await GetCurrentUserIdAsync();
            _logger.LogInformation("User {UserId} creating task '{Title}'", dto.UserId, dto.Title);

            try
            {
                await _taskService.CreateTaskAsync(dto);
                TempData["Success"] = "Task created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET /Tasks/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = await GetCurrentUserIdAsync();
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task is null || task.UserId != userId)
                return NotFound();

            return View(new UpdateTaskDto { Id = task.Id, Title = task.Title, Description = task.Description });
        }

        // POST /Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateTaskDto dto)
        {
            if (id != dto.Id) return BadRequest();
            if (!ModelState.IsValid) return View(dto);

            // Ownership check
            var userId = await GetCurrentUserIdAsync();
            var existing = await _taskService.GetTaskByIdAsync(id);
            if (existing is null || existing.UserId != userId)
                return NotFound();

            try
            {
                var result = await _taskService.UpdateTaskAsync(dto);
                if (result is null) return NotFound();

                TempData["Success"] = "Task updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET /Tasks/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = await GetCurrentUserIdAsync();
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task is null || task.UserId != userId)
                return NotFound();

            return View(task);
        }

        // POST /Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = await GetCurrentUserIdAsync();
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task is null || task.UserId != userId)
                return NotFound();

            await _taskService.DeleteTaskAsync(id);
            TempData["Success"] = "Task deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Tasks/GetUserTasks
        [HttpGet]
        public async Task<IActionResult> GetUserTasks()
        {
            var tasks = await _taskService.GetTasksByUserIdAsync(CurrentUserId);
            return Json(tasks);
        }
    }
}