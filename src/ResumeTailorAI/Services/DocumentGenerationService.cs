using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using ResumeTailorAI.Models;
using System.IO;

namespace ResumeTailorAI.Services;

public interface IDocumentGenerationService
{
    Task<string> GenerateResumeAsync(TailoringResultModel tailoredResume, string sessionPath, CancellationToken ct = default);
    Task<string> GenerateCoverLetterAsync(TailoringResultModel tailoredResume, JDAnalysisModel jd, string targetRole, string sessionPath, CancellationToken ct = default);
}

public class DocumentGenerationService : IDocumentGenerationService
{
    private readonly ILogger<DocumentGenerationService> _logger;

    public DocumentGenerationService(ILogger<DocumentGenerationService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateResumeAsync(TailoringResultModel tailoredResume, string sessionPath, CancellationToken ct = default)
    {
var filePath = System.IO.Path.Combine(sessionPath, "TailoredResume.docx");
         
        using (var doc = WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            var resume = tailoredResume.TailoredResume;

            AddHeading(body, "PROFESSIONAL SUMMARY");
            AddParagraph(body, resume.ProfessionalSummary ?? "", true);

            if (resume.CoreCompetencies != null && resume.CoreCompetencies.Any())
            {
                AddHeading(body, "CORE COMPETENCIES");
                AddParagraph(body, string.Join(", ", resume.CoreCompetencies));
            }

            if (resume.TechnicalSkills != null && resume.TechnicalSkills.Any())
            {
                AddHeading(body, "TECHNICAL SKILLS");
                foreach (var cat in resume.TechnicalSkills.OrderBy(s => s.Priority))
                {
                    if (cat.Skills.Any())
                    {
                        var skillsText = cat.Category + ": " + string.Join(", ", cat.Skills);
                        AddBullet(body, skillsText);
                    }
                }
            }

            if (resume.Experience != null && resume.Experience.Any())
            {
                AddHeading(body, "PROFESSIONAL EXPERIENCE");
                foreach (var exp in resume.Experience)
                {
                    AddHeading(body, exp.Employer + " - " + exp.Title, false, true);
                    var dateRange = exp.IsCurrent ? exp.StartDate + " - Present" : exp.StartDate + " - " + exp.EndDate;
                    AddParagraph(body, dateRange, italic: true);

                    if (exp.Bullets != null)
                    {
                        foreach (var bullet in exp.Bullets)
                        {
                            var text = bullet.Tailored ?? bullet.Original;
                            if (!string.IsNullOrEmpty(text))
                            {
                                AddBullet(body, text);
                            }
                        }
                    }
                    AddSpacing(body);
                }
            }

            if (resume.Projects != null && resume.Projects.Any())
            {
                AddHeading(body, "PROJECTS");
                foreach (var proj in resume.Projects)
                {
                    AddHeading(body, proj.Name, false, true);
                    if (!string.IsNullOrEmpty(proj.Description))
                    {
                        AddParagraph(body, proj.Description);
                    }
                    if (proj.Technologies != null && proj.Technologies.Any())
                    {
                        AddBullet(body, "Technologies: " + string.Join(", ", proj.Technologies));
                    }
                    if (proj.Highlights != null)
                    {
                        foreach (var h in proj.Highlights)
                        {
                            AddBullet(body, h);
                        }
                    }
                    AddSpacing(body);
                }
            }

            if (resume.Education != null && resume.Education.Any())
            {
                AddHeading(body, "EDUCATION");
                foreach (var edu in resume.Education)
                {
                    var eduText = edu.Degree + " in " + edu.Field + ", " + edu.Institution + " (" + edu.GraduationYear + ")";
                    AddBullet(body, eduText);
                    if (!string.IsNullOrEmpty(edu.Honors))
                    {
                        AddParagraph(body, "Honors: " + edu.Honors);
                    }
                }
            }

            if (resume.Certifications != null && resume.Certifications.Any())
            {
                AddHeading(body, "CERTIFICATIONS");
                foreach (var cert in resume.Certifications)
                {
                    var certText = cert.Name;
                    if (!string.IsNullOrEmpty(cert.Issuer))
                        certText += ", " + cert.Issuer;
                    if (!string.IsNullOrEmpty(cert.IssueDate))
                        certText += " (" + cert.IssueDate + ")";
                    AddBullet(body, certText);
                }
            }
        }

        _logger.LogInformation("Resume generated at: {FilePath}", filePath);
        return filePath;
    }

    public async Task<string> GenerateCoverLetterAsync(TailoringResultModel tailoredResume, JDAnalysisModel jd, string targetRole, string sessionPath, CancellationToken ct = default)
    {
var filePath = System.IO.Path.Combine(sessionPath, "CoverLetter.docx");
 
        using (var doc = WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            var resume = tailoredResume.TailoredResume;
            var candidate = new CandidateInfo();
            
            var techSkills = string.Join(", ", resume.TechnicalSkills
                .Where(s => s.Skills.Any())
                .SelectMany(s => s.Skills)
                .Take(8));

            var experienceBullets = resume.Experience
                .SelectMany(e => e.Bullets)
                .Select(b => b.Tailored)
                .Where(b => !string.IsNullOrEmpty(b))
                .Take(5)
                .ToList();

            var relevantExperience = string.Join(" ", experienceBullets);
            var technicalStrengths = "My technical expertise includes " + techSkills + ", which aligns with the requirements for " + targetRole + ".";
            var architectureStrengths = GetArchitectureStrengths(resume);
            var whyCandidateFits = "Based on my experience with " + techSkills + " and my track record in delivering high-quality solutions, I am confident I can contribute effectively to " + (jd.Company ?? "your company") + " as a " + targetRole + ".";

            AddParagraph(body, DateTime.Now.ToString("MMMM d, yyyy"));
            AddSpacing(body);

            AddHeading(body, targetRole + " - Cover Letter", true);
            AddSpacing(body);

            AddParagraph(body, "Dear Hiring Manager,");
            AddSpacing(body);

            AddParagraph(body, "I am writing to express my interest in the " + targetRole + " position at " + (jd.Company ?? "your company") + ".");
            AddSpacing(body);

            AddParagraph(body, relevantExperience);
            AddSpacing(body);

            AddParagraph(body, technicalStrengths);
            AddSpacing(body);

            AddParagraph(body, architectureStrengths);
            AddSpacing(body);

            AddParagraph(body, whyCandidateFits);
            AddSpacing(body);

            AddParagraph(body, "Thank you for considering my application. I look forward to discussing how my experience can contribute to your team.");
            AddSpacing(body);

            AddParagraph(body, "Sincerely,");
            AddParagraph(body, candidate.Name ?? "Candidate Name");
        }

        _logger.LogInformation("Cover letter generated at: {FilePath}", filePath);
        return filePath;
    }

    private string GetArchitectureStrengths(TailoredResume resume)
    {
        var archSkills = resume.TechnicalSkills
            .Where(s => s.Category.Contains("Architecture") || s.Category.Contains("Cloud"))
            .SelectMany(s => s.Skills)
            .ToList();

        if (!archSkills.Any())
        {
            return "While my current focus is on application development, I have experience with system design principles and architectural patterns.";
        }

        return "My experience includes " + string.Join(", ", archSkills) + ", which provides me with strong architectural foundations.";
    }

    private void AddHeading(Body body, string text, bool bold = true, bool italic = false)
    {
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        var props = new RunProperties();
        if (bold) props.Bold = new Bold();
        if (italic) props.Italic = new Italic();
        run.AppendChild(props);
        run.AppendChild(new Text(text));
        AddSpacing(body);
    }

    private void AddParagraph(Body body, string text, bool bold = false, bool italic = false)
    {
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        var props = new RunProperties();
        if (bold) props.Bold = new Bold();
        if (italic) props.Italic = new Italic();
        run.AppendChild(props);
        run.AppendChild(new Text(text));
    }

    private void AddBullet(Body body, string text)
    {
        var para = body.AppendChild(new Paragraph());
        var props = para.AppendChild(new ParagraphProperties());
        var indent = new Indentation();
        indent.Left = "200";
        props.Append(indent);
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(text));
    }

private void AddSpacing(Body body)
{
          var para = body.AppendChild(new Paragraph());
          var props = para.AppendChild(new ParagraphProperties());
          var spacing = new SpacingBetweenLines(){ After = "120" };
          props.Append(spacing);
      }
}
