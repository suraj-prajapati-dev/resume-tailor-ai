using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ResumeTailorAI.Configuration;
using ResumeTailorAI.Models;
using System.Text.Json;

namespace ResumeTailorAI.Services;

public interface IAIService
{
    Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default);
    Task<ChatMessageContent> GetChatCompletionAsync(ChatHistory chatHistory, CancellationToken ct = default);
    Task<ResumeAnalysisModel?> AnalyzeResumeAsync(string resumeText, CancellationToken ct = default);
    Task<JDAnalysisModel?> AnalyzeJobDescriptionAsync(string jdText, string targetRole, CancellationToken ct = default);
    Task<SkillMatchResultModel?> MatchSkillsAsync(ResumeAnalysisModel resume, JDAnalysisModel jd, CancellationToken ct = default);
    Task<TailoringResultModel?> TailorResumeAsync(ResumeAnalysisModel resume, JDAnalysisModel jd, SkillMatchResultModel match, string targetRole, CancellationToken ct = default);
    Task<ATSAnalysisModel?> ValidateATSAsync(TailoringResultModel tailoredResume, JDAnalysisModel jd, string resumeText, CancellationToken ct = default);
    Task<GuardrailResultModel?> ValidateGuardrailsAsync(TailoringResultModel tailoredResume, string originalResumeText, ResumeAnalysisModel resume, SkillMatchResultModel match, CancellationToken ct = default);
    Task<string> ExtractSkillsAsync(string resumeText, CancellationToken ct = default);
}

public class AIService : IAIService
{
    private readonly ILogger<AIService> _logger;
    private readonly AIConfiguration _config;
    private readonly Kernel _kernel;
    private readonly string _skillsPath;

    public AIService(
        ILogger<AIService> logger,
        IOptions<AppConfiguration> config,
        Kernel kernel)
    {
        _logger = logger;
        _config = config.Value.AI;
        _kernel = kernel;
        _skillsPath = Path.Combine(AppContext.BaseDirectory, "Skills");
    }

