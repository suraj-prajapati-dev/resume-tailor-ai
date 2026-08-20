using System.Text.Json.Serialization;

namespace ResumeTailorAI.Models;

public class TailoringResultModel
{
    [JsonPropertyName("tailoredResume")]
    public TailoredResume TailoredResume { get; set; } = new();

    [JsonPropertyName("changes")]
    public List<ChangeRecord> Changes { get; set; } = new();

    [JsonPropertyName("keywordsIntegrated")]
    public List<string> KeywordsIntegrated { get; set; } = new();
}

public class TailoredResume
{
    [JsonPropertyName("professionalSummary")]
    public string ProfessionalSummary { get; set; } = string.Empty;

    [JsonPropertyName("coreCompetencies")]
    public List<string> CoreCompetencies { get; set; } = new();

    [JsonPropertyName("technicalSkills")]
    public List<TechnicalSkillCategory> TechnicalSkills { get; set; } = new();

    [JsonPropertyName("experience")]
    public List<TailoredExperience> Experience { get; set; } = new();

    [JsonPropertyName("projects")]
    public List<TailoredProject> Projects { get; set; } = new();

    [JsonPropertyName("education")]
    public List<EducationInfo> Education { get; set; } = new();

    [JsonPropertyName("certifications")]
    public List<CertificationInfo> Certifications { get; set; } = new();
}

public class TechnicalSkillCategory
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("skills")]
    public List<string> Skills { get; set; } = new();

    [JsonPropertyName("priority")]
    public int Priority { get; set; }
}

public class TailoredExperience
{
    [JsonPropertyName("employer")]
    public string Employer { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;

    [JsonPropertyName("isCurrent")]
    public bool IsCurrent { get; set; }

    [JsonPropertyName("bullets")]
    public List<TailoredBullet> Bullets { get; set; } = new();
}

public class TailoredBullet
{
    [JsonPropertyName("original")]
    public string Original { get; set; } = string.Empty;

    [JsonPropertyName("tailored")]
    public string Tailored { get; set; } = string.Empty;

    [JsonPropertyName("evidence")]
    public string Evidence { get; set; } = string.Empty;

    [JsonPropertyName("keywordsAdded")]
    public List<string> KeywordsAdded { get; set; } = new();
}

public class TailoredProject
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("technologies")]
    public List<string> Technologies { get; set; } = new();

    [JsonPropertyName("highlights")]
    public List<string> Highlights { get; set; } = new();
}

public class ChangeRecord
{
    [JsonPropertyName("section")]
    public string Section { get; set; } = string.Empty;

    [JsonPropertyName("changeType")]
    public string ChangeType { get; set; } = string.Empty;

    [JsonPropertyName("original")]
    public string Original { get; set; } = string.Empty;

    [JsonPropertyName("modified")]
    public string Modified { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("evidence")]
    public string Evidence { get; set; } = string.Empty;
}

public class ResumeAnalysisOutputModel
{
    [JsonPropertyName("targetRole")]
    public string TargetRole { get; set; } = string.Empty;

    [JsonPropertyName("overallMatchScore")]
    public double OverallMatchScore { get; set; }

    [JsonPropertyName("resumeSummary")]
    public string ResumeSummary { get; set; } = string.Empty;

    [JsonPropertyName("jdSummary")]
    public string JdSummary { get; set; } = string.Empty;

    [JsonPropertyName("matchedSkills")]
    public List<string> MatchedSkills { get; set; } = new();

    [JsonPropertyName("partialMatches")]
    public List<PartialMatch> PartialMatches { get; set; } = new();

    [JsonPropertyName("missingSkills")]
    public List<MissingSkill> MissingSkills { get; set; } = new();

    [JsonPropertyName("experienceMatch")]
    public ExperienceMatch ExperienceMatch { get; set; } = new();

    [JsonPropertyName("atsAnalysis")]
    public ATSAnalysisModel AtsAnalysis { get; set; } = new();

    [JsonPropertyName("tailoringRecommendations")]
    public List<TailoringRecommendation> TailoringRecommendations { get; set; } = new();

    [JsonPropertyName("tailoredResume")]
    public TailoredResume TailoredResume { get; set; } = new();

    [JsonPropertyName("guardrail")]
    public GuardrailResultModel Guardrail { get; set; } = new();

    [JsonPropertyName("requiresHumanApproval")]
    public bool RequiresHumanApproval { get; set; } = true;
}

public class TailoringRecommendation
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("section")]
    public string Section { get; set; } = string.Empty;
}

public class CoverLetterInput
{
    [JsonPropertyName("candidateName")]
    public string CandidateName { get; set; } = string.Empty;

    [JsonPropertyName("candidateEmail")]
    public string CandidateEmail { get; set; } = string.Empty;

    [JsonPropertyName("candidatePhone")]
    public string CandidatePhone { get; set; } = string.Empty;

    [JsonPropertyName("candidateLocation")]
    public string CandidateLocation { get; set; } = string.Empty;

    [JsonPropertyName("targetRole")]
    public string TargetRole { get; set; } = string.Empty;

    [JsonPropertyName("company")]
    public string Company { get; set; } = string.Empty;

    [JsonPropertyName("relevantExperience")]
    public string RelevantExperience { get; set; } = string.Empty;

    [JsonPropertyName("technicalStrengths")]
    public string TechnicalStrengths { get; set; } = string.Empty;

    [JsonPropertyName("architectureStrengths")]
    public string ArchitectureStrengths { get; set; } = string.Empty;

    [JsonPropertyName("whyCandidateFits")]
    public string WhyCandidateFits { get; set; } = string.Empty;
}
