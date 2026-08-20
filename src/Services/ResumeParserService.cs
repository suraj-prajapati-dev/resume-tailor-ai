using Microsoft.Extensions.Logging;
using ResumeTailorAI.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ResumeTailorAI.Services;

public interface IResumeParserService
{
    Task<ResumeModel> ParseAsync(DocumentText documentText, CancellationToken ct = default);
    Task<string> ExtractSkillsAsync(DocumentText documentText, CancellationToken ct = default);
    Task<ResumeAnalysisModel> AnalyzeAsync(DocumentText documentText, CancellationToken ct = default);
}

public class ResumeParserService : IResumeParserService
{
    private readonly ILogger<ResumeParserService> _logger;
    private readonly IAIService _aiService;

    public ResumeParserService(ILogger<ResumeParserService> logger, IAIService aiService)
    {
        _logger = logger;
        _aiService = aiService;
    }

    public async Task<ResumeModel> ParseAsync(DocumentText documentText, CancellationToken ct = default)
    {
        _logger.LogInformation("Parsing resume: {FileName}", documentText.FileName);
        return new ResumeModel
        {
            FileName = documentText.FileName,
            FileType = documentText.FileType,
            ExtractedText = documentText.ExtractedText,
            PageCount = documentText.PageCount,
            CharacterCount = documentText.CharacterCount
        };
    }

    public async Task<string> ExtractSkillsAsync(DocumentText documentText, CancellationToken ct = default)
    {
        return await _aiService.ExtractSkillsAsync(documentText.ExtractedText, ct);
    }

    public async Task<ResumeAnalysisModel> AnalyzeAsync(DocumentText documentText, CancellationToken ct = default)
    {
        _logger.LogInformation("Analyzing resume: {FileName}", documentText.FileName);
        
        if (string.IsNullOrWhiteSpace(documentText.ExtractedText))
        {
            _logger.LogWarning("Resume text is empty for: {FileName}", documentText.FileName);
            return new ResumeAnalysisModel();
        }

        var skill = await _aiService.AnalyzeResumeAsync(documentText.ExtractedText, ct);

        if (skill is null)
        {
            _logger.LogWarning("AI returned null for resume analysis");
            return new ResumeAnalysisModel();
        }

        _logger.LogInformation("Resume analysis completed for: {FileName}", documentText.FileName);
        return skill;
    }
}

public interface IJobDescriptionParserService
{
    Task<JobDescriptionModel> ParseAsync(DocumentText documentText, string targetRole, CancellationToken ct = default);
    Task<JDAnalysisModel> AnalyzeAsync(DocumentText documentText, string targetRole, CancellationToken ct = default);
}

public class JobDescriptionParserService : IJobDescriptionParserService
{
    private readonly ILogger<JobDescriptionParserService> _logger;
    private readonly IAIService _aiService;

    public JobDescriptionParserService(ILogger<JobDescriptionParserService> logger, IAIService aiService)
    {
        _logger = logger;
        _aiService = aiService;
    }

    public async Task<JobDescriptionModel> ParseAsync(DocumentText documentText, string targetRole, CancellationToken ct = default)
    {
        _logger.LogInformation("Parsing job description: {FileName}", documentText.FileName);
        return new JobDescriptionModel
        {
            FileName = documentText.FileName,
            FileType = documentText.FileType,
            ExtractedText = documentText.ExtractedText,
            TargetRole = targetRole
        };
    }

    public async Task<JDAnalysisModel> AnalyzeAsync(DocumentText documentText, string targetRole, CancellationToken ct = default)
    {
        _logger.LogInformation("Analyzing job description: {FileName}", documentText.FileName);
        
        if (string.IsNullOrWhiteSpace(documentText.ExtractedText))
        {
            _logger.LogWarning("JD text is empty for: {FileName}", documentText.FileName);
            return new JDAnalysisModel { TargetRole = targetRole };
        }

        var skill = await _aiService.AnalyzeJobDescriptionAsync(documentText.ExtractedText, targetRole, ct);

        if (skill is null)
        {
            _logger.LogWarning("AI returned null for JD analysis");
            return new JDAnalysisModel { TargetRole = targetRole };
        }

        _logger.LogInformation("JD analysis completed for: {FileName}", documentText.FileName);
        return skill;
    }
}