    public async Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default)
    {
        var chat = _kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);
        var response = await chat.GetChatMessageContentsAsync(chatHistory, cancellationToken: ct);
        return string.Join("\n", response.Select(r => r.Content ?? ""));
    }

    public async Task<ChatMessageContent> GetChatCompletionAsync(ChatHistory chatHistory, CancellationToken ct = default)
    {
        var chat = _kernel.GetRequiredService<IChatCompletionService>();
        var response = await chat.GetChatMessageContentsAsync(chatHistory, cancellationToken: ct);
        return response.First();
    }

    public async Task<ResumeAnalysisModel?> AnalyzeResumeAsync(string resumeText, CancellationToken ct = default)
    {
        var skillPrompt = await LoadSkillPromptAsync("ResumeIntelligence/SKILL.md");
        var fullPrompt = skillPrompt + "\n\nResume Text:\n" + resumeText;
        var result = await ExecuteAIAnalysis<ResumeAnalysisModel>(fullPrompt, ct);
        return result;
    }

    public async Task<JDAnalysisModel?> AnalyzeJobDescriptionAsync(string jdText, string targetRole, CancellationToken ct = default)
    {
        var skillPrompt = await LoadSkillPromptAsync("JDIntelligence/SKILL.md");
        var fullPrompt = skillPrompt + "\n\nJob Description Text:\n" + jdText + "\n\nTarget Role: " + targetRole;
        var result = await ExecuteAIAnalysis<JDAnalysisModel>(fullPrompt, ct);
        return result;
    }

    public async Task<SkillMatchResultModel?> MatchSkillsAsync(ResumeAnalysisModel resume, JDAnalysisModel jd, CancellationToken ct = default)
    {
        var skillPrompt = await LoadSkillPromptAsync("SkillMatching/SKILL.md");
        var resumeJson = JsonSerializer.Serialize(resume);
        var jdJson = JsonSerializer.Serialize(jd);
        var fullPrompt = skillPrompt + "\n\nResume Intelligence:\n" + resumeJson + "\n\nJD Intelligence:\n" + jdJson;
        var result = await ExecuteAIAnalysis<SkillMatchResultModel>(fullPrompt, ct);
        return result;
    }

    public async Task<TailoringResultModel?> TailorResumeAsync(ResumeAnalysisModel resume, JDAnalysisModel jd, SkillMatchResultModel match, string targetRole, CancellationToken ct = default)
    {
        var skillPrompt = await LoadSkillPromptAsync("ResumeTailoring/SKILL.md");
        var resumeJson = JsonSerializer.Serialize(resume);
        var jdJson = JsonSerializer.Serialize(jd);
        var matchJson = JsonSerializer.Serialize(match);
        var fullPrompt = skillPrompt + "\n\nResume Intelligence:\n" + resumeJson + "\n\nJD Intelligence:\n" + jdJson + "\n\nSkill Match:\n" + matchJson + "\n\nTarget Role: " + targetRole;
        var result = await ExecuteAIAnalysis<TailoringResultModel>(fullPrompt, ct);
        return result;
    }

    public async Task<ATSAnalysisModel?> ValidateATSAsync(TailoringResultModel tailoredResume, JDAnalysisModel jd, string resumeText, CancellationToken ct = default)
    {
        var skillPrompt = await LoadSkillPromptAsync("ATSValidation/SKILL.md");
        var tailoredJson = JsonSerializer.Serialize(tailoredResume);
        var jdJson = JsonSerializer.Serialize(jd);
        var fullPrompt = skillPrompt + "\n\nTailored Resume:\n" + tailoredJson + "\n\nJD Intelligence:\n" + jdJson + "\n\nOriginal Resume:\n" + resumeText;
        var result = await ExecuteAIAnalysis<ATSAnalysisModel>(fullPrompt, ct);
        return result;
    }

    public async Task<GuardrailResultModel?> ValidateGuardrailsAsync(TailoringResultModel tailoredResume, string originalResumeText, ResumeAnalysisModel resume, SkillMatchResultModel match, CancellationToken ct = default)
    {
        var skillPrompt = await LoadSkillPromptAsync("Guardrails/SKILL.md");
        var tailoredJson = JsonSerializer.Serialize(tailoredResume);
        var resumeJson = JsonSerializer.Serialize(resume);
        var matchJson = JsonSerializer.Serialize(match);
        var fullPrompt = skillPrompt + "\n\nTailored Resume:\n" + tailoredJson + "\n\nOriginal Resume Text:\n" + originalResumeText + "\n\nResume Intelligence:\n" + resumeJson + "\n\nSkill Match:\n" + matchJson;
        var result = await ExecuteAIAnalysis<GuardrailResultModel>(fullPrompt, ct);
        return result;
    }

    public async Task<string> ExtractSkillsAsync(string resumeText, CancellationToken ct = default)
    {
        var prompt = "Extract all skills mentioned in the resume text. Return a comma-separated list of skills.\n\nResume Text:\n" + resumeText;

        try
        {
            var chat = _kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);
            var response = await chat.GetChatMessageContentsAsync(
                chatHistory,
                new OpenAIPromptExecutionSettings
                {
                    Temperature = (float)_config.Temperature,
                    MaxTokens = _config.MaxTokens
                },
                cancellationToken: ct);
            return string.Join("\n", response.Select(r => r.Content ?? ""));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract skills from resume");
            return string.Empty;
        }
    }

    private async Task<T?> ExecuteAIAnalysis<T>(string prompt, CancellationToken ct) where T : class
    {
        try
        {
            var chat = _kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);
            var response = await chat.GetChatMessageContentsAsync(
                chatHistory,
                new OpenAIPromptExecutionSettings
                {
                    Temperature = (float)_config.Temperature,
                    MaxTokens = _config.MaxTokens
                },
                cancellationToken: ct);

            var json = string.Join("\n", response.Select(r => r.Content ?? ""));
            var model = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute AI analysis");
            return null;
        }
    }

    private async Task<string> LoadSkillPromptAsync(string skillRelativePath)
    {
        var skillPath = Path.Combine(_skillsPath, skillRelativePath);
        if (!File.Exists(skillPath))
        {
            _logger.LogWarning("Skill prompt file not found: {Path}", skillPath);
            return string.Empty;
        }

        return await File.ReadAllTextAsync(skillPath);
    }
}
