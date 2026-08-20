using System.Text;
using Markdig;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ResumeTailorAI.Services;

public interface IDocumentParser
{
    Task<DocumentText> ParseAsync(string filePath, CancellationToken ct = default);
    bool CanParse(string fileExtension);
}

public class DocumentText
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public int CharacterCount { get; set; }
}

public class PdfDocumentParser : IDocumentParser
{
    public bool CanParse(string fileExtension)
    {
        return fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<DocumentText> ParseAsync(string filePath, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(filePath);
        var result = new DocumentText
        {
            FileName = fileName,
            FileType = "pdf"
        };

        try
        {
            using var pdf = PdfDocument.Open(filePath);
            var text = new StringBuilder();
            int pageCount = 0;

            foreach (var page in pdf.GetPages().Where(p => !ct.IsCancellationRequested))
            {
                text.AppendLine(page.Text);
                pageCount++;
            }

            result.ExtractedText = text.ToString();
            result.PageCount = pageCount;
            result.CharacterCount = result.ExtractedText.Length;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse PDF: {fileName}", ex);
        }

        return result;
    }
}

public class DocxDocumentParser : IDocumentParser
{
    public bool CanParse(string fileExtension)
    {
        return fileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<DocumentText> ParseAsync(string filePath, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(filePath);
        var result = new DocumentText
        {
            FileName = fileName,
            FileType = "docx",
            PageCount = 1
        };

        try
        {
            var text = new StringBuilder();

            using var archive = new System.IO.Compression.ZipArchive(File.OpenRead(filePath), System.IO.Compression.ZipArchiveMode.Read);
            
            var document = archive.Entries.FirstOrDefault(e => e.FullName.Equals("word/document.xml", StringComparison.OrdinalIgnoreCase));
            
            if (document != null)
            {
                using var stream = document.Open();
                using var reader = new StreamReader(stream);
                var xmlContent = await reader.ReadToEndAsync();
                
                text.Append(ExtractTextFromWordXml(xmlContent));
            }

            result.ExtractedText = text.ToString();
            result.CharacterCount = result.ExtractedText.Length;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse DOCX: {fileName}", ex);
        }

        return result;
    }

    private static string ExtractTextFromWordXml(string xmlContent)
    {
        var text = new StringBuilder();
        var inText = false;
        var currentText = new StringBuilder();

        for (int i = 0; i < xmlContent.Length; i++)
        {
            if (xmlContent[i] == '<')
            {
                var closeIndex = xmlContent.IndexOf('>', i);
                if (closeIndex == -1) break;

                var tag = xmlContent.Substring(i, closeIndex - i + 1);

                if (tag.StartsWith("<w:t") || tag.StartsWith("<w:instrText"))
                {
                    inText = true;
                }
                else if (tag.StartsWith("</w:t>") || tag.StartsWith("</w:instrText>"))
                {
                    if (inText && currentText.Length > 0)
                    {
                        text.Append(currentText.ToString());
                        currentText.Clear();
                    }
                    inText = false;
                }
                else if (tag.StartsWith("</w:p>") || tag.StartsWith("</w:tbl>"))
                {
                    text.AppendLine();
                }

                i = closeIndex;
            }
            else if (inText)
            {
                currentText.Append(xmlContent[i]);
            }
        }

        return text.ToString();
    }
}

public class MarkdownDocumentParser : IDocumentParser
{
    public bool CanParse(string fileExtension)
    {
        return fileExtension.Equals(".md", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<DocumentText> ParseAsync(string filePath, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(filePath);
        var result = new DocumentText
        {
            FileName = fileName,
            FileType = "md",
            PageCount = 1
        };

        try
        {
            var markdown = await File.ReadAllTextAsync(filePath, ct);
            var pipeline = new MarkdownPipelineBuilder().Build();
            var html = Markdown.ToHtml(markdown, pipeline);
            
            result.ExtractedText = StripHtml(html);
            result.CharacterCount = result.ExtractedText.Length;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse Markdown: {fileName}", ex);
        }

        return result;
    }

    private static string StripHtml(string html)
    {
        var text = new StringBuilder();
        var inTag = false;

        foreach (var c in html)
        {
            if (c == '<')
            {
                inTag = true;
            }
            else if (c == '>')
            {
                inTag = false;
                text.Append('\n');
            }
            else if (!inTag)
            {
                text.Append(c);
            }
        }

        return text.ToString();
    }
}

public class TextDocumentParser : IDocumentParser
{
    public bool CanParse(string fileExtension)
    {
        return fileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<DocumentText> ParseAsync(string filePath, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(filePath);
        var result = new DocumentText
        {
            FileName = fileName,
            FileType = "txt",
            PageCount = 1
        };

        try
        {
            result.ExtractedText = await File.ReadAllTextAsync(filePath, ct);
            result.CharacterCount = result.ExtractedText.Length;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse text file: {fileName}", ex);
        }

        return result;
    }
}

public class DocumentParserService : IDocumentParserService
{
    private readonly List<IDocumentParser> _parsers;
    private readonly ILogger<DocumentParserService> _logger;

    public DocumentParserService(ILogger<DocumentParserService> logger)
    {
        _parsers = new List<IDocumentParser>
        {
            new PdfDocumentParser(),
            new DocxDocumentParser(),
            new MarkdownDocumentParser(),
            new TextDocumentParser()
        };
        _logger = logger;
    }

    public async Task<DocumentText> ParseAsync(string filePath, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var parser = _parsers.FirstOrDefault(p => p.CanParse(extension));

        if (parser == null)
        {
            throw new NotSupportedException($"Unsupported file type: {extension}");
        }

        return await parser.ParseAsync(filePath, ct);
    }

    public bool CanParse(string fileExtension)
    {
        return _parsers.Any(p => p.CanParse(fileExtension));
    }

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken ct = default)
    {
        var document = await ParseAsync(filePath, ct);
        return document.ExtractedText;
    }
}

public interface IDocumentParserService
{
    Task<DocumentText> ParseAsync(string filePath, CancellationToken ct = default);
    Task<string> ExtractTextAsync(string filePath, CancellationToken ct = default);
    bool CanParse(string fileExtension);
}