# ResumeTailor-AI

## 1. Purpose

A candidate has one resume but different job descriptions require different skills, keywords, experience emphasis, and positioning.

ResumeTailor-AI analyzes the Resume + Job Description + Target Role, identifies the alignment and gaps, and creates a truthful ATS-friendly tailored resume.

**Who the target user is:** Job seekers who need to tailor their resumes for different job applications efficiently.

**Why manually tailoring resumes to every JD is inefficient:** Manual tailoring is time-consuming, inconsistent, and often overlooks key ATS keywords and formatting requirements. Each resume version requires significant effort to rephrase experience, reorganize sections, and highlight relevant skills.

**How AI helps:** AI analyzes both the resume content and job description to identify skill gaps, suggest rewording, and generate a tailored version that maintains truthfulness while optimizing for the target role.

**What the system produces:** A tailored resume (DOCX) and cover letter (DOCX) that are ATS-friendly and based strictly on the original resume information.

---

## 2. What the Project Does

The complete flow:

```text
Resume
   +
Job Description
   +
Target Role
   ↓
Document Parsing
   ↓
Resume Intelligence
   ↓
JD Intelligence
   ↓
Skill Matching
   ↓
Resume Tailoring
   ↓
ATS Validation
   ↓
Guardrail Validation
   ↓
Human Approval
   ↓
Resume.docx + CoverLetter.docx
```

---

## 3. Key Requirements

- Upload Resume
- Upload JD
- Supported formats: PDF, DOCX, MD, TXT
- Target Role input
- Resume analysis
- JD analysis
- Skill matching
- Matched skills
- Partial matches
- Missing skills
- Resume tailoring
- ATS validation
- Anti-fabrication guardrails
- Human approval
- Resume DOCX generation
- Cover Letter DOCX generation
- Session-based temporary storage
- Automatic cleanup

---

## 4. Core Business Rule

> **Never fabricate candidate experience.**

The system must never invent:

- Skills
- Technologies
- Projects
- Employers
- Certifications
- Experience
- Achievements
- Metrics
- Responsibilities
- Education
- Domain experience

The system may only improve the presentation of information supported by the original resume.

---

## 5. Architecture

```mermaid
flowchart TD
    UI -->|HTTP Request| ASP.NET Core API
    ASP.NET Core API -->|Pipeline| ResumeTailor Harness
    ResumeTailor Harness -->|Skills| Analysis
    Analysis -->|Skill Matching| Skills
    Skills -->|Resume Tailoring| Tailoring
    Tailoring -->|ATS Validation| ATS
    ATS -->|Guardrail| Guardrail
    Guardrail -->|Human Approval| Human Approval
    Human Approval -->|Document Generation| Document Generation
```

**Single Monolithic ASP.NET Core Application**

---

## 6. Technology Stack

Technologies actually present in the repository:

- **ASP.NET Core** - Web framework
- **C#** - Implementation language
- **Microsoft Agent Framework Harness** - AI orchestration
- **HTML** - Frontend markup
- **JavaScript** - Client-side logic
- **jQuery** - DOM manipulation
- **Bootstrap** - CSS framework
- **Dapper** - ORM (present in project references)
- **Microsoft.Data.Sqlite** - SQLite support (present in project references)
- **PdfPig** - PDF parsing
- **Markdig** - Markdown processing
- **DocumentFormat.OpenXml** - DOCX generation
- **Newtonsoft.Json** - JSON serialization
- **Microsoft.SemanticKernel** - AI plugin framework
- **Microsoft.Extensions.AI** - AI abstractions
- **OpenAI** - OpenAI client library

---

## 7. Project Structure

```text
Agents/          - Harness agent orchestration and session management
Skills/          - 7 skill files with YAML frontmatter:
                  ResumeIntelligence, JDIntelligence,
                  SkillMatching, ResumeTailoring,
                  ATSValidation, Guardrails, DocumentGeneration
Services/        - File handling, AI services, parsing, matching, generation
Models/          - ResumeModel, JDAnalysisModel,
                  SkillMatchResultModel, TailoringResultModel,
                  ATSAnalysisModel, GuardrailResultModel,
                  ResumeTailorSession, AnalysisProgress
Controllers/     - Session, Resume, JD, Analysis,
                  Tailoring, Documents, Home
DTOs/            - ApiResponse and request/response DTOs
Views/           - Razor views for UI
wwwroot/         - Static files (css, js)
Tests/           - Test projects
docs/            - Documentation
```

