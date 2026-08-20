using Microsoft.AspNetCore.Mvc;
using ResumeTailorAI.Agents;
using ResumeTailorAI.DTOs;
using ResumeTailorAI.Models;
using ResumeTailorAI.Services;

namespace ResumeTailorAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IAgentOrchestrator _orchestrator;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        ISessionService sessionService,
        IAgentOrchestrator orchestrator,
        ILogger<AnalysisController> logger)
    {
        _sessionService = sessionService;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    private string GetSessionId()
    {
        return HttpContext.Session.GetString("SessionId") ?? string.Empty;
    }

    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<AnalysisStatusResponse>>> StartAnalysis()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        try
        {
            var sessionId = GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                return Unauthorized(new ApiResponse<AnalysisStatusResponse>
                {
                    Success = false,
                    Message = "No active session"
                });
            }

            var session = _sessionService.GetSession(sessionId);
            if (session == null)
            {
                return NotFound(new ApiResponse<AnalysisStatusResponse>
                {
                    Success = false,
                    Message = "Session not found"
                });
            }

            if (string.IsNullOrEmpty(session.ResumeText))
            {
                return BadRequest(new ApiResponse<AnalysisStatusResponse>
                {
                    Success = false,
                    Message = "Resume not uploaded"
                });
            }

            if (string.IsNullOrEmpty(session.JDText))
            {
                return BadRequest(new ApiResponse<AnalysisStatusResponse>
                {
                    Success = false,
                    Message = "Job description not uploaded"
                });
            }

            session.IsLocked = true;
            _sessionService.UpdateSession(session);

            _logger.LogInformation("Starting analysis for session {SessionId}", sessionId);

            var context = await _orchestrator.RunAnalysisAsync(sessionId, cts.Token);

            if (!string.IsNullOrEmpty(context.ErrorMessage))
            {
                session.IsLocked = false;
                _sessionService.UpdateSession(session);
                
                return StatusCode(500, new ApiResponse<AnalysisStatusResponse>
                {
                    Success = false,
                    Message = context.ErrorMessage,
                    Errors = context.Errors
                });
            }

            _logger.LogInformation("Analysis completed for session {SessionId}", sessionId);

            return Ok(new ApiResponse<AnalysisStatusResponse>
            {
                Success = true,
                Message = "Analysis completed successfully",
                Data = new AnalysisStatusResponse
                {
                    IsComplete = true,
                    ProgressPercentage = 100,
                    CompletedSteps = new List<string>
                    {
                        "Resume Intelligence",
                        "JD Intelligence",
                        "Skill Matching",
                        "Resume Tailoring",
                        "ATS Validation",
                        "Guardrail Validation"
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed");
            return StatusCode(500, new ApiResponse<AnalysisStatusResponse>
            {
                Success = false,
                Message = "Analysis failed",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<AnalysisStatusResponse>>> GetStatus()
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized(new ApiResponse<AnalysisStatusResponse>
            {
                Success = false,
                Message = "No active session"
            });
        }

        var session = _sessionService.GetSession(sessionId);
        if (session == null)
        {
            return NotFound(new ApiResponse<AnalysisStatusResponse>
            {
                Success = false,
                Message = "Session not found"
            });
        }

        var isComplete = session.GuardrailResult != null;
        
        return Ok(new ApiResponse<AnalysisStatusResponse>
        {
            Success = true,
            Message = "Status retrieved",
            Data = new AnalysisStatusResponse
            {
                IsComplete = isComplete,
                ProgressPercentage = isComplete ? 100 : 0,
                CompletedSteps = new List<string>(),
                CurrentStep = isComplete ? "Complete" : "In Progress"
            }
        });
    }

    [HttpGet("result")]
    public async Task<ActionResult<ApiResponse<AnalysisResultResponse>>> GetResult()
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized(new ApiResponse<AnalysisResultResponse>
            {
                Success = false,
                Message = "No active session"
            });
        }

        var session = _sessionService.GetSession(sessionId);
        if (session == null)
        {
            return NotFound(new ApiResponse<AnalysisResultResponse>
            {
                Success = false,
                Message = "Session not found"
            });
        }

        if (session.GuardrailResult == null)
        {
            return BadRequest(new ApiResponse<AnalysisResultResponse>
            {
                Success = false,
                Message = "Analysis not complete"
            });
        }

        var match = session.SkillMatchResult;
        var response = new AnalysisResultResponse
        {
            TargetRole = session.TargetRole,
            OverallMatchScore = match?.OverallMatchScore ?? 0,
            ResumeSummary = session.AnalysisResult?.ProfessionalSummary ?? "",
            JdSummary = BuildJdSummary(session.JDAnalysis),
            MatchedSkills = match?.MatchedSkills ?? new(),
            PartialMatches = (match?.PartialMatches ?? new())
                .Select(p => new PartialMatchDto
                {
                    Skill = p.Skill,
                    Gap = p.Gap,
                    Recommendation = p.Recommendation
                }).ToList(),
            MissingSkills = (match?.MissingSkills ?? new())
                .Select(m => new MissingSkillDto
                {
                    Skill = m.Skill,
                    Priority = m.Priority,
                    Action = m.Action
                }).ToList(),
            ExperienceMatch = new ExperienceMatchDto
            {
                RequiredYears = match?.ExperienceMatch.RequiredYears ?? 0,
                CandidateYears = match?.ExperienceMatch.CandidateYears ?? 0,
                Match = match?.ExperienceMatch.Match ?? "Unknown",
                Details = match?.ExperienceMatch.Details ?? ""
            },
            AtsAnalysis = session.AtsResult ?? new ATSAnalysisModel(),
            TailoringRecommendations = new List<TailoringRecommendationDto>(),
            TailoredResume = session.TailoringResult?.TailoredResume ?? new(),
            Guardrail = session.GuardrailResult ?? new GuardrailResultModel(),
            RequiresHumanApproval = true
        };

        if (session.TailoringResult?.Changes?.Any() == true)
        {
            var index = 0;
            foreach (var change in session.TailoringResult.Changes.Take(10))
            {
                response.TailoringRecommendations.Add(new TailoringRecommendationDto
                {
                    Id = ++index,
                    Description = change.Reason,
                    Priority = 1,
                    Section = change.Section
                });
            }
        }

        return Ok(new ApiResponse<AnalysisResultResponse>
        {
            Success = true,
            Message = "Analysis results retrieved",
            Data = response
        });
    }

    private static string BuildJdSummary(JDAnalysisModel? jd)
    {
        if (jd == null) return "No JD analysis available";
        
        var summary = $"Target Role: {jd.TargetRole}. ";
        summary += $"Required skills: {jd.RequiredSkills?.Count ?? 0}. ";
        summary += $"Responsibilities identified: {jd.Responsibilities?.Count ?? 0}.";
        return summary;
    }
}
