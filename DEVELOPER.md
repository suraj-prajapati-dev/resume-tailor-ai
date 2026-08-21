# DEVELOPER.md

## 1. Development Objective

Technical purpose of ResumeTailor-AI: A monolithic ASP.NET Core application that tailors resumes to specific job descriptions using AI. The system analyzes a candidate's resume and a job description, identifies skill alignments and gaps, and generates a truthful, ATS-friendly tailored resume and cover letter. The monolithic architecture avoids unnecessary microservices complexity while providing a complete resume tailoring pipeline from file upload to document generation.

## 2. Architecture Rules

**Single monolithic application:** All functionality resides in one ASP.NET Core project. No microservices decomposition.

**No microservices:** The Microsoft Agent Framework Harness provides AI orchestration within the same process, not as separate services.

**No unnecessary infrastructure:** No Redis, Kafka, RabbitMQ, or other message brokers.

**ASP.NET Core backend:** Full MVC/Razor Pages and Web API controllers.

**Simple HTML + jQuery + Bootstrap frontend:** No complex frontend frameworks (React, Angular, Vue).

**Microsoft Agent Framework Harness for AI orchestration:** The HarnessAgent provides built-in function calling, history persistence, todo management, and file memory. Skills are loaded from .md files in the Skills folder.

**Skills stored separately:** 7 skill files in the Skills folder with YAML frontmatter format. Each skill defines its purpose, inputs, rules, and output format.

**Temporary session-based document processing:** Resume and JD files are stored temporarily in session-specific directories under App_Data/TempSessions/{sessionId}. All generated documents and temporary files are cleaned up on session expiry or logout.

**No permanent Resume/JD storage:** Uploaded files are not intended for persistent storage. They exist only for the duration of the user session and are automatically cleaned up.

**Human approval before final document generation:** The guardrail validation must PASS before documents can be generated, and human approval is required to approve the tailored resume before document generation proceeds.

## 3. Development Principles

```text
Keep it simple
Prefer existing framework capabilities
Do not over-engineer
Small classes
Small methods
Clear responsibilities
Dependency Injection
Async/await
CancellationToken
Configuration over hardcoding
```

**Avoid unnecessary:**

- CQRS
- MediatR
- Repository pattern everywhere
- Event bus
- Microservices
- Redis
- Kafka
- RabbitMQ
- Complex frontend frameworks

These are only used if a real requirement demands them, which none currently do in this project.

## 4. Agent Rules

```text
Harness
   ↓
Skill
   ↓
Structured Output
   ↓
Validation
```

**Important rules:**

- **Never trust raw AI output** - Always validate AI results through structured models and guardrails
- **Prefer structured JSON** - AI prompts should request structured output that can be deserialized into model classes
- **Validate AI output** - Check for null, validate required fields, ensure invariants hold
- **Never bypass guardrails** - Guardrail validation must pass before document generation
- **Never generate final documents before human approval** - The approval flow must be completed first
- **Never fabricate candidate information** - All tailored content must trace to original resume evidence

## 5. Skill Development Rules

Each skill should be created/updated in `Skills/<SkillName>/SKILL.md` with this structure:

```text
Purpose
Inputs
Instructions
Business Rules
Output Format
Validation Rules
Failure Conditions
Examples
```

**Business instructions should be in skill files** instead of unnecessarily hardcoding large prompts in C#. The AIService.LoadSkillPromptAsync method reads these .md files and uses them as prompts for the AI.

Current skills and their locations:

- `Skills/ResumeIntelligence/SKILL.md` - Resume parsing and structured extraction
- `Skills/JDIntelligence/SKILL.md` - Job description analysis and requirement extraction
- `Skills/SkillMatching/SKILL.md` - Skill comparison and matching
- `Skills/ResumeTailoring/SKILL.md` - Resume content tailoring
- `Skills/ATSValidation/SKILL.md` - ATS compatibility validation
- `Skills/Guardrails/SKILL.md` - Fabrication detection and validation
- `Skills/DocumentGeneration/SKILL.md` - DOCX generation