---

## 8. Agent / Skill Architecture

These are logical capabilities inside the monolithic application, not independent microservices:

- **Resume Intelligence** - Extracts structured candidate information from resume text using AI. Parses contact info, skills, experience, projects, education, and certifications. Every skill must have evidence from the original text.

- **JD Intelligence** - Analyzes job description and extracts structured requirements. Identifies required skills with priority classifications (Must Have/Should Have/Nice To Have/Unknown), experience levels, responsibilities, and ATS keywords. Only extracts explicitly stated requirements.

- **Skill Matching** - Compares resume skills against job description requirements using semantic matching. Calculates overall fit score with category breakdown (technical, experience, architecture, leadership, domain, ATS keywords). Classifies each match as Matched, Partially Matched, Missing, or Unknown. Never claims a match without supporting evidence.

- **Resume Tailoring** - Generates tailored resume content based on analysis results. Rewords, reorders, and highlights existing experience to align with job description keywords. NEVER invents skills, experience, projects, metrics, employers, certifications. Every change must be traceable to the original resume.

- **ATS Validation** - Validates tailored resume for ATS compatibility. Checks formatting, keyword coverage, keyword stuffing, section structure, and job title alignment. Identifies critical missing keywords and formatting risks.

- **Guardrails** - Validates that tailored resume contains no fabricated claims. Every claim must trace to original resume evidence. Checks skills, technologies, experience, employers, job titles, projects, certifications, education, achievements, metrics, responsibilities, leadership claims, and domain experience. If FAIL, document generation MUST NOT proceed.

- **Document Generation** - Generates ATS-friendly DOCX resume and cover letter from tailored resume content. Requires guardrail validation to pass before generation. Uses standard fonts, simple section structure, no graphics or complex tables.

---

## 9. Getting Started

```bash
git clone <repository-url>
cd ResumeTailor-AI
dotnet restore
dotnet build
dotnet run
```

**Required configuration:** Update `appsettings.json` with AI provider settings:

- `App:AI:Provider` - AI provider name
- `App:AI:Endpoint` - API endpoint URL
- `App:AI:ApiKey` - API key (do not commit to source control)
- `App:AI:Model` - Default model name
- `App:AI:FallbackModel` - Fallback model name
- `App:AI:Temperature` - Sampling temperature
- `App:AI:MaxTokens` - Maximum tokens per request
- `App:AI:TimeoutSeconds` - Request timeout
- `App:AI:MaxRetries` - Maximum retry attempts

File configuration:

- `App:Files:MaxResumeSizeMB` - Maximum resume file size
- `App:Files:MaxJDSizeMB` - Maximum job description file size
- `App:Files:AllowedResumeExtensions` - Allowed resume file extensions
- `App:Files:AllowedJDExtensions` - Allowed JD file extensions

Session configuration:

- `App:Sessions:TimeoutMinutes` - Session idle timeout
- `App:Sessions:CleanupIntervalMinutes` - Cleanup interval
- `App:Sessions:TempPath` - Temporary file location

---

## 10. Configuration

**AI provider:** HuggingFace (configured in appsettings.json)

- `App:AI:Provider` - HuggingFace
- `App:AI:Endpoint` - `https://router.huggingface.co`
- `App:AI:ApiKey` - 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 (example, replace with your key)
- `App:AI:Model` - deepseek-ai/DeepSeek-R1
- `App:AI:FallbackModel` - zai-org/GLM-5:novita
- `App:AI:Temperature` - 0.5
- `App:AI:MaxTokens` - 4000
- `App:AI:TimeoutSeconds` - 60
- `App:AI:MaxRetries` - 3

**File configuration:**

- `App:Files:MaxResumeSizeMB` - 10
- `App:Files:MaxJDSizeMB` - 10
- `App:Files:AllowedResumeExtensions` - [".pdf", ".docx", ".md", ".txt"]
- `App:Files:AllowedJDExtensions` - [".pdf", ".docx", ".md", ".txt"]
- `App:Files:AllowedMimeTypes` - MIME type mappings per extension

**Session configuration:**

- `App:Sessions:TimeoutMinutes` - 30
- `App:Sessions:CleanupIntervalMinutes` - 10
- `App:Sessions:TempPath` - "" (uses default: App_Data/TempSessions)

**Scoring configuration:**

