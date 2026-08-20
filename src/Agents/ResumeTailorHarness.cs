using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResumeTailorAI.Configuration;
using ResumeTailorAI.Models;
using ResumeTailorAI.Services;
using ResumeTailorAI.Skills;

namespace ResumeTailorAI.Agents;

public interface IResumeTailorHarness
{
    Task<ResumeAnalysisModel> ExecuteResumeIntelligenceAsync(string resumeText, string sessionId, CancellationToken ct = default);
    Task<JDAnalysisModel> ExecuteJDIntelligenceAsync(string jdText, string targetRole, string sessionId, CancellationToken ct = default);
    Task<SkillMatchResultModel> ExecuteSkillMatchingAsync(ResumeAnalysisModel resume, JDAnalysisModel jd, string sessionId, CancellationToken ct = default);
    Task<TailoringResultModel> ExecuteResumeTailoringAsync(ResumeAnalysisModel resume, JDAnalysisModel jd, SkillMatchResultModel match, string targetRole, string sessionId, CancellationToken ct = default);
    Task<ATSAnalysisModel> ExecuteATSValidationAsync(TailoringResultModel tailoredResume, JDAnalysisModel jd, string originalResumeText, string sessionId, CancellationToken ct = default);
    Task<GuardrailResultModel> ExecuteGuardrailsAsync(TailoringResultModel tailoredResume, string originalResumeText, ResumeAnalysisModel resume, SkillMatchResultModel match, string sessionId, CancellationToken ct = default);
}

public class ResumeTailorHarness : IResumeTailorHarness
{
    private readonly IAIService _aiService;
    private readonly ISkillMatchingService _skillMatchingService;
    private readonly IAtsValidationService _atsValidationService;
    private readonly ILogger<ResumeTailorHarness> _logger;
    private readonly AppConfiguration _config;
    private readonly SkillLoader _skillLoader;

    public ResumeTailorHarness(
        IAIService aiService,
        ISkillMatchingService skillMatchingService,
        IAtsValidationService atsValidationService,
        IOptions<AppConfiguration> config,
        ILogger<ResumeTailorHarness> logger)
    {
        _aiService = aiService;
        _skillMatchingService = skillMatchingService;
        _atsValidationService = atsValidationService;
        _config = config.Value;
        _logger = logger;
        _skillLoader = new SkillLoader(Path.Combine(AppContext.BaseDirectory, "Skills"));
    }

    public async Task<ResumeAnalysisModel> ExecuteResumeIntelligenceAsync(string resumeText, string sessionId, CancellationToken ct = default)
    {
        _logger.LogInformation("Resume Intelligence skill started for session {SessionId}", sessionId);
        
        var result = await _aiService.AnalyzeResumeAsync(resumeText, ct);
        
        if (result == null)
        {
            _logger.LogWarning("Resume Intelligence returned null for session {SessionId}", sessionId);
            result = new ResumeAnalysisModel();
        }

        _logger.LogInformation("Resume Intelligence completed for session {SessionId}. Skills found: {SkillCount}", 
            sessionId, result.Skills?.Count ?? 0);
        
        return result;
    }

    public async Task<JDAnalysisModel> ExecuteJDIntelligenceAsync(string jdText, string targetRole, string sessionId, CancellationToken ct = default)
    {
        _logger.LogInformation("JD Intelligence skill started for session {SessionId}", sessionId);
        
        var result = await _aiService.AnalyzeJobDescriptionAsync(jdText, targetRole, ct);
        
        if (result == null)
        {
            _logger.LogWarning("JD Intelligence returned null for session {SessionId}", sessionId);
            result = new JDAnalysisModel { TargetRole = targetRole };
        }

        _logger.LogInformation("JD Intelligence completed for session {SessionId}. Requirements: {ReqCount}",
            sessionId, result.RequiredSkills?.Count ?? 0);
        
        return result;
    }

    public async Task<SkillMatchResultModel> ExecuteSkillMatchingAsync(ResumeAnalysisModel resume, JDAnalysisModel jd, string sessionId, CancellationToken ct = default)
    {
        _logger.LogInformation("Skill Matching skill started for session {SessionId}", sessionId);
        
        var result = await _skillMatchingService.MatchAsync(resume, jd, ct);
        
        _logger.LogInformation("Skill Matching completed for session {SessionId}. Match score: {Score}%",
            sessionId, result.OverallMatchScore);
        
        return result;
    }

    public async Task<TailoringResultModel> ExecuteResumeTailoringAsync(
        ResumeAnalysisModel resume, 
        JDAnalysisModel jd, 
        SkillMatchResultModel match, 
        string targetRole, 
        string sessionId, 
        CancellationToken ct = default)
    {
        _logger.LogInformation("Resume Tailoring skill started for session {SessionId}", sessionId);
        
        var result = await _aiService.TailorResumeAsync(resume, jd, match, targetRole, ct);
        
        if (result == null)
        {
            _logger.LogWarning("Resume Tailoring returned null for session {SessionId}", sessionId);
            result = new TailoringResultModel();
        }

        _logger.LogInformation("Resume Tailoring completed for session {SessionId}. Changes: {ChangeCount}",
            sessionId, result.Changes?.Count ?? 0);
        
        return result;
    }

    public async Task<ATSAnalysisModel> ExecuteATSValidationAsync(
        TailoringResultModel tailoredResume, 
        JDAnalysisModel jd, 
        string originalResumeText, 
        string sessionId, 
        CancellationToken ct = default)
    {
        _logger.LogInformation("ATS Validation skill started for session {SessionId}", sessionId);
        
        var result = await _atsValidationService.ValidateAsync(tailoredResume, jd, originalResumeText, ct);
        
        _logger.LogInformation("ATS Validation completed for session {SessionId}. ATS Score: {Score}",
            sessionId, result.AtsScore);
        
        return result;
    }

    public async Task<GuardrailResultModel> ExecuteGuardrailsAsync(
        TailoringResultModel tailoredResume, 
        string originalResumeText, 
        ResumeAnalysisModel resume, 
        SkillMatchResultModel match, 
        string sessionId, 
        CancellationToken ct = default)
    {
        _logger.LogInformation("Guardrail validation started for session {SessionId}", sessionId);
        
        var result = await _aiService.ValidateGuardrailsAsync(tailoredResume, originalResumeText, resume, match, ct);
        
        if (result == null)
        {
            _logger.LogWarning("Guardrail validation returned null for session {SessionId}", sessionId);
            result = new GuardrailResultModel { Status = "FAIL", Summary = "Validation failed - unable to validate" };
        }

        if (result.Status == "FAIL")
        {
            _logger.LogWarning("Guardrail validation FAILED for session {SessionId}. Unsupported claims: {ClaimCount}",
                sessionId, result.UnsupportedClaims?.Count ?? 0);
        }
        else
        {
            _logger.LogInformation("Guardrail validation PASSED for session {SessionId}", sessionId);
        }
        
        return result;
    }
}
