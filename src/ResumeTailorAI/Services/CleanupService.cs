using Microsoft.Extensions.Options;
using ResumeTailorAI.Configuration;

namespace ResumeTailorAI.Services;

public class CleanupService : BackgroundService
{
    private readonly ILogger<CleanupService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly SessionConfiguration _config;

    public CleanupService(
        ILogger<CleanupService> logger,
        IServiceProvider serviceProvider,
        IOptions<AppConfiguration> config)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config.Value.Sessions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup");
            }

            await Task.Delay(TimeSpan.FromMinutes(_config.CleanupIntervalMinutes), stoppingToken);
        }

        _logger.LogInformation("Cleanup service stopping");
    }

    private async Task PerformCleanupAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
        var timeout = TimeSpan.FromMinutes(_config.TimeoutMinutes);
        
        var expiredSessions = sessionService.GetExpiredSessions(timeout).ToList();
        
        foreach (var session in expiredSessions)
        {
            try
            {
                await sessionService.DeleteSessionAsync(session.SessionId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup session {SessionId}", session.SessionId);
            }
        }

        var tempPath = _config.TempPath;
        if (!string.IsNullOrEmpty(tempPath) && Directory.Exists(tempPath))
        {
            var orphanedDirs = Directory.GetDirectories(tempPath);
            foreach (var dir in orphanedDirs)
            {
                var dirName = Path.GetFileName(dir);
                var sessionExists = sessionService.GetSession(dirName) != null;
                
                if (!sessionExists)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if ((DateTime.UtcNow - dirInfo.LastWriteTimeUtc) > timeout)
                    {
                        _logger.LogInformation("Cleaning up orphaned directory: {DirName}", dirName);
                        fileService.DeleteDirectory(dir);
                    }
                }
            }
        }
    }
}

public interface ISessionCleanupService
{
    Task CleanupExpiredSessionsAsync(CancellationToken ct = default);
}

public class SessionCleanupService : ISessionCleanupService
{
    private readonly ISessionService _sessionService;
    private readonly IFileService _fileService;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly SessionConfiguration _config;

    public SessionCleanupService(
        ISessionService sessionService,
        IFileService fileService,
        IConfiguration configuration,
        ILogger<SessionCleanupService> logger)
    {
        _sessionService = sessionService;
        _fileService = fileService;
        _logger = logger;
        _config = new SessionConfiguration
        {
            TimeoutMinutes = configuration.GetValue<int>("Sessions:TimeoutMinutes", 30),
            CleanupIntervalMinutes = configuration.GetValue<int>("Sessions:CleanupIntervalMinutes", 10),
            TempPath = configuration.GetValue<string>("Sessions:TempPath") ?? 
                       Path.Combine(AppContext.BaseDirectory, "App_Data", "TempSessions")
        };
    }

    public async Task CleanupExpiredSessionsAsync(CancellationToken ct = default)
    {
        var timeout = TimeSpan.FromMinutes(_config.TimeoutMinutes);
        var expiredSessions = _sessionService.GetExpiredSessions(timeout).ToList();

        foreach (var session in expiredSessions)
        {
            try
            {
                await _sessionService.DeleteSessionAsync(session.SessionId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup session {SessionId}", session.SessionId);
            }
        }
    }
}