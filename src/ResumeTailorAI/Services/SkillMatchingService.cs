using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResumeTailorAI.Configuration;
using ResumeTailorAI.Models;

namespace ResumeTailorAI.Services;

public interface ISkillMatchingService
{
    Task<SkillMatchResultModel> MatchAsync(ResumeAnalysisModel resume, JDAnalysisModel jd, CancellationToken ct = default);
}

public class SkillMatchingService : ISkillMatchingService
{
    private readonly IAIService _aiService;
    private readonly ILogger<SkillMatchingService> _logger;
    private readonly ScoringConfiguration _scoringConfig;

    public SkillMatchingService(
        IAIService aiService,
        IOptions<AppConfiguration> config,
        ILogger<SkillMatchingService> logger)
    {
        _aiService = aiService;
        _scoringConfig = config.Value.Scoring;
        _logger = logger;
    }

    public async Task<SkillMatchResultModel> MatchAsync(ResumeAnalysisModel resume, JDAnalysisModel jd, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting skill matching");
        
        var result = await _aiService.MatchSkillsAsync(resume, jd, ct);
        
        if (result == null)
        {
            _logger.LogWarning("Skill matching returned null, returning empty result");
            result = new SkillMatchResultModel();
        }

        CalculateCategoryScores(result, resume, jd);
        CalculateOverallScore(result);
        BuildSkillLists(result);
        CalculateExperienceMatch(result, resume, jd);

        _logger.LogInformation("Skill matching completed. Overall score: {Score}", result.OverallMatchScore);
        return result;
    }

    private void CalculateCategoryScores(SkillMatchResultModel result, ResumeAnalysisModel resume, JDAnalysisModel jd)
    {
        var scores = new CategoryScores();
        
        var skillMatches = result.SkillMatches;
        var totalSkills = skillMatches.Count;
        if (totalSkills == 0)
        {
            return;
        }

        var techScores = skillMatches.Where(s => s.JdCategory == "Technical" || s.JdCategory == "Framework" || s.JdCategory == "Language" || s.JdCategory == "Database" || s.JdCategory == "Cloud" || s.JdCategory == "Tool" || s.JdCategory == "Architecture");
        scores.TechnicalSkills = CalculatePercentage(techScores);

        var expScores = skillMatches.Where(s => s.JdCategory == "Experience");
        scores.Experience = CalculatePercentage(expScores);

        var archScores = skillMatches.Where(s => s.JdCategory == "Architecture");
        scores.Architecture = CalculatePercentage(archScores);

        var leadScores = skillMatches.Where(s => s.JdCategory == "Leadership" || s.JdCategory == "Soft");
        scores.Leadership = CalculatePercentage(leadScores);

        var domainScores = skillMatches.Where(s => s.JdCategory == "Domain");
        scores.Domain = CalculatePercentage(domainScores);

        scores.AtsKeywords = CalculateKeywordCoverage(jd, resume);

        result.CategoryScores = scores;
    }

    private double CalculatePercentage(IEnumerable<SkillMatchDetail> matches)
    {
        var list = matches.ToList();
        if (!list.Any()) return 0;

        var totalWeight = list.Sum(m => GetPriorityWeight(m.JdPriority));
        if (totalWeight == 0) return 0;

        var matchedWeight = list
            .Where(m => m.Match == "Matched")
            .Sum(m => GetPriorityWeight(m.JdPriority)) * 1.0;

        var partialWeight = list
            .Where(m => m.Match == "Partially Matched")
            .Sum(m => GetPriorityWeight(m.JdPriority)) * 0.5;

        return (matchedWeight + partialWeight) / totalWeight * 100;
    }

    private double GetPriorityWeight(string priority)
    {
        if (string.IsNullOrEmpty(priority)) return 0;
        
        return priority.ToLowerInvariant() switch
        {
            "must have" => 1.0,
            "should have" => 0.5,
            "nice to have" => 0.2,
            "unknown" => 0.0,
            _ => 0.5
        };
    }

    private double CalculateKeywordCoverage(JDAnalysisModel jd, ResumeAnalysisModel resume)
    {
        if (jd.Keywords.Count == 0) return 100;

        var resumeText = string.Join(" ", resume.Keywords);
        var matchedKeywords = jd.Keywords.Count(kw => 
            resume.Keywords.Any(rk => rk.Contains(kw, StringComparison.OrdinalIgnoreCase) || 
                                     kw.Contains(rk, StringComparison.OrdinalIgnoreCase)));
        
        return (double)matchedKeywords / jd.Keywords.Count * 100;
    }

    private void CalculateOverallScore(SkillMatchResultModel result)
    {
        result.OverallMatchScore = (
            result.CategoryScores.TechnicalSkills * _scoringConfig.TechnicalWeight +
            result.CategoryScores.Experience * _scoringConfig.ExperienceWeight +
            result.CategoryScores.AtsKeywords * _scoringConfig.ATSWeight +
            result.CategoryScores.Leadership * _scoringConfig.LeadershipWeight +
            result.CategoryScores.Domain * _scoringConfig.DomainWeight +
            result.CategoryScores.Architecture * _scoringConfig.ArchitectureWeight
        ) / 100;
    }

    private void BuildSkillLists(SkillMatchResultModel result)
    {
        result.MatchedSkills = result.SkillMatches
            .Where(s => s.Match == "Matched")
            .Select(s => s.JdSkill)
            .Distinct()
            .ToList();

        result.PartialMatches = result.SkillMatches
            .Where(s => s.Match == "Partially Matched")
            .Select(s => new PartialMatch
            {
                Skill = s.JdSkill,
                Gap = "Partial match - candidate has related experience",
                Recommendation = "Highlight transferable skills and adjacent experience"
            })
            .ToList();

        result.MissingSkills = result.SkillMatches
            .Where(s => s.Match == "Missing")
            .Select(s => new MissingSkill
            {
                Skill = s.JdSkill,
                Priority = s.JdPriority,
                Action = "Do not add to resume"
            })
            .ToList();
    }

    private void CalculateExperienceMatch(SkillMatchResultModel result, ResumeAnalysisModel resume, JDAnalysisModel jd)
    {
        result.ExperienceMatch.RequiredYears = jd.ExperienceRequired.MinYears;
        result.ExperienceMatch.CandidateYears = resume.TotalExperienceYears;
        
        if (resume.TotalExperienceYears >= jd.ExperienceRequired.MinYears)
        {
            result.ExperienceMatch.Match = "Meets";
            result.ExperienceMatch.Details = $"Candidate has {resume.TotalExperienceYears} years of experience, meeting the minimum of {jd.ExperienceRequired.MinYears}";
        }
        else
        {
            result.ExperienceMatch.Match = "Below";
            result.ExperienceMatch.Details = $"Candidate has {resume.TotalExperienceYears} years, below the required {jd.ExperienceRequired.MinYears} years";
        }
    }
}
