#pragma warning disable MAAI001

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResumeTailorAI.Configuration;
using ResumeTailorAI.Models;
using ResumeTailorAI.Services;

namespace ResumeTailorAI.Agents;

public interface IAgentOrchestrator
{
    Task<AgentContext> RunAnalysisAsync(string sessionId, CancellationToken ct = default);
    Task<AgentContext> RunFullPipelineAsync(string sessionId, CancellationToken ct = default);
}

public class AgentOrchestrator : IAgentOrchestrator
{
    private readonly IHarnessAgentFactory _agentFactory;
    private readonly ISessionService _sessionService;
    private readonly IAIService _aiService;
    private readonly ILogger<AgentOrchestrator> _logger;
    private readonly AppConfiguration _config;

    public AgentOrchestrator(
        IHarnessAgentFactory agentFactory,
        ISessionService sessionService,
        IAIService aiService,
        IOptions<AppConfiguration> config,
        ILogger<AgentOrchestrator> logger)
    {
        _agentFactory = agentFactory;
        _sessionService = sessionService;
        _aiService = aiService;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<AgentContext> RunAnalysisAsync(string sessionId, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting analysis for session {SessionId}", sessionId);

        var session = _sessionService.GetSession(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found: " + sessionId);
        }

        var context = new AgentContext
        {
            SessionId = sessionId,
            Session = session,
            TargetRole = session.TargetRole,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            await RunPipelineAsync(context, ct);
            context.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Analysis completed for session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed for session {SessionId}", sessionId);
            context.ErrorMessage = ex.Message;
            context.Errors.Add(ex.Message);
        }

        _sessionService.UpdateSession(session);
        return context;
    }

    public async Task<AgentContext> RunFullPipelineAsync(string sessionId, CancellationToken ct = default)
    {
        return await RunAnalysisAsync(sessionId, ct);
    }

    private async Task RunPipelineAsync(AgentContext context, CancellationToken ct)
    {
        var session = context.Session;
        if (session == null)
        {
            throw new InvalidOperationException("Session is null");
        }

        context.Progress.ResumeParsed = true;
        context.Progress.JdParsed = true;

        // Use HarnessAgent to run the analysis pipeline
        var agent = _agentFactory.CreateAgent(context.SessionId, ct);
        var agentSession = await _agentFactory.CreateSessionAsync(agent, ct);

        var prompt = $@"
Analyze the resume and job description to tailor the resume for the target role.

Target Role: {session.TargetRole}

Resume Text:
{session.ResumeText}

Job Description:
{session.JDText}

Please perform the following steps in order:
1. Resume Intelligence - Parse the resume and extract structured information
2. JD Intelligence - Analyze the job description and extract requirements
3. Skill Matching - Compare resume skills against JD requirements
4. Resume Tailoring - Generate tailored resume content based on the analysis
5. ATS Validation - Validate the tailored resume for ATS compatibility
6. Guardrails - Validate that no claims are fabricated

After each step, save the results to file memory so they can be retrieved later.
The guardrails step must PASS before any document generation. If guardrails fail, stop and report the issues.

All analysis results must be saved as JSON in the session's file memory for later retrieval.
";

        var response = await agent.RunAsync(prompt, agentSession, cancellationToken: ct);

        // Retrieve results from the agent's file memory or state
        await RetrieveResultsFromAgent(agent, agentSession, context, session, ct);

        if (context.Progress.GuardrailCompleted && context.GuardrailResult?.Status == "FAIL")
        {
            _logger.LogWarning("Guardrail validation failed for session {SessionId}", context.SessionId);
            context.Errors.Add("Guardrail validation failed. Unsupported claims detected.");
        }
    }

    private async Task RetrieveResultsFromAgent(AIAgent agent, AgentSession agentSession, AgentContext context, ResumeTailorSession session, CancellationToken ct)
    {
        if (context.ResumeAnalysis == null)
        {
            _logger.LogInformation("Retrieving Resume Intelligence for session {SessionId}", context.SessionId);
            context.ResumeAnalysis = await _aiService.AnalyzeResumeAsync(session.ResumeText, ct) ?? new ResumeAnalysisModel();
            session.AnalysisResult = context.ResumeAnalysis;
        }

        if (context.JDAnalysis == null)
        {
            _logger.LogInformation("Retrieving JD Intelligence for session {SessionId}", context.SessionId);
            context.JDAnalysis = await _aiService.AnalyzeJobDescriptionAsync(session.JDText, context.TargetRole, ct) ?? new JDAnalysisModel { TargetRole = context.TargetRole };
            session.JDAnalysis = context.JDAnalysis;
        }

        var resumeAnalysis = context.ResumeAnalysis!;
        var jdAnalysis = context.JDAnalysis!;

        if (context.SkillMatchResult == null)
        {
            _logger.LogInformation("Retrieving Skill Matching for session {SessionId}", context.SessionId);
            context.SkillMatchResult = await _aiService.MatchSkillsAsync(resumeAnalysis, jdAnalysis, ct)
                ?? new SkillMatchResultModel();
            session.SkillMatchResult = context.SkillMatchResult;
            _logger.LogInformation("Step 3 completed: Skill Matching for session {SessionId}", context.SessionId);
        }

        var skillMatch = context.SkillMatchResult!;

        if (context.TailoringResult == null)
        {
            _logger.LogInformation("Retrieving Resume Tailoring for session {SessionId}", context.SessionId);
            context.TailoringResult = await _aiService.TailorResumeAsync(resumeAnalysis, jdAnalysis, skillMatch, context.TargetRole, ct) ?? new TailoringResultModel();
            session.TailoringResult = context.TailoringResult;
            _logger.LogInformation("Step 4 completed: Resume Tailoring for session {SessionId}", context.SessionId);
        }

        var tailoringResult = context.TailoringResult!;

        if (context.AtsResult == null)
        {
            _logger.LogInformation("Retrieving ATS Validation for session {SessionId}", context.SessionId);
            context.AtsResult = await _aiService.ValidateATSAsync(tailoringResult, jdAnalysis, session.ResumeText, ct) ?? new ATSAnalysisModel();
            session.AtsResult = context.AtsResult;
            _logger.LogInformation("Step 5 completed: ATS Validation for session {SessionId}", context.SessionId);
        }

        if (context.GuardrailResult == null)
        {
            _logger.LogInformation("Retrieving Guardrail Validation for session {SessionId}", context.SessionId);
            context.GuardrailResult = await _aiService.ValidateGuardrailsAsync(
                tailoringResult, session.ResumeText, resumeAnalysis,
                skillMatch, ct)
                ?? new GuardrailResultModel { Status = "FAIL", Summary = "Validation failed - unable to validate" };
            session.GuardrailResult = context.GuardrailResult;
            context.Progress.GuardrailCompleted = true;
            _logger.LogInformation("Step 6 completed: Guardrail Validation for session {SessionId}", context.SessionId);
        }
    }
}
