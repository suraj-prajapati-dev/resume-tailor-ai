using Microsoft.Extensions.Logging;
using ResumeTailorAI.Models;

namespace ResumeTailorAI.Services;

public interface IAtsValidationService
{
    Task<ATSAnalysisModel> ValidateAsync(
        TailoringResultModel tailoredResume,
        JDAnalysisModel jd,
        string originalResumeText,
        CancellationToken ct = default);
}

public class AtsValidationService : IAtsValidationService
{
    private readonly IAIService _aiService;
    private readonly ILogger<AtsValidationService> _logger;

    public AtsValidationService(IAIService aiService, ILogger<AtsValidationService> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<ATSAnalysisModel> ValidateAsync(
        TailoringResultModel tailoredResume,
        JDAnalysisModel jd,
        string originalResumeText,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Starting ATS validation");

        var result = await _aiService.ValidateATSAsync(tailoredResume, jd, originalResumeText, ct);
        
        if (result == null)
        {
            _logger.LogWarning("ATS validation returned null, returning default result");
            result = new ATSAnalysisModel
            {
                AtsScore = 0,
                KeywordCoverage = 0,
                IsAtsFriendly = false,
                Recommendations = new List<string> { "ATS validation failed - unable to validate" }
            };
        }

        _logger.LogInformation("ATS validation completed. Score: {Score}", result.AtsScore);
        return result;
    }
}