using System.Text.Json.Serialization;

namespace ResumeTailorAI.Models;

public class JDAnalysisModel
{
    [JsonPropertyName("targetRole")]
    public string TargetRole { get; set; } = string.Empty;

    [JsonPropertyName("company")]
    public string Company { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("employmentType")]
    public string EmploymentType { get; set; } = string.Empty;

    [JsonPropertyName("experienceRequired")]
    public ExperienceRequirement ExperienceRequired { get; set; } = new();

    [JsonPropertyName("requiredSkills")]
    public List<JDSkillRequirement> RequiredSkills { get; set; } = new();

    [JsonPropertyName("responsibilities")]
    public List<string> Responsibilities { get; set; } = new();

    [JsonPropertyName("domainRequirements")]
    public List<string> DomainRequirements { get; set; } = new();

    [JsonPropertyName("educationRequirements")]
    public List<EducationRequirement> EducationRequirements { get; set; } = new();

    [JsonPropertyName("certificationRequirements")]
    public List<CertificationRequirement> CertificationRequirements { get; set; } = new();

    [JsonPropertyName("cloudRequirements")]
    public List<string> CloudRequirements { get; set; } = new();

    [JsonPropertyName("architectureRequirements")]
    public List<string> ArchitectureRequirements { get; set; } = new();

    [JsonPropertyName("leadershipRequirements")]
    public List<string> LeadershipRequirements { get; set; } = new();

    [JsonPropertyName("keywords")]
    public List<string> Keywords { get; set; } = new();

    [JsonPropertyName("atsPhrases")]
    public List<string> AtsPhrases { get; set; } = new();
}

public class ExperienceRequirement
{
    [JsonPropertyName("minYears")]
    public int MinYears { get; set; }

    [JsonPropertyName("maxYears")]
    public int? MaxYears { get; set; }

    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;
}

public class JDSkillRequirement
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("isExplicit")]
    public bool IsExplicit { get; set; }

    [JsonPropertyName("isAmbiguous")]
    public bool IsAmbiguous { get; set; }

    [JsonPropertyName("context")]
    public string Context { get; set; } = string.Empty;

    [JsonPropertyName("atsPhrases")]
    public List<string> AtsPhrases { get; set; } = new();
}

public class EducationRequirement
{
    [JsonPropertyName("degree")]
    public string Degree { get; set; } = string.Empty;

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public bool Required { get; set; }
}

public class CertificationRequirement
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public bool Required { get; set; }
}