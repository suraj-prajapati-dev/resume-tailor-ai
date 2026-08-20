using Microsoft.AspNetCore.Mvc;
using ResumeTailorAI.DTOs;
using ResumeTailorAI.Models;
using ResumeTailorAI.Services;

namespace ResumeTailorAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IFileService _fileService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        ISessionService sessionService,
        IFileService fileService,
        ILogger<DocumentsController> logger)
    {
        _sessionService = sessionService;
        _fileService = fileService;
        _logger = logger;
    }

    private string GetSessionId()
    {
        return HttpContext.Session.GetString("SessionId") ?? string.Empty;
    }

    [HttpGet("resume")]
    public async Task<IActionResult> DownloadResume()
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "No active session"
            });
        }

        var session = _sessionService.GetSession(sessionId);
        if (session == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Session not found"
            });
        }

        if (session.ApprovalStatus != ApprovalStatus.Approved ||
            string.IsNullOrEmpty(session.GeneratedResumePath) ||
            !System.IO.File.Exists(session.GeneratedResumePath))
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Resume not generated or approved"
            });
        }

        var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        var fileBytes = await System.IO.File.ReadAllBytesAsync(session.GeneratedResumePath);
        
        _logger.LogInformation("Resume downloaded for session {SessionId}", sessionId);
        
        return File(fileBytes, contentType, "TailoredResume.docx");
    }

    [HttpGet("cover-letter")]
    public async Task<IActionResult> DownloadCoverLetter()
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "No active session"
            });
        }

        var session = _sessionService.GetSession(sessionId);
        if (session == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Session not found"
            });
        }

        if (session.ApprovalStatus != ApprovalStatus.Approved ||
            string.IsNullOrEmpty(session.GeneratedCoverLetterPath) ||
            !System.IO.File.Exists(session.GeneratedCoverLetterPath))
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Cover letter not generated or approved"
            });
        }

        var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        var fileBytes = await System.IO.File.ReadAllBytesAsync(session.GeneratedCoverLetterPath);
        
        _logger.LogInformation("Cover letter downloaded for session {SessionId}", sessionId);
        
        return File(fileBytes, contentType, "CoverLetter.docx");
    }
}