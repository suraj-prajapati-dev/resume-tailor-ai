#pragma warning disable MAAI001

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using ResumeTailorAI.Configuration;
using System.ClientModel;

namespace ResumeTailorAI.Agents;

public interface IHarnessAgentFactory
{
    AIAgent CreateAgent(string sessionId, CancellationToken ct = default);
    Task<AgentSession> CreateSessionAsync(AIAgent agent, CancellationToken ct = default);
}

public class HarnessAgentFactory : IHarnessAgentFactory
{
    private readonly AIConfiguration _config;
    private readonly ILogger<HarnessAgentFactory> _logger;
    private readonly string _skillsPath;

    public HarnessAgentFactory(
        IOptions<AppConfiguration> config,
        ILogger<HarnessAgentFactory> logger)
    {
        _config = config.Value.AI;
        _logger = logger;
        _skillsPath = Path.Combine(AppContext.BaseDirectory, "Skills");
    }

    public AIAgent CreateAgent(string sessionId, CancellationToken ct = default)
    {
        var chatClient = CreateChatClient();

        var options = new HarnessAgentOptions
        {
            Name = "resume-tailor-agent",
            HarnessInstructions = @"You are ResumeTailor-AI, an expert resume tailoring assistant.
Use the available skills to analyze resumes, job descriptions, and create tailored content.
Always validate results against guardrails before producing output.
Never fabricate information - only use what exists in the original resume.
Ask for human approval before generating final documents.",
            ChatOptions = new ChatOptions
            {
                Instructions = "You are ResumeTailor-AI, an AI-powered resume tailoring assistant. Your goal is to help users tailor their resumes to specific job descriptions while ensuring accuracy and compliance with ATS requirements. Your primary directive is to never fabricate information - only enhance existing resume content based on the job description.",
                ModelId = _config.Model,
                Temperature = (float)_config.Temperature,
            },
            AgentModeProviderOptions = new AgentModeProviderOptions
            {
                DefaultMode = "plan",
            },
            AgentSkillsSource = new AgentFileSkillsSource(_skillsPath),
        };

        var agent = chatClient.AsHarnessAgent(options);
        return agent;
    }

    public async Task<AgentSession> CreateSessionAsync(AIAgent agent, CancellationToken ct = default)
    {
        var session = await agent.CreateSessionAsync(ct);
        return session;
    }

    private IChatClient CreateChatClient()
    {
        if (_config.Provider == "AzureOpenAI" && !string.IsNullOrEmpty(_config.Endpoint))
        {
            var endpoint = new Uri(_config.Endpoint);
            var credential = new ApiKeyCredential(_config.ApiKey);
            var client = new global::Azure.AI.OpenAI.AzureOpenAIClient(endpoint, credential);
            return (IChatClient)client.GetChatClient(_config.Model);
        }
        else
        {
            var client = new OpenAIClient(_config.ApiKey);
            return (IChatClient)client.GetChatClient(_config.Model);
        }
    }
}