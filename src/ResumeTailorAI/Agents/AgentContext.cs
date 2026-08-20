using Microsoft.Extensions.Logging;
using ResumeTailorAI.Models;
using ResumeTailorAI.Services;

namespace ResumeTailorAI.Agents;

public class AgentContext
{
    public string SessionId { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public ResumeTailorSession? Session { get; set; }
    public DocumentText? ResumeDocument { get; set; }
    public DocumentText? JDDocument { get; set; }
    public ResumeAnalysisModel? ResumeAnalysis { get; set; }
    public JDAnalysisModel? JDAnalysis { get; set; }
    public SkillMatchResultModel? SkillMatchResult { get; set; }
    public TailoringResultModel? TailoringResult { get; set; }
    public ATSAnalysisModel? AtsResult { get; set; }
    public GuardrailResultModel? GuardrailResult { get; set; }
    public AnalysisProgress Progress { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
}
