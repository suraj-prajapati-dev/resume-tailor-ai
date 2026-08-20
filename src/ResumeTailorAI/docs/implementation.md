# ResumeTailor-AI Implementation Plan

## Overview
Implement ResumeTailor-AI using **Microsoft Agent Framework Harness** as the agent runtime. The Harness provides built-in: function invocation, history persistence, compaction, TodoProvider, AgentModeProvider, FileMemoryProvider, FileAccessProvider, AgentSkillsProvider, BackgroundAgentsProvider, WebSearch, ToolApproval, OpenTelemetry, and LoopEvaluators.

## Architecture

### Agent Design
- **Single AIAgent** (HarnessAgent) per session
- **HarnessAgent** wraps `IChatClient` with `AsHarnessAgent()`
- **Skills** as `.md` files in `skills/` folder (discovered by AgentSkillsProvider)
- **Session state** managed by Harness (per-service-call history, file memory)
- **No custom orchestration** - Harness handles the agent loop

### Data Flow
User Request
    ?
HTTP API ? SessionController/ResumeController/JDController
    ?
Create/Retrieve Session (SessionService)
    ?
Upload Files ? FileService ? DocumentParserService
    ?
Start Analysis ? HarnessAgent.RunAsync() with session context
    ?
Harness internally:
  - TodoProvider: Creates plan (ResumeIntelligence ? JDIntelligence ? SkillMatching ? ResumeTailoring ? ATSValidation ? Guardrails)
  - AgentModeProvider: Plan mode ? Execute mode
  - AgentSkillsProvider: Loads skills from skills/ folder
  - FileMemoryProvider: Persists intermediate results
  - FileAccessProvider: Reads/writes temp files
  - LoopEvaluator: Re-invokes until all todos complete
    ?
Returns final result (AnalysisResultResponse)
    ?
Human Approval (TailoringController)
    ?
Document Generation (DocumentGenerationService)
    ?
Download (DocumentsController)

## Implementation Phases

### Phase 1: Foundation & Configuration (Week 1)
- [ ] Fix compilation errors (DocumentGenerationService, AIService)
- [ ] Update `Program.cs` to register HarnessAgent correctly
- [ ] Configure `HarnessAgentOptions` with all providers enabled
- [ ] Set up AI chat client (OpenAI/Azure OpenAI)
- [ ] Configure file memory store path
- [ ] Add `Microsoft.Agents.AI` package (for Harness)

### Phase 2: Skills Implementation (Week 1-2)
Create skill files in `skills/` folder (each with SKILL.md + optional scripts):
- [ ] `resume-intelligence/SKILL.md` - Parse & extract structured resume data
- [ ] `jd-intelligence/SKILL.md` - Analyze job description
- [ ] `skill-matching/SKILL.md` - Compare resume vs JD skills
- [ ] `resume-tailoring/SKILL.md` - Generate tailored resume content
- [ ] `ats-validation/SKILL.md` - Validate ATS compatibility
- [ ] `guardrails/SKILL.md` - Validate no fabrication
- [ ] `document-generation/SKILL.md` - Generate DOCX files

### Phase 3: Harness Agent Integration (Week 2)
- [ ] Replace custom `AgentOrchestrator`/`ResumeTailorHarness` with `AIAgent.AsHarnessAgent()`
- [ ] Configure `HarnessAgentOptions`:
  - `TodoProvider` - enabled
  - `AgentModeProvider` - enabled (plan/execute modes)
  - `FileMemoryProvider` - enabled (session-scoped)
  - `FileAccessProvider` - enabled (temp directory)
  - `AgentSkillsProvider` - enabled (skills folder)
  - `WebSearch` - disabled (not needed)
  - `ToolApproval` - enabled
  - `OpenTelemetry` - enabled
  - `LoopEvaluators` - `TodoCompletionLoopEvaluator` for execute mode
- [ ] Create session-scoped agent factory
- [ ] Implement `IChatClient` wrapper for AI calls

### Phase 4: API Controllers (Week 2-3)
- [ ] `SessionController` - Start/end sessions, session status
- [ ] `ResumeController` - Upload resume, parse with DocumentParserService
- [ ] `JDController` - Upload JD, parse with DocumentParserService
- [ ] `AnalysisController` - Start analysis, get status/results
- [ ] `TailoringController` - Preview, approve/reject, regenerate
- [ ] `DocumentsController` - Download generated DOCX files

### Phase 5: Document Processing (Week 3)
- [ ] Fix `DocumentParserService` (PDF, DOCX, MD, TXT)
- [ ] Fix `DocumentGenerationService` (OpenXML SDK 3.x compatibility)
- [ ] Add temp file cleanup (SessionService + CleanupService)

### Phase 6: UI (Week 3-4)
- [ ] Home page: Upload resume/JD, target role, analyze
- [ ] Analysis page: Progress with todo list display
- [ ] Result page: Match score, skills breakdown, ATS analysis, guardrail status
- [ ] Approval page: Preview tailored resume, approve/reject
- [ ] Download page: Download Resume.docx, CoverLetter.docx

### Phase 7: Security & Testing (Week 4)
- [ ] File validation (type, size, MIME, path traversal)
- [ ] Anti-forgery tokens
- [ ] Session isolation
- [ ] No sensitive data in logs
- [ ] Unit tests for: file validation, skill matching, guardrails, session cleanup
- [ ] End-to-end integration test

