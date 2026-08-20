using System.Collections.Concurrent;

namespace ResumeTailorAI.Skills;

public class SkillLoader
{
    private readonly string _skillsPath;
    private readonly ConcurrentDictionary<string, string> _skillCache;

    public SkillLoader(string skillsPath)
    {
        _skillsPath = skillsPath;
        _skillCache = new ConcurrentDictionary<string, string>();
    }

    public async Task<string> LoadSkillAsync(string skillName, CancellationToken ct = default)
    {
        if (_skillCache.TryGetValue(skillName, out var cached))
        {
            return cached;
        }

        var skillPath = Path.Combine(_skillsPath, skillName, "SKILL.md");
        if (!File.Exists(skillPath))
        {
            return string.Empty;
        }

        var content = await File.ReadAllTextAsync(skillPath, ct);
        _skillCache.TryAdd(skillName, content);
        return content;
    }

    public void ClearCache()
    {
        _skillCache.Clear();
    }
}