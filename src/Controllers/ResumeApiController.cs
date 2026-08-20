using Microsoft.AspNetCore.Mvc;
using ResumeTailorAI.DTOs;
using ResumeTailorAI.Services;

namespace ResumeTailorAI.Controllers;

[ApiController]
[Route("api/resume")]
public class ResumeApiController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IFileService _fileService;
    private readonly IDocumentParserService _parserService;
    private readonly IResumeParserService _resumeParser;
    private readonly ILogger<ResumeApiController> _logger;

    public ResumeApiController(
        ISessionService sessionService,
        IFileService fileService,
        IDocumentParserService parserService,
        IResumeParserService resumeParser,
        ILogger<ResumeApiController> logger)
    {
        _sessionService = sessionService;
        _fileService = fileService;
        _parserService = parserService;
        _resumeParser = resumeParser;
        _logger = logger;
    }

    private string GetSessionId()
    {
        return HttpContext.Session.GetString("SessionId") ?? string.Empty;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadResume([FromForm] IFormFile file)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        
        try
        {
            var sessionId = GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                return Unauthorized(new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "No active session"
                });
            }

            var session = _sessionService.GetSession(sessionId);
            if (session == null)
            {
                return NotFound(new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "Session not found"
                });
            }

            if (session.IsLocked)
            {
                return BadRequest(new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "Session is locked. Analysis already started."
                });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "File is empty or not provided"
                });
            }

            var isValid = await _fileService.ValidateFileAsync(file, cts.Token);
            if (!isValid)
            {
                return BadRequest(new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "Invalid file",
                    Errors = new List<string> { "File type, size, or MIME type not supported" }
                });
            }

            var filePath = await _fileService.SaveFileAsync(file, sessionId, cts.Token);
            var docText = await _parserService.ParseAsync(filePath, cts.Token);

            session.ResumeFilePath = filePath;
            session.ResumeText = docText.ExtractedText;

            await _resumeParser.ParseAsync(docText, cts.Token);
            
            _sessionService.UpdateSession(session);

            _logger.LogInformation("Resume uploaded for session {SessionId}: {FileName}", 
                sessionId, file.FileName);

            return Ok(new ApiResponse<UploadResponse>
            {
                Success = true,
                Message = "Resume uploaded and parsed successfully",
                Data = new UploadResponse
                {
                    FileName = file.FileName,
                    FileType = Path.GetExtension(file.FileName),
                    Size = (int)file.Length,
                    Message = $"Extracted {docText.CharacterCount} characters"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload resume");
            return StatusCode(500, new ApiResponse<UploadResponse>
            {
                Success = false,
                Message = "Failed to process file",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
