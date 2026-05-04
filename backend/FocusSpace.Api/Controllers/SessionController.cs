using FocusSpace.Application.DTOs;
using FocusSpace.Application.Interfaces;
using FocusSpace.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FocusSpace.Api.Controllers;

[Authorize(Roles = "User,Admin")]
public class SessionController : Controller
{
    private readonly ISessionService _sessionService;
    private readonly UserManager<User> _userManager;

    public SessionController(ISessionService sessionService, UserManager<User> userManager)
    {
        _sessionService = sessionService;
        _userManager = userManager;
    }

    private async Task<int> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id ?? throw new InvalidOperationException("Authenticated user not found.");
    }

    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Start([FromBody] StartSessionRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = await GetCurrentUserIdAsync();
        var dto = new CreateSessionDto
        {
            UserId = userId,
            TaskId = request.TaskId,
            PlannedDuration = TimeSpan.FromMinutes(request.PlannedMinutes)
        };

        var sessionId = await _sessionService.StartSessionAsync(dto);
        return Ok(new { sessionId });
    }

    [HttpPost]
    public async Task<IActionResult> Complete([FromBody] UpdateSessionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _sessionService.CompleteSessionAsync(dto);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Pause([FromBody] int sessionId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _sessionService.PauseSessionAsync(sessionId);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Resume([FromBody] int sessionId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _sessionService.ResumeSessionAsync(sessionId);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetUserSessions()
    {
        var userId = await GetCurrentUserIdAsync();
        var sessions = await _sessionService.GetSessionsByUserIdAsync(userId);
        return Json(sessions);
    }

    [HttpGet]
    public async Task<IActionResult> GetFocusRecommendation()
    {
        var userId = await GetCurrentUserIdAsync();
        var recommendation = await _sessionService.GetFocusRecommendationAsync(userId);
        return Json(recommendation);
    }
}

public sealed class StartSessionRequest
{
    public int? TaskId { get; set; }
    public int PlannedMinutes { get; set; }
}