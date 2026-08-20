using ResumeTailorAI.Models;

namespace ResumeTailorAI.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

public class StartSessionRequest
{
    public string TargetRole { get; set; } = string.Empty;
}

public class StartSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
}

public class UploadFileRequest
{
    public IFormFile File { get; set; } = null!;
}

public class UploadResponse
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class AnalysisStatusResponse
{
    public bool IsComplete { get; set; }
    public List<string> CompletedSteps { get; set; } = new();
    public string CurrentStep { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
}

public class AnalysisResultResponse
{
    public string TargetRole { get; set; } = string.Empty;
    public double OverallMatchScore { get; set; }
    public string ResumeSummary { get; set; } = string.Empty;
    public string JdSummary { get; set; } = string.Empty;
    public List<string> MatchedSkills { get; set; } = new();
    public List<PartialMatchDto> PartialMatches { get; set; } = new();
    public List<MissingSkillDto> MissingSkills { get; set; } = new();
    public ExperienceMatchDto ExperienceMatch { get; set; } = new();
    public ATSAnalysisModel AtsAnalysis { get; set; } = new();
    public List<TailoringRecommendationDto> TailoringRecommendations { get; set; } = new();
    public TailoredResume TailoredResume { get; set; } = new();
    public GuardrailResultModel Guardrail { get; set; } = new();
    public bool RequiresHumanApproval { get; set; } = true;
}

public class PartialMatchDto
{
    public string Skill { get; set; } = string.Empty;
    public string Gap { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class MissingSkillDto
{
    public string Skill { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class ExperienceMatchDto
{
    public int RequiredYears { get; set; }
    public double CandidateYears { get; set; }
    public string Match { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

public class TailoringRecommendationDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Section { get; set; } = string.Empty;
}

public class GeneratePreviewRequest
{
    public TailoringResultModel TailoringResult { get; set; } = null!;
    public GuardrailResultModel GuardrailResult { get; set; } = null!;
}

public class ApproveRequest
{
    public bool Approved { get; set; }
}

public class GenerateDocumentsResponse
{
    public string ResumeDownloadUrl { get; set; } = string.Empty;
    public string CoverLetterDownloadUrl { get; set; } = string.Empty;
}
