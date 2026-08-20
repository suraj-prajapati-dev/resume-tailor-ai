using System.Text.Json.Serialization;

namespace ResumeTailorAI.Models;

public class ATSAnalysisModel
{
    [JsonPropertyName("atsScore")]
    public int AtsScore { get; set; }

    [JsonPropertyName("keywordCoverage")]
    public double KeywordCoverage { get; set; }

    [JsonPropertyName("criticalMissingKeywords")]
    public List<string> CriticalMissingKeywords { get; set; } = new();

    [JsonPropertyName("potentialKeywordStuffing")]
    public List<KeywordStuffingAlert> PotentialKeywordStuffing { get; set; } = new();

    [JsonPropertyName("formattingRisks")]
    public List<FormattingRisk> FormattingRisks { get; set; } = new();

    [JsonPropertyName("sectionStructure")]
    public SectionStructure SectionStructure { get; set; } = new();

    [JsonPropertyName("jobTitleAlignment")]
    public JobTitleAlignment JobTitleAlignment { get; set; } = new();

    [JsonPropertyName("skillsAlignment")]
    public SkillsAlignment SkillsAlignment { get; set; } = new();

    [JsonPropertyName("recommendations")]
    public List<string> Recommendations { get; set; } = new();

    [JsonPropertyName("isAtsFriendly")]
    public bool IsAtsFriendly { get; set; }
}

public class KeywordStuffingAlert
{
    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("threshold")]
    public int Threshold { get; set; }

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;
}

public class FormattingRisk
{
    [JsonPropertyName("issue")]
    public string Issue { get; set; } = string.Empty;

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("recommendation")]
    public string Recommendation { get; set; } = string.Empty;
}

public class SectionStructure
{
    [JsonPropertyName("hasContactInfo")]
    public bool HasContactInfo { get; set; }

    [JsonPropertyName("hasProfessionalSummary")]
    public bool HasProfessionalSummary { get; set; }

    [JsonPropertyName("hasExperience")]
    public bool HasExperience { get; set; }

    [JsonPropertyName("hasSkills")]
    public bool HasSkills { get; set; }

    [JsonPropertyName("hasEducation")]
    public bool HasEducation { get; set; }

    [JsonPropertyName("hasCertifications")]
    public bool HasCertifications { get; set; }

    [JsonPropertyName("issues")]
    public List<string> Issues { get; set; } = new();
}

public class JobTitleAlignment
{
    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("targetTitle")]
    public string TargetTitle { get; set; } = string.Empty;

    [JsonPropertyName("candidateTitles")]
    public List<string> CandidateTitles { get; set; } = new();

    [JsonPropertyName("recommendation")]
    public string Recommendation { get; set; } = string.Empty;
}

public class SkillsAlignment
{
    [JsonPropertyName("matched")]
    public int Matched { get; set; }

    [JsonPropertyName("missing")]
    public int Missing { get; set; }

    [JsonPropertyName("coverage")]
    public double Coverage { get; set; }
}