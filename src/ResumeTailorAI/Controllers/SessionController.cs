using Microsoft.AspNetCore.Mvc;
using ResumeTailorAI.DTOs;
using ResumeTailorAI.Models;
using ResumeTailorAI.Services;

namespace ResumeTailorAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IFileService _fileService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(
        ISessionService sessionService,
        IFileService fileService,
        ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _fileService = fileService;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<StartSessionResponse>>> StartSession([FromBody] StartSessionRequest request)
    {
        try
        {
            var session = await _sessionService.CreateSessionAsync(request.TargetRole);
            
            HttpContext.Session.SetString("SessionId", session.SessionId);
            
            _logger.LogInformation("Session started: {SessionId}", session.SessionId);
            
            return Ok(new ApiResponse<StartSessionResponse>
            {
                Success = true,
                Message = "Session created successfully",
                Data = new StartSessionResponse { SessionId = session.SessionId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start session");
            return StatusCode(500, new ApiResponse<StartSessionResponse>
            {
                Success = false,
                Message = "Failed to create session",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("status")]
    public ActionResult<ApiResponse<object>> GetSessionStatus()
    {
        var sessionId = HttpContext.Session.GetString("SessionId");
        if (string.IsNullOrEmpty(sessionId))
        {
            return Ok(new ApiResponse<object>
            {
                Success = false,
                Message = "No active session"
            });
        }

        var session = _sessionService.GetSession(sessionId);
        if (session == null)
        {
            HttpContext.Session.Remove("SessionId");
            return Ok(new ApiResponse<object>
            {
                Success = false,
                Message = "Session expired"
            });
        }

        _sessionService.UpdateLastAccessed(sessionId);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Session active",
            Data = new
            {
                sessionId = session.SessionId,
                targetRole = session.TargetRole,
                createdAt = session.CreatedAt,
                hasResume = !string.IsNullOrEmpty(session.ResumeText),
                hasJd = !string.IsNullOrEmpty(session.JDText),
                analysisComplete = session.GuardrailResult != null
            }
        });
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        try
        {
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (!string.IsNullOrEmpty(sessionId))
            {
                await _sessionService.DeleteSessionAsync(sessionId);
                HttpContext.Session.Remove("SessionId");
            }

            _logger.LogInformation("Session ended: {SessionId}", sessionId ?? "unknown");
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Session ended successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to logout");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to end session"
            });
        }
    }
}