## Key Technical Decisions

### 1. HarnessAgent per Session
Each user session gets its own `AIAgent` instance with isolated:
- Chat history (per-service-call persistence)
- Todo list (plan/execute modes)
- File memory (session-scoped)
- File access (temp directory)
- Skills (same skills folder, but isolated context)

### 2. Skill Structure
Each skill in `skills/{skill-name}/`:
```
skills/
+-- resume-intelligence/
¦   +-- SKILL.md
¦   +-- (optional reference files)
+-- jd-intelligence/
¦   +-- SKILL.md
+-- skill-matching/
¦   +-- SKILL.md
+-- resume-tailoring/
¦   +-- SKILL.md
+-- ats-validation/
¦   +-- SKILL.md
+-- guardrails/
¦   +-- SKILL.md
+-- document-generation/
    +-- SKILL.md
```

### 3. Session State Management
- SessionService: In-memory dictionary with background cleanup
- Temp files in `App_Data/TempSessions/{sessionId}/`
- Cleanup: 30-min timeout, 10-min interval

### 4. Human Approval Flow
1. Analysis completes ? Guardrail PASS
2. UI shows preview + "Approve & Generate" / "Reject"
3. On approve ? DocumentGenerationService creates DOCX
4. On reject ? Return to upload/edit

### 5. No Fabrication Enforcement
- Guardrails skill validates every claim against original resume
- If FAIL ? Analysis stops, UI shows unsupported claims
- Document generation only proceeds after Guardrail PASS + Human Approval

## File Structure After Implementation
```
ResumeTailor-AI/
+-- src/ResumeTailorAI/
¦   +-- Agents/
¦   ¦   +-- HarnessAgentFactory.cs      # Creates HarnessAgent per session
¦   +-- Configuration/
¦   ¦   +-- AppConfiguration.cs
¦   ¦   +-- HarnessOptions.cs
¦   +-- Controllers/
¦   ¦   +-- SessionController.cs
¦   ¦   +-- ResumeController.cs
¦   ¦   +-- JDController.cs
¦   ¦   +-- AnalysisController.cs
¦   ¦   +-- TailoringController.cs
¦   ¦   +-- DocumentsController.cs
¦   +-- Models/                         # (existing - keep)
¦   +-- Services/
¦   ¦   +-- FileService.cs
¦   ¦   +-- DocumentParserService.cs
¦   ¦   +-- DocumentGenerationService.cs
¦   ¦   +-- SessionService.cs
¦   ¦   +-- CleanupService.cs
¦   +-- Skills/                         # Skill files (read by Harness)
¦   ¦   +-- resume-intelligence/SKILL.md
¦   ¦   +-- jd-intelligence/SKILL.md
¦   ¦   +-- skill-matching/SKILL.md
¦   ¦   +-- resume-tailoring/SKILL.md
¦   ¦   +-- ats-validation/SKILL.md
¦   ¦   +-- guardrails/SKILL.md
¦   ¦   +-- document-generation/SKILL.md
¦   +-- Views/                          # (existing - keep)
¦   +-- wwwroot/                        # (existing - keep)
¦   +-- Program.cs                      # Register HarnessAgent, services
¦   +-- appsettings.json
¦   +-- ResumeTailorAI.csproj
+-- tests/ResumeTailorAI.Tests/
+-- docs/
+-- ResumeTailor-AI.sln
```

## NuGet Packages to Add
- `Microsoft.Agents.AI` (for HarnessAgent, providers)
- `Microsoft.Extensions.AI` (for IChatClient)
- `OpenAI` or `Azure.AI.OpenAI` (for chat client)

## Validation Criteria
1. ? Upload Resume (PDF/DOCX/MD/TXT)
2. ? Upload JD (PDF/DOCX/MD/TXT)
3. ? Enter Target Role
4. ? Analysis runs through all 6 skills
5. ? Todo list visible during analysis
6. ? Match score + skill breakdown displayed
7. ? ATS analysis shown
8. ? Guardrail validation (PASS/FAIL with details)
9. ? Human approval required before generation
10. ? Resume.docx + CoverLetter.docx generated
11. ? Files downloadable
12. ? Temp files cleaned up after session ends
13. ? No sensitive data in logs

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Harness API changes | Pin package versions, test after updates |
| Skill loading issues | Test each skill independently, use clear SKILL.md format |
| Context window overflow | Enable compaction, set MaxContextWindowTokens |
| File cleanup failures | Idempotent cleanup, background service + on-session-end |
| AI response parsing | Strict JSON schemas in SKILL.md, validate output |

## Open Questions
1. **AI Provider**: OpenAI direct or Azure OpenAI? (affects chat client setup)
2. **Session persistence**: In-memory only, or need Redis for scale?
3. **Concurrent analyses**: Single agent per session, or shared with isolation?
4. **Web search needed?**: Probably not for resume tailoring - disable to save costs
5. **Background agents**: Useful for parallel skill execution? (e.g., ResumeIntelligence + JDIntelligence in parallel)

## Next Steps
1. **Fix compilation errors** - DocumentGenerationService, AIService
2. **Add Microsoft.Agents.AI package** - For HarnessAgent
3. **Create HarnessAgentFactory** - Session-scoped agent creation
4. **Refactor skills** - Ensure SKILL.md format works with AgentSkillsProvider
5. **Replace custom orchestration** - Use Harness built-in loop