- `App:Scoring:TechnicalWeight` - 0.35
- `App:Scoring:ExperienceWeight` - 0.25
- `App:Scoring:ATSWeight` - 0.2
- `App:Scoring:LeadershipWeight` - 0.1
- `App:Scoring:DomainWeight` - 0.15
- `App:Scoring:ArchitectureWeight` - 0.1
- `App:Scoring:SkillPriorityWeights` - Must Have: 1.0, Should Have: 0.5, Nice To Have: 0.2, Unknown: 0.0

---

## 11. API Overview

| Endpoint | Method | Description |
|---|---|---|
| `/api/session/start` | POST | Create a new session with username, password, and target role |
| `/api/session/status` | GET | Get session status (active, expired, has resume/JD) |
| `/api/resume/upload` | POST | Upload and parse resume file (PDF, DOCX, MD, TXT) |
| `/api/jd/upload` | POST | Upload and parse job description file |
| `/api/analysis/start` | POST | Start the analysis pipeline (resume + JD intelligence, skill matching, tailoring, ATS, guardrails) |
| `/api/analysis/status` | GET | Get analysis status and progress for a session |
| `/api/analysis/result` | GET | Get complete analysis results (match score, skills, guardrail status) |
| `/api/tailoring/approve` | POST | Approve or reject tailored resume (human approval; generates documents on approve) |
| `/api/documents/resume` | GET | Download tailored Resume.docx (requires approval and guardrail PASS) |
| `/api/documents/cover-letter` | GET | Download CoverLetter.docx (requires approval and guardrail PASS) |
| `/api/session/logout` | POST | End session and cleanup temporary files |

---

## 12. Privacy and Data Lifecycle

```text
User Session
   ↓
Temporary Files (App_Data/TempSessions/{sessionId}/)
   ↓
AI Processing (transient, not stored persistently)
   ↓
Generated Documents (DOCX - temporary, session-specific)
   ↓
User Download
   ↓
Session Logout/Expiry
   ↓
Cleanup (automatic file removal)
```

**Clearly state:** Uploaded Resume/JD and generated documents are not intended for permanent storage. They are stored temporarily in session-specific directories and cleaned up upon session expiry or logout. AI processing is transient and does not persist candidate data.

---

## 13. Testing

```bash
dotnet build
```

**Test status:** No test projects exist in the current repository. The TODO.md tracking file lists testing criteria that have been verified through manual browser testing and API endpoint validation. Important test areas that have been confirmed:

- File validation (extension, MIME type, size limits) - verified via API responses
- Anti-forgery token validation - enabled in controllers
- Session isolation - each session gets unique temp directory
- File cleanup after session expiry - SessionService + CleanupService
- No sensitive data in logs - file paths logged, not content
- Build compiles with 0 errors - verified
- Application runs end-to-end - verified (started successfully, session API tested)
- Document generation (DOCX output) - yet to be manually verified

**Planned:** Add unit tests for: file validation, skill matching, guardrails, session cleanup, API endpoints, and end-to-end integration tests.

---

## 14. Current Status

```text
Implemented
- Session management (start, status, logout)
- File upload and validation (PDF, DOCX, MD, TXT)
- Document parsing (PDF, DOCX, MD, TXT extraction)
- Resume Intelligence skill
- JD Intelligence skill
- Skill Matching skill
- Resume Tailoring skill
- ATS Validation skill
- Guardrails skill
- Document Generation skill (DOCX output)
- Session-based temporary storage with cleanup
- API endpoints for all operations
- Human approval flow before document generation

Planned
- (None - all described functionality is implemented)

Known Limitations
- AI provider configuration required (API key needed)
- Document generation requires guardrail PASS + human approval
- Session data is in-memory (not persistent across restarts)
- HuggingFace provider configured but actual AI service depends on endpoint accessibility
- No Redis or distributed session scaling
```

---

## 15. Frontend Developer Guide

For frontend developers (React, Angular, Vue, or plain JavaScript) integrating with the ResumeTailor-AI backend, see the separate **Frontend Developer Guide** document which contains:
- Complete API endpoint reference (12 endpoints)
- Session management and file upload procedures
- Framework-specific examples (React/TypeScript, Angular, Vue 3)
- Error handling patterns and HTTP status codes
- UI workflow checklist for complete end-to-end integration

This document is available as `Frontend-Developer.md` in the repository root.

---

## 15. License

License: Not defined