using Microsoft.Extensions.Options;
using ResumeTailorAI.Configuration;
using ResumeTailorAI.Models;

namespace ResumeTailorAI.Services;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string sessionId, CancellationToken ct = default);
    void DeleteFile(string filePath);
    void DeleteDirectory(string directoryPath);
    string GetSafeFileName(string fileName);
    bool IsValidFileExtension(string fileName, List<string> allowedExtensions);
    bool IsValidMimeType(string fileName, string mimeType, Dictionary<string, List<string>> allowedMimeTypes);
    bool IsFileSizeValid(long fileSize, int maxSizeMB);
    Task<bool> ValidateFileAsync(IFormFile file, CancellationToken ct = default);
    string ValidateUpload(IFormFile file, string documentType, CancellationToken ct = default);
    Task<string> SaveUploadedFileAsync(IFormFile file, string sessionId, string documentType, CancellationToken ct = default);
    long GetDirectorySize(string directoryPath);
}

public class FileService : IFileService
{
    private readonly FileConfiguration _config;
    private readonly ILogger<FileService> _logger;
    private readonly string _tempPath;

    public FileService(
        IOptions<AppConfiguration> config,
        IWebHostEnvironment environment,
        ILogger<FileService> logger)
    {
        _config = config.Value.Files;
        _logger = logger;
        _tempPath = string.IsNullOrEmpty(config.Value.Sessions.TempPath) 
            ? Path.Combine(environment.ContentRootPath, "App_Data", "TempSessions")
            : config.Value.Sessions.TempPath;
        
        Directory.CreateDirectory(_tempPath);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string sessionId, CancellationToken ct = default)
    {
        var sessionPath = Path.Combine(_tempPath, sessionId);
        Directory.CreateDirectory(sessionPath);

        var safeFileName = GetSafeFileName(file.FileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss_");
        var uniqueFileName = $"{timestamp}{safeFileName}";
        var filePath = Path.Combine(sessionPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        {
            await file.CopyToAsync(stream, ct);
        }

        _logger.LogInformation("File saved for session {SessionId}: {FileName}", sessionId, safeFileName);
        return filePath;
    }

    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FilePath}", Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file: {FilePath}", filePath);
            }
        }
    }

    public void DeleteDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            try
            {
                Directory.Delete(directoryPath, recursive: true);
                _logger.LogInformation("Directory deleted: {DirectoryPath}", Path.GetFileName(directoryPath));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete directory: {DirectoryPath}", directoryPath);
            }
        }
    }

    public string GetSafeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        safeName = Path.GetFileName(safeName);
        return safeName;
    }

    public bool IsValidFileExtension(string fileName, List<string> allowedExtensions)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsValidMimeType(string fileName, string mimeType, Dictionary<string, List<string>> allowedMimeTypes)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (allowedMimeTypes.ContainsKey(extension))
        {
            return allowedMimeTypes[extension].Any(m => m.Equals(mimeType, StringComparison.OrdinalIgnoreCase));
        }
        return false;
    }

    public bool IsFileSizeValid(long fileSize, int maxSizeMB)
    {
        var maxSizeBytes = maxSizeMB * 1024L * 1024L;
        return fileSize > 0 && fileSize <= maxSizeBytes;
    }

    public async Task<bool> ValidateFileAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("File validation failed: file is null or empty");
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!IsValidFileExtension(file.FileName, _config.AllowedResumeExtensions))
        {
            _logger.LogWarning("File validation failed: unsupported extension {Extension}", extension);
            return false;
        }

        if (!IsFileSizeValid(file.Length, _config.MaxResumeSizeMB))
        {
            _logger.LogWarning("File validation failed: file size {Size} exceeds limit {Limit}", file.Length, _config.MaxResumeSizeMB);
            return false;
        }

        var mimeType = file.ContentType;
        if (!IsValidMimeType(file.FileName, mimeType, _config.AllowedMimeTypes))
        {
            _logger.LogWarning("File validation failed: invalid MIME type {MimeType}", mimeType);
            return false;
        }

        return true;
    }

    public long GetDirectorySize(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return 0;

        return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
            .Sum(f => new FileInfo(f).Length);
    }

    public string ValidateUpload(IFormFile file, string documentType, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return "File is null or empty";

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        List<string> allowedExtensions;
        int maxSizeMB;

        if (documentType == "Resume")
        {
            allowedExtensions = _config.AllowedResumeExtensions;
            maxSizeMB = _config.MaxResumeSizeMB;
        }
        else
        {
            allowedExtensions = _config.AllowedJDExtensions;
            maxSizeMB = _config.MaxJDSizeMB;
        }

        if (!IsValidFileExtension(file.FileName, allowedExtensions))
            return $"File extension {extension} is not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}";

        if (!IsFileSizeValid(file.Length, maxSizeMB))
            return $"File size exceeds the maximum allowed size of {maxSizeMB}MB";

        var mimeType = file.ContentType;
        if (!IsValidMimeType(file.FileName, mimeType, _config.AllowedMimeTypes))
            return $"File type {mimeType} is not allowed";

        return string.Empty;
    }

    public async Task<string> SaveUploadedFileAsync(IFormFile file, string sessionId, string documentType, CancellationToken ct = default)
    {
        var sessionPath = Path.Combine(_tempPath, sessionId);
        Directory.CreateDirectory(sessionPath);

        var safeFileName = GetSafeFileName(file.FileName);
        var prefix = documentType == "Resume" ? "Resume_" : "JD_";
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss_");
        var uniqueFileName = $"{prefix}{timestamp}{safeFileName}";
        var filePath = Path.Combine(sessionPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        {
            await file.CopyToAsync(stream, ct);
        }

        _logger.LogInformation("{DocumentType} file saved for session {SessionId}: {FileName}",
            documentType, sessionId, safeFileName);
        return filePath;
    }
}