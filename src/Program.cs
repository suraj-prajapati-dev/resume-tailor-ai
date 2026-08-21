using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using ResumeTailorAI.Agents;
using ResumeTailorAI.Configuration;
using ResumeTailorAI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppConfiguration>(builder.Configuration.GetSection("App"));

builder.Services.Configure<AIConfiguration>(builder.Configuration.GetSection("App:AI"));

builder.Services.Configure<FileConfiguration>(builder.Configuration.GetSection("App:Files"));

builder.Services.Configure<SessionConfiguration>(builder.Configuration.GetSection("App:Sessions"));

builder.Services.Configure<ScoringConfiguration>(builder.Configuration.GetSection("App:Scoring"));

builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IAIService, AIService>();
builder.Services.AddSingleton<IResumeParserService, ResumeParserService>();
builder.Services.AddSingleton<IJobDescriptionParserService, JobDescriptionParserService>();
builder.Services.AddSingleton<IDocumentParserService, DocumentParserService>();
builder.Services.AddSingleton<ISkillMatchingService, SkillMatchingService>();
builder.Services.AddSingleton<IAtsValidationService, AtsValidationService>();
builder.Services.AddSingleton<IDocumentGenerationService, DocumentGenerationService>();
builder.Services.AddSingleton<ISessionCleanupService, SessionCleanupService>();

builder.Services.AddSingleton<ISessionService, SessionService>();
builder.Services.AddSingleton<IResumeTailorHarness, ResumeTailorHarness>();
builder.Services.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();

builder.Services.AddSingleton<IHarnessAgentFactory, HarnessAgentFactory>();

builder.Services.AddHostedService<CleanupService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAntiforgery();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<Kernel>(sp =>
{
    var config = sp.GetRequiredService<IOptions<AppConfiguration>>().Value;
    var kernelBuilder = Kernel.CreateBuilder();

    if (!string.IsNullOrEmpty(config.AI.ApiKey))
    {
        kernelBuilder.AddOpenAIChatCompletion(config.AI.Model, config.AI.ApiKey);
    }

    return kernelBuilder.Build();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapFallbackToFile("/index.html");

app.Run();

public partial class Program { }