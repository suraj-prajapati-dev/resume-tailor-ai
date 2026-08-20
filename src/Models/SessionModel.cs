namespace ResumeTailorAI.Models;

public class ResumeTailorSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    public string? ResumeFilePath { get; set; }
    public string? JDFilePath { get; set; }
    public string ResumeText { get; set; } = string.Empty;
    public string JDText { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public JDAnalysisModel? JDAnalysis { get; set; }
    public ResumeAnalysisModel? AnalysisResult { get; set; }
    public SkillMatchResultModel? SkillMatchResult { get; set; }
    public TailoringResultModel? TailoringResult { get; set; }
    public ATSAnalysisModel? AtsResult { get; set; }
    public GuardrailResultModel? GuardrailResult { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public string? GeneratedResumePath { get; set; }
    public string? GeneratedCoverLetterPath { get; set; }
    public bool IsLocked { get; set; } = false;
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public class AnalysisProgress
{
    public bool ResumeParsed { get; set; } = false;
    public bool JdParsed { get; set; } = false;
    public bool SkillsExtracted { get; set; } = false;
    public bool SkillsMatched { get; set; } = false;
    public bool TailoringCompleted { get; set; } = false;
    public bool AtsCompleted { get; set; } = false;
    public bool GuardrailCompleted { get; set; } = false;
    public bool AnalysisComplete => GuardrailCompleted;
}
