using Microsoft.Extensions.Primitives;
using ResumeTailorAI.Models;
using System.Collections.Concurrent;

namespace ResumeTailorAI.Services;

public interface ISessionService
{
    Task<ResumeTailorSession> CreateSessionAsync(string targetRole, CancellationToken ct = default);
    ResumeTailorSession? GetSession(string sessionId);
    void UpdateLastAccessed(string sessionId);
    void UpdateSession(ResumeTailorSession session);
    Task<bool> DeleteSessionAsync(string sessionId, CancellationToken ct = default);
    IEnumerable<ResumeTailorSession> GetAllSessions();
    IEnumerable<ResumeTailorSession> GetExpiredSessions(TimeSpan timeout);
}

public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<string, ResumeTailorSession> _sessions;
    private readonly ILogger<SessionService> _logger;
    private readonly IFileService _fileService;
    private readonly Timer _cleanupTimer;

    public SessionService(ILogger<SessionService> logger, IFileService fileService, IConfiguration configuration)
    {
        _sessions = new ConcurrentDictionary<string, ResumeTailorSession>();
        _logger = logger;
        _fileService = fileService;

        var cleanupInterval = configuration.GetValue<int>("Sessions:CleanupIntervalMinutes", 10);
        _cleanupTimer = new Timer(async _ => await CleanupExpiredSessionsAsync(), 
            null, 
            TimeSpan.FromMinutes(cleanupInterval), 
            TimeSpan.FromMinutes(cleanupInterval));
    }

    public async Task<ResumeTailorSession> CreateSessionAsync(string targetRole, CancellationToken ct = default)
    {
        var session = new ResumeTailorSession
        {
            SessionId = Guid.NewGuid().ToString(),
            TargetRole = targetRole,
            CreatedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };

        _sessions.TryAdd(session.SessionId, session);
        _logger.LogInformation("Session created: {SessionId}", session.SessionId);
        return session;
    }

    public ResumeTailorSession? GetSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.LastAccessedAt = DateTime.UtcNow;
            return session;
        }
        return null;
    }

    public void UpdateLastAccessed(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.LastAccessedAt = DateTime.UtcNow;
        }
    }

    public void UpdateSession(ResumeTailorSession session)
    {
        _sessions[session.SessionId] = session;
        session.LastAccessedAt = DateTime.UtcNow;
    }

    public async Task<bool> DeleteSessionAsync(string sessionId, CancellationToken ct = default)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            await CleanupSessionAsync(session, ct);
            _logger.LogInformation("Session deleted: {SessionId}", sessionId);
            return true;
        }
        return false;
    }

    public IEnumerable<ResumeTailorSession> GetAllSessions()
    {
        return _sessions.Values;
    }

    public IEnumerable<ResumeTailorSession> GetExpiredSessions(TimeSpan timeout)
    {
        var now = DateTime.UtcNow;
        return _sessions.Values.Where(s => (now - s.LastAccessedAt) > timeout);
    }

    private async Task CleanupExpiredSessionsAsync()
    {
        try
        {
            var timeoutMinutes = 30;
            var timeout = TimeSpan.FromMinutes(timeoutMinutes);
            var expiredSessions = GetExpiredSessions(timeout).ToList();

            foreach (var session in expiredSessions)
            {
                if (_sessions.TryRemove(session.SessionId, out _))
                {
                    await CleanupSessionAsync(session, CancellationToken.None);
                }
            }

            if (expiredSessions.Any())
            {
                _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during session cleanup");
        }
    }

    private async Task CleanupSessionAsync(ResumeTailorSession session, CancellationToken ct)
    {
        try
        {
            if (!string.IsNullOrEmpty(session.ResumeFilePath))
            {
                _fileService.DeleteFile(session.ResumeFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete resume file for session {SessionId}", session.SessionId);
        }

        try
        {
            if (!string.IsNullOrEmpty(session.JDFilePath))
            {
                _fileService.DeleteFile(session.JDFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete JD file for session {SessionId}", session.SessionId);
        }

        try
        {
            if (!string.IsNullOrEmpty(session.GeneratedResumePath))
            {
                _fileService.DeleteFile(session.GeneratedResumePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete generated resume for session {SessionId}", session.SessionId);
        }

        try
        {
            if (!string.IsNullOrEmpty(session.GeneratedCoverLetterPath))
            {
                _fileService.DeleteFile(session.GeneratedCoverLetterPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete generated cover letter for session {SessionId}", session.SessionId);
        }

        var sessionDir = Path.Combine(AppContext.BaseDirectory, "App_Data", "TempSessions", session.SessionId);
        _fileService.DeleteDirectory(sessionDir);

        await Task.CompletedTask;
    }
}