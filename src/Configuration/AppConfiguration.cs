namespace ResumeTailorAI.Configuration;

public class AIConfiguration
{
    public string Provider { get; set; } = "OpenAI";
    public string Model { get; set; } = "gpt-4o-mini";
    public string Endpoint { get; set; } = "https://api.openai.com/v1";
    public string ApiKey { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 4000;
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 3;
}

public class FileConfiguration
{
    public int MaxResumeSizeMB { get; set; } = 10;
    public int MaxJDSizeMB { get; set; } = 10;
    public List<string> AllowedResumeExtensions { get; set; } = new() { ".pdf", ".docx", ".md", ".txt" };
    public List<string> AllowedJDExtensions { get; set; } = new() { ".pdf", ".docx", ".md", ".txt" };
    public Dictionary<string, List<string>> AllowedMimeTypes { get; set; } = new()
    {
        [".pdf"] = new() { "application/pdf" },
        [".docx"] = new() { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        [".md"] = new() { "text/markdown" },
        [".txt"] = new() { "text/plain" }
    };
}

public class SessionConfiguration
{
    public int TimeoutMinutes { get; set; } = 30;
    public int CleanupIntervalMinutes { get; set; } = 10;
    public string TempPath { get; set; } = string.Empty;
}

public class AppConfiguration
{
    public AIConfiguration AI { get; set; } = new();
    public FileConfiguration Files { get; set; } = new();
    public SessionConfiguration Sessions { get; set; } = new();
    public ScoringConfiguration Scoring { get; set; } = new();
}

public class ScoringConfiguration
{
    public double TechnicalWeight { get; set; } = 0.4;
    public double ExperienceWeight { get; set; } = 0.3;
    public double ATSWeight { get; set; } = 0.2;
    public double LeadershipWeight { get; set; } = 0.1;
    public double DomainWeight { get; set; } = 0.2;
    public double ArchitectureWeight { get; set; } = 0.1;
    public Dictionary<string, double> SkillPriorityWeights { get; set; } = new()
    {
        ["Must Have"] = 1.0,
        ["Should Have"] = 0.5,
        ["Nice To Have"] = 0.2,
        ["Unknown"] = 0.0
    };
}