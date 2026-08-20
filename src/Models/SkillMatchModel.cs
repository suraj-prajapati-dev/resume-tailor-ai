using System.Text.Json.Serialization;

namespace ResumeTailorAI.Models;

public class SkillMatchResultModel
{
    [JsonPropertyName("overallMatchScore")]
    public double OverallMatchScore { get; set; }

    [JsonPropertyName("categoryScores")]
    public CategoryScores CategoryScores { get; set; } = new();

    [JsonPropertyName("skillMatches")]
    public List<SkillMatchDetail> SkillMatches { get; set; } = new();

    [JsonPropertyName("matchedSkills")]
    public List<string> MatchedSkills { get; set; } = new();

    [JsonPropertyName("partialMatches")]
    public List<PartialMatch> PartialMatches { get; set; } = new();

    [JsonPropertyName("missingSkills")]
    public List<MissingSkill> MissingSkills { get; set; } = new();

    [JsonPropertyName("experienceMatch")]
    public ExperienceMatch ExperienceMatch { get; set; } = new();

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}

public class CategoryScores
{
    [JsonPropertyName("technicalSkills")]
    public double TechnicalSkills { get; set; }

    [JsonPropertyName("experience")]
    public double Experience { get; set; }

    [JsonPropertyName("architecture")]
    public double Architecture { get; set; }

    [JsonPropertyName("leadership")]
    public double Leadership { get; set; }

    [JsonPropertyName("domain")]
    public double Domain { get; set; }

    [JsonPropertyName("atsKeywords")]
    public double AtsKeywords { get; set; }
}

public class SkillMatchDetail
{
    [JsonPropertyName("jdSkill")]
    public string JdSkill { get; set; } = string.Empty;

    [JsonPropertyName("jdPriority")]
    public string JdPriority { get; set; } = string.Empty;

    [JsonPropertyName("jdCategory")]
    public string JdCategory { get; set; } = string.Empty;

    [JsonPropertyName("resumeSkill")]
    public string? ResumeSkill { get; set; }

    [JsonPropertyName("resumeEvidence")]
    public string? ResumeEvidence { get; set; }

    [JsonPropertyName("match")]
    public string Match { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;
}

public class PartialMatch
{
    [JsonPropertyName("skill")]
    public string Skill { get; set; } = string.Empty;

    [JsonPropertyName("gap")]
    public string Gap { get; set; } = string.Empty;

    [JsonPropertyName("recommendation")]
    public string Recommendation { get; set; } = string.Empty;
}

public class MissingSkill
{
    [JsonPropertyName("skill")]
    public string Skill { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;
}

public class ExperienceMatch
{
    [JsonPropertyName("requiredYears")]
    public int RequiredYears { get; set; }

    [JsonPropertyName("candidateYears")]
    public double CandidateYears { get; set; }

    [JsonPropertyName("match")]
    public string Match { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public string Details { get; set; } = string.Empty;
}