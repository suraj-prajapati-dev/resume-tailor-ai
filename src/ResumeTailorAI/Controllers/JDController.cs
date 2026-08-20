using Microsoft.AspNetCore.Mvc;
using ResumeTailorAI.DTOs;
using ResumeTailorAI.Models;
using ResumeTailorAI.Services;

namespace ResumeTailorAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JDController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IFileService _fileService;
    private readonly IDocumentParserService _documentParser;
    private readonly IJobDescriptionParserService _jdParser;
    private readonly ILogger<JDController> _logger;

    public JDController(
        ISessionService sessionService,
        IFileService fileService,
        IDocumentParserService documentParser,
        IJobDescriptionParserService jdParser,
        ILogger<JDController> logger)
    {
        _sessionService = sessionService;
        _fileService = fileService;
        _documentParser = documentParser;
        _jdParser = jdParser;
        _logger = logger;
    }

    private string GetSessionId()
    {
        return HttpContext.Session.GetString("SessionId") ?? string.Empty;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadJD(IFormFile file)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        return await UploadFileInternal(file, cts.Token);
    }

    private async Task<ActionResult<ApiResponse<UploadResponse>>> UploadFileInternal(IFormFile file, CancellationToken ct)
    {
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

            var isValid = await _fileService.ValidateFileAsync(file, ct);
            if (!isValid)
            {
                return BadRequest(new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "Invalid file",
                    Errors = new List<string> { "File type, size, or MIME type not supported" }
                });
            }

            var filePath = await _fileService.SaveFileAsync(file, sessionId, ct);
            
            var docText = await _documentParser.ParseAsync(filePath, ct);
            var jdModel = await _jdParser.ParseAsync(docText, session.TargetRole, ct);

            session.JDFilePath = filePath;
            session.JDText = jdModel.ExtractedText ?? docText.ExtractedText;

            _sessionService.UpdateSession(session);

            _logger.LogInformation("JD uploaded for session {SessionId}: {FileName}", sessionId, file.FileName);

            return Ok(new ApiResponse<UploadResponse>
            {
                Success = true,
                Message = "Job Description uploaded successfully",
                Data = new UploadResponse
                {
                    FileName = file.FileName,
                    FileType = Path.GetExtension(file.FileName),
                    Size = (int)file.Length,
                    Message = "File processed successfully"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload JD");
            return StatusCode(500, new ApiResponse<UploadResponse>
            {
                Success = false,
                Message = "Failed to process file",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