## 6. Data Flow

```text
Upload
 ↓
Validation (file type, size, MIME)
 ↓
Temporary Storage (session-specific directory)
 ↓
Text Extraction (DocumentParserService - PDF/DOCX/MD/TXT)
 ↓
Resume/JD Intelligence (AI analysis via skills)
 ↓
Matching (skill matching with category scores)
 ↓
Tailoring (resume content rewording/reordering)
 ↓
ATS (ATS compatibility validation)
 ↓
Guardrail (fabrication check)
 ↓
Approval (human approval)
 ↓
DOCX (document generation)
 ↓
Cleanup (temp file removal)
```

## 7. Important Domain Models

```text
ResumeModel
- Parses and stores raw resume data (file name, type, extracted text)
- Not an analysis model - pure data container from file parsing

JobDescriptionModel
- Parses and stores raw JD data (file name, type, extracted text, target role)

SkillMatchResultModel
- Overall match score (0-100)
- Category scores breakdown (technical, experience, architecture, leadership, domain, ATS keywords)
- Individual skill matches with JD skill, resume skill, match type, confidence
- Matched skills list, partial matches, missing skills
- Experience match (required vs candidate years)

TailoringResultModel
- Tailored resume content (professional summary, core competencies, technical skills, experience, projects, education, certifications)
- Changes array documenting every modification
- Keywords integrated from JD

ATSAnalysisModel
- ATS score (0-100)
- Keyword coverage percentage
- Critical missing keywords
- Potential keyword stuffing alerts
- Formatting risks
- Section structure validation
- Job title alignment
- Skills alignment counts and coverage

GuardrailResultModel
- Status: PASS or FAIL
- Claim validations per section with supported/unsupported status
- Unsupported claims with reasons and suggested corrections
- Fabrication detection flags (experience inflation, metric invention)

ResumeTailorSession
- Session identifier and lifecycle tracking
- Resume text and JD text storage
- Analysis results (analysisResult, skillMatchResult, tailoringResult, atsResult, guardrailResult)
- Approval status (Pending, Approved, Rejected)
- Generated document paths
- IsLocked flag (prevents re-analysis)

AnalysisProgress
- Step-by-step progress tracking (ResumeParsed, JdParsed, SkillsExtracted, SkillsMatched, TailoringCompleted, AtsCompleted, GuardrailCompleted)
- AnalysisComplete computed property (GuardrailCompleted)
```

## 8. API Development Rules

**Request validation:** All API endpoints validate required parameters (session existence, file validity, session state). Unauthorized access without active session returns 401. Missing files or invalid formats return 400. Business rule violations (session locked, missing resume/JD) return 400 or 404.

**Response format:** All API responses use the `ApiResponse<T>` generic pattern with `Success`, `Message`, `Data`, and optional `Errors` fields. HTTP status codes reflect the business outcome (200 for success, 400 for validation errors, 401 for no session, 404 for not found, 500 for unexpected errors).

**HTTP status codes:**
- 200 - OK, operation successful
- 400 - Bad request, validation or business rule failure
- 401 - Unauthorized, no active session
- 404 - Not found, session or resource not found
- 500 - Internal server error, unexpected failure

**Error handling:** Exceptions are logged and returned in error responses. Never expose sensitive information (resume content, JD content, API keys) in error messages or responses.

**Session validation:** Every API endpoint that requires an active session checks for the session ID in the HTTP session cookie. The session ID is set when `/api/session/start` is called and retrieved via `HttpContext.Session.GetString("SessionId")`.

**CancellationToken:** All async methods accept CancellationToken for operation cancellation. Timeout configurations are applied at the controller level (e.g., 2 minutes for file upload, 5 minutes for analysis).

**No sensitive information in responses:** Resume content, JD content, phone numbers, emails, AI prompts containing personal data, and generated resume content should never appear in API responses. Only DTO models should be returned, and even those should exclude raw text content.

## 9. File Handling Rules

**Supported file types:** PDF, DOCX, MD, TXT

