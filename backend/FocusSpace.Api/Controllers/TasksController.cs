using FocusSpace.Application.DTOs;
using FocusSpace.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FocusSpace.Api.Controllers
{
    /// <summary>
    /// MVC controller that handles the Task Management use-case.
    /// Provides CRUD operations and renders Razor views.
    /// </summary>
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        // Temporary hardcoded user until auth is implemented
        private const int CurrentUserId = 5;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        // GET /Tasks
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("User {UserId} is viewing their task list", CurrentUserId);

            var tasks = await _taskService.GetTasksByUserIdAsync(CurrentUserId);
            return View(tasks);
        }

        // GET /Tasks/Details/5
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("User {UserId} requested details for task {TaskId}", CurrentUserId, id);

            var task = await _taskService.GetTaskByIdAsync(id);

            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} not found", id);
                return NotFound();
            }

            return View(task);
        }

        // GET /Tasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST /Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            dto.UserId = CurrentUserId;

            _logger.LogInformation("User {UserId} is creating task '{Title}'", CurrentUserId, dto.Title);

            try
            {
                await _taskService.CreateTaskAsync(dto);
                TempData["Success"] = "Task created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating task for user {UserId}", CurrentUserId);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET /Tasks/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task is null)
                return NotFound();

            var dto = new UpdateTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description
            };

            return View(dto);
        }

        // POST /Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateTaskDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(dto);

            _logger.LogInformation("User {UserId} is editing task {TaskId}", CurrentUserId, id);

            try
            {
                var result = await _taskService.UpdateTaskAsync(dto);

                if (result is null)
                    return NotFound();

                TempData["Success"] = "Task updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating task {TaskId}", id);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET /Tasks/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task is null)
                return NotFound();

            return View(task);
        }

        // POST /Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("User {UserId} is deleting task {TaskId}", CurrentUserId, id);

            var deleted = await _taskService.DeleteTaskAsync(id);

            if (!deleted)
                return NotFound();

            TempData["Success"] = "Task deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
