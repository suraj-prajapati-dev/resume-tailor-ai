using Microsoft.AspNetCore.Mvc;
using ResumeTailorAI.Agents;
using ResumeTailorAI.DTOs;
using ResumeTailorAI.Models;
using ResumeTailorAI.Services;

namespace ResumeTailorAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TailoringController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IDocumentGenerationService _docGenService;
    private readonly ILogger<TailoringController> _logger;

    public TailoringController(
        ISessionService sessionService,
        IDocumentGenerationService docGenService,
        ILogger<TailoringController> logger)
    {
        _sessionService = sessionService;
        _docGenService = docGenService;
        _logger = logger;
    }

    private string GetSessionId()
    {
        return HttpContext.Session.GetString("SessionId") ?? string.Empty;
    }

    [HttpPost("generate-preview")]
    public async Task<ActionResult<ApiResponse<object>>> GeneratePreview([FromBody] GeneratePreviewRequest request)
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

        _logger.LogInformation("Generating preview for session {SessionId}", sessionId);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Preview generated",
            Data = request.TailoringResult?.TailoredResume
        });
    }

    [HttpPost("approve")]
    public async Task<ActionResult<ApiResponse<GenerateDocumentsResponse>>> Approve([FromBody] ApproveRequest request)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized(new ApiResponse<GenerateDocumentsResponse>
            {
                Success = false,
                Message = "No active session"
            });
        }

        var session = _sessionService.GetSession(sessionId);
        if (session == null)
        {
            return NotFound(new ApiResponse<GenerateDocumentsResponse>
            {
                Success = false,
                Message = "Session not found"
            });
        }

        if (!request.Approved)
        {
            session.ApprovalStatus = ApprovalStatus.Rejected;
            _sessionService.UpdateSession(session);
            
            return Ok(new ApiResponse<GenerateDocumentsResponse>
            {
                Success = true,
                Message = "Approval rejected"
            });
        }

        try
        {
            var sessionPath = Path.Combine(AppContext.BaseDirectory, "App_Data", "TempSessions", sessionId);
            Directory.CreateDirectory(sessionPath);

            if (session.TailoringResult != null && session.GuardrailResult?.Status == "PASS")
            {
                var resumePath = await _docGenService.GenerateResumeAsync(
                    session.TailoringResult, sessionPath, cts.Token);
                
                var coverLetterPath = await _docGenService.GenerateCoverLetterAsync(
                    session.TailoringResult, session.JDAnalysis ?? new JDAnalysisModel(),
                    session.TargetRole, sessionPath, cts.Token);

                session.GeneratedResumePath = resumePath;
                session.GeneratedCoverLetterPath = coverLetterPath;
                session.ApprovalStatus = ApprovalStatus.Approved;
                _sessionService.UpdateSession(session);

                _logger.LogInformation("Documents generated for session {SessionId}", sessionId);

                return Ok(new ApiResponse<GenerateDocumentsResponse>
                {
                    Success = true,
                    Message = "Documents generated successfully",
                    Data = new GenerateDocumentsResponse
                    {
                        ResumeDownloadUrl = "/api/documents/resume",
                        CoverLetterDownloadUrl = "/api/documents/cover-letter"
                    }
                });
            }
            else
            {
                return BadRequest(new ApiResponse<GenerateDocumentsResponse>
                {
                    Success = false,
                    Message = "Cannot generate documents - guardrail validation required"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate documents for session {SessionId}", sessionId);
            return StatusCode(500, new ApiResponse<GenerateDocumentsResponse>
            {
                Success = false,
                Message = "Failed to generate documents",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("preview")]
    public async Task<ActionResult<ApiResponse<ResumeAnalysisOutputModel>>> GetPreview()
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized(new ApiResponse<ResumeAnalysisOutputModel>
            {
                Success = false,
                Message = "No active session"
            });
        }

        var session = _sessionService.GetSession(sessionId);
        if (session == null)
        {
            return NotFound(new ApiResponse<ResumeAnalysisOutputModel>
            {
                Success = false,
                Message = "Session not found"
            });
        }

        if (session.GuardrailResult == null)
        {
            return BadRequest(new ApiResponse<ResumeAnalysisOutputModel>
            {
                Success = false,
                Message = "Analysis not complete"
            });
        }

        var match = session.SkillMatchResult;
        var response = new ResumeAnalysisOutputModel
        {
            TargetRole = session.TargetRole,
            OverallMatchScore = match?.OverallMatchScore ?? 0,
            ResumeSummary = session.AnalysisResult?.ProfessionalSummary ?? "",
            JdSummary = BuildJdSummary(session.JDAnalysis),
            MatchedSkills = match?.MatchedSkills ?? new(),
            PartialMatches = match?.PartialMatches ?? new(),
            MissingSkills = match?.MissingSkills ?? new(),
            ExperienceMatch = match?.ExperienceMatch ?? new(),
            AtsAnalysis = session.AtsResult ?? new ATSAnalysisModel(),
            TailoringRecommendations = session.TailoringResult?.Changes?
                .Select(c => new TailoringRecommendation
                {
                    Id = 0,
                    Description = c.Reason,
                    Priority = 1,
                    Section = c.Section
                }).ToList() ?? new(),
            TailoredResume = session.TailoringResult?.TailoredResume ?? new(),
            Guardrail = session.GuardrailResult,
            RequiresHumanApproval = true
        };

        return Ok(new ApiResponse<ResumeAnalysisOutputModel>
        {
            Success = true,
            Message = "Preview retrieved",
            Data = response
        });
    }

    private static string BuildJdSummary(JDAnalysisModel? jd)
    {
        if (jd == null) return "No JD analysis available";
        
        var summary = "Target Role: " + jd.TargetRole + ". ";
        summary += "Required skills: " + (jd.RequiredSkills?.Count ?? 0) + ". ";
        summary += "Responsibilities identified: " + (jd.Responsibilities?.Count ?? 0) + ".";
        return summary;
    }
}
