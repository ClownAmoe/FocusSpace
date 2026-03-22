using FocusSpace.Application.DTOs;
using FocusSpace.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FocusSpace.Api.Controllers;

public class SessionController : Controller
{
    private readonly ISessionService _sessionService;

    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Start([FromBody] CreateSessionDto dto)
    {
        var sessionId = await _sessionService.StartSessionAsync(dto);
        return Ok(new { sessionId });
    }

    [HttpPost]
    public async Task<IActionResult> Complete([FromBody] UpdateSessionDto dto)
    {
        await _sessionService.CompleteSessionAsync(dto);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Pause([FromBody] int sessionId)
    {
        await _sessionService.PauseSessionAsync(sessionId);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Resume([FromBody] int sessionId)
    {
        await _sessionService.ResumeSessionAsync(sessionId);
        return Ok();
    }
}