**File size limits:** Maximum 10MB for both resumes and job descriptions (configured in appsettings.json)

**Temporary storage:** Files stored in `App_Data/TempSessions/{sessionId}/{unique_filename}`. Each session gets its own isolated directory.

**Safe file names:** The FileService.GetSafeFileName method strips invalid path characters and ensures safe file names. File names include timestamp prefixes to avoid collisions.

**Path traversal protection:** File paths are constructed using Path.Combine and validated. No user input directly determines file system paths.

**MIME validation:** File MIME types are validated against allowed types per extension. The FileService.ValidateFileAsync method checks extension, size, and MIME type.

**Cleanup:** 
- SessionService.CleanupSessionAsync removes resume file, JD file, generated resume path, generated cover letter path, and the session temp directory
- CleanupService background service runs every 10 minutes to clean up expired sessions (30-minute timeout)
- Orphaned directories in the temp path are cleaned up if no corresponding session exists

**Never store uploaded files in wwwroot:** Uploaded files must be stored in session-specific temporary directories, not in the static wwwroot folder.

## 10. Security Rules

**Never log:**
- Resume content
- JD content
- Phone number
- Email
- AI prompts containing personal data
- Generated resume content
- API keys

**Never commit:**
- API keys (in appsettings.json or any code file)
- Secrets (connection strings, passwords)
- Personal documents (actual resumes or JDs in source control)
- Generated resumes (DOCX files in source control)
- Temporary uploads (temp directory contents in source control)

**Logging guidance:** Log only non-sensitive information: file names, session IDs, operation timestamps, success/failure status. The FileService logs file paths but not content. The AIService logs prompt outlines (not full content).

## 11. Running the Project

```bash
dotnet restore
dotnet build
dotnet run
```

**Note:** No test projects exist in the current repository. Testing has been verified through manual API endpoint validation and the TODO.md tracking criteria.

## 12. Adding a New Feature

Simple development workflow:

```text
1. Understand requirement - Review existing implementation and similar features
2. Inspect existing implementation - Check relevant controllers, services, models, and skills
3. Identify affected layer - Determine which layer needs updating (API, service, model, skill, UI)
4. Update model/service/skill/API/UI as required - Make minimal changes to implement the feature
5. Build - Run dotnet build to verify compilation
6. Test - Run dotnet test to verify no regressions
7. Validate end-to-end - Test the complete flow from upload to document generation
8. Update documentation - Update README.md and DEVELOPER.md if needed
```

**Do not create new abstractions unless necessary:** Reuse existing services, models, and patterns before introducing new layers of abstraction.

## 13. AI Coding Agent Rules

**Before changing code:**
- Inspect existing implementation
- Do not assume files/classes exist
- Do not duplicate existing services
- Do not change architecture without reason
- Do not introduce unnecessary dependencies
- Reuse existing services
- Keep changes minimal
- Build after changes
- Run tests
- Update documentation

**When requirements are unclear:**
- Prefer the simplest implementation
- Do not invent business requirements
- Document assumptions
- Consult existing similar implementations in the codebase
- If uncertain, mark functionality as "Planned" rather than claiming it as implemented

## 14. Definition of Done

A feature is complete only when:

```text
Code implemented
Build passes
Tests pass
Security considered
AI guardrails considered
UI works
API works
Documentation updated
No unnecessary dependency introduced
```

All four aspects must be satisfied:
1. **Code implemented** - The feature has been coded following project patterns
2. **Build passes** - dotnet build completes with no errors
3. **Tests pass** - dotnet test runs successfully
4. **Security considered** - No sensitive data exposure, proper validation
5. **AI guardrails considered** - Guardrail validation integrated, no fabrication possible
6. **UI works** - Frontend interacts correctly with the new API endpoints
7. **API works** - Endpoints return expected responses with correct status codes
8. **Documentation updated** - README.md and DEVELOPER.md reflect the changes
9. **No unnecessary dependency introduced** - No new NuGet packages or abstractions added without reason