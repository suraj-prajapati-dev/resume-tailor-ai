using System.Text.Json.Serialization;

namespace ResumeTailorAI.Models;

public class ResumeModel
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("fileType")]
    public string FileType { get; set; } = string.Empty;

    [JsonPropertyName("extractedText")]
    public string ExtractedText { get; set; } = string.Empty;

    [JsonPropertyName("pageCount")]
    public int PageCount { get; set; }

    [JsonPropertyName("characterCount")]
    public int CharacterCount { get; set; }
}

public class JobDescriptionModel
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("fileType")]
    public string FileType { get; set; } = string.Empty;

    [JsonPropertyName("extractedText")]
    public string ExtractedText { get; set; } = string.Empty;

    [JsonPropertyName("targetRole")]
    public string TargetRole { get; set; } = string.Empty;
}

public class TargetRoleModel
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}

public class SkillMatchModel
{
    [JsonPropertyName("skill")]
    public string Skill { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("proficiency")]
    public string Proficiency { get; set; } = string.Empty;

    [JsonPropertyName("yearsOfExperience")]
    public double? YearsOfExperience { get; set; }

    [JsonPropertyName("evidence")]
    public string Evidence { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

public class ResumeAnalysisModel
{
    [JsonPropertyName("candidate")]
    public CandidateInfo Candidate { get; set; } = new();

    [JsonPropertyName("professionalSummary")]
    public string ProfessionalSummary { get; set; } = string.Empty;

    [JsonPropertyName("totalExperienceYears")]
    public double TotalExperienceYears { get; set; }

    [JsonPropertyName("currentRole")]
    public string CurrentRole { get; set; } = string.Empty;

    [JsonPropertyName("currentEmployer")]
    public string CurrentEmployer { get; set; } = string.Empty;

    [JsonPropertyName("employmentHistory")]
    public List<EmploymentEntry> EmploymentHistory { get; set; } = new();

    [JsonPropertyName("skills")]
    public List<SkillInfo> Skills { get; set; } = new();

    [JsonPropertyName("projects")]
    public List<ProjectInfo> Projects { get; set; } = new();

    [JsonPropertyName("education")]
    public List<EducationInfo> Education { get; set; } = new();

    [JsonPropertyName("certifications")]
    public List<CertificationInfo> Certifications { get; set; } = new();

    [JsonPropertyName("keywords")]
    public List<string> Keywords { get; set; } = new();
}

public class CandidateInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("linkedin")]
    public string LinkedIn { get; set; } = string.Empty;

    [JsonPropertyName("github")]
    public string GitHub { get; set; } = string.Empty;

    [JsonPropertyName("portfolio")]
    public string Portfolio { get; set; } = string.Empty;
}

public class EmploymentEntry
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

    [JsonPropertyName("responsibilities")]
    public List<string> Responsibilities { get; set; } = new();

    [JsonPropertyName("achievements")]
    public List<string> Achievements { get; set; } = new();

    [JsonPropertyName("technologies")]
    public List<string> Technologies { get; set; } = new();
}

public class SkillInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("proficiency")]
    public string Proficiency { get; set; } = string.Empty;

    [JsonPropertyName("yearsOfExperience")]
    public double? YearsOfExperience { get; set; }

    [JsonPropertyName("evidence")]
    public string Evidence { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

public class ProjectInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("technologies")]
    public List<string> Technologies { get; set; } = new();

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("achievements")]
    public List<string> Achievements { get; set; } = new();
}

public class EducationInfo
{
    [JsonPropertyName("degree")]
    public string Degree { get; set; } = string.Empty;

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("institution")]
    public string Institution { get; set; } = string.Empty;

    [JsonPropertyName("graduationYear")]
    public string GraduationYear { get; set; } = string.Empty;

    [JsonPropertyName("honors")]
    public string Honors { get; set; } = string.Empty;
}

public class CertificationInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = string.Empty;

    [JsonPropertyName("issueDate")]
    public string IssueDate { get; set; } = string.Empty;

    [JsonPropertyName("expiryDate")]
    public string? ExpiryDate { get; set; }

    [JsonPropertyName("credentialId")]
    public string CredentialId { get; set; } = string.Empty;
}