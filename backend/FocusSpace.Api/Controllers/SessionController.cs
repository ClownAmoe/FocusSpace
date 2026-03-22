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
        if (!ModelState.IsValid) return BadRequest(ModelState);
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
}