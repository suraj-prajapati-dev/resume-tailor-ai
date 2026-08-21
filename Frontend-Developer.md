# Frontend Developer Guide

This guide helps frontend developers (React, Angular, Vue, or plain JavaScript) integrate with the ResumeTailor-AI backend. All API endpoints are RESTful and follow consistent patterns.

## 1. Base URL

```text
Development: http://localhost:5065 (or whatever port Kestrel uses)
Production: Your deployed URL
```

## 2. Session Management

**All API calls require a session ID** (except session start). The session ID is obtained by creating a new session and stored in an HTTP-only cookie or client-side state.

### 2.1 Create Session

```http
POST /api/session/start
Content-Type: application/json

{
  "username": "your-name",
  "password": "your-password", 
  "targetRole": "Software Engineer"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Session created successfully",
  "data": {
    "sessionId": "abc123-def456-789ghi"
  }
}
```

**Set cookie:** The response includes a session cookie. Store it for subsequent requests.

### 2.2 Check Session Status

```http
GET /api/session/status
```

**Response:**
```json
{
  "success": true,
  "message": "Session active",
  "data": {
    "sessionId": "abc123-def456-789ghi",
    "targetRole": "Software Engineer",
    "hasResume": true,
    "hasJd": false,
    "analysisComplete": false
  }
}
```

## 3. File Upload Endpoints

### 3.1 Upload Resume

```http
POST /api/resume/upload
Content-Type: multipart/form-data
Authorization: Bearer {sessionId} (or include session cookie)
```

**Parameters:**
- `file`: The resume file (PDF, DOCX, MD, or TXT, max 10MB)

**Response:**
```json
{
  "success": true,
  "message": "Resume uploaded successfully",
  "data": {
    "fileName": "Resume.pdf",
    "fileType": ".pdf",
    "size": 1024500,
    "message": "File processed successfully"
  }
}
```

### 3.2 Upload Job Description

```http
POST /api/jd/upload
Content-Type: multipart/form-data
Authorization: Bearer {sessionId} (or include session cookie)
```

**Parameters:**
- `file`: The job description file (PDF, DOCX, MD, or TXT, max 10MB)

**Response:**
```json
{
  "success": true,
  "message": "Job Description uploaded successfully",
  "data": {
    "fileName": "JD.pdf",
    "fileType": ".pdf",
    "size": 2048500,
    "message": "File processed successfully"
  }
}
```

### 3.3 Required Form Order

1. Upload Resume first
2. Upload Job Description second
3. Enter target role (or use existing session target role)
4. Click "Analyze Resume" to start analysis

## 4. Analysis Endpoints

### 4.1 Start Analysis

```http
POST /api/analysis/start?targetRole=Software Engineer
```

**Query Parameters:**
- `targetRole`: Optional. Overwrites session target role if provided.

**Response:**
```json
{
  "success": true,
  "message": "Analysis completed successfully",
  "data": {
    "isComplete": true,
    "progressPercentage": 100,
    "completedSteps": [
      "Resume Intelligence",
      "JD Intelligence",
      "Skill Matching",
      "Resume Tailoring",
      "ATS Validation",
      "Guardrail Validation"
    ]
  }
}
```

### 4.2 Get Analysis Status

```http
GET /api/analysis/status
```

**Response:**
```json
{
  "success": true,
  "message": "Status retrieved",
  "data": {
    "isComplete": true,
    "progressPercentage": 100,
    "completedSteps": [],
    "currentStep": "Complete"
  }
}
```

### 4.3 Get Analysis Results

```http
GET /api/analysis/result
```

**Response - Key fields:**
```json
{
  "success": true,
  "message": "Analysis results retrieved",
  "data": {
    "targetRole": "Software Engineer",
    "overallMatchScore": 85.5,
    "resumeSummary": "Senior software engineer with 5 years...",
    "jdSummary": "Target Role: Software Engineer. Required skills: 8...",
    "matchedSkills": ["C#", ".NET", "SQL", "Azure"],
    "partialMatches": [
      {
        "skill": "Kubernetes",
        "gap": "Partial match - candidate has related experience",
        "recommendation": "Highlight transferable skills"
      }
    ],
    "missingSkills": [
      {
        "skill": "TypeScript",
        "priority": "Must Have",
        "action": "Do not add to resume"
      }
    ],
    "experienceMatch": {
      "requiredYears": 5,
      "candidateYears": 5,
      "match": "Meets",
      "details": "Candidate has 5 years of experience, meeting the minimum of 5 years"
    },
    "atsAnalysis": {
      "atsScore": 92,
      "keywordCoverage": 88.5,
      "isAtsFriendly": true,
      "recommendations": ["Good keyword coverage"]
    },
    "tailoredResume": {
      "professionalSummary": "Tailored summary...",
      "coreCompetencies": ["C#", ".NET", "Web API"],
      "technicalSkills": [...],
      "experience": [...]
    },
    "guardrail": {
      "status": "PASS",
      "summary": "All claims validated successfully"
    },
    "requiresHumanApproval": true
  }
}
```

## 5. Tailoring / Approval Endpoints

### 5.1 Generate Preview

```http
POST /api/tailoring/generate-preview
Content-Type: application/json

{
  "tailoringResult": { ... },
  "guardrailResult": { ... }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Preview generated",
  "data": {
    "tailoredResume": { ... }
  }
}
```

### 5.2 Approve/Reject

```http
POST /api/tailoring/approve
Content-Type: application/json

{
  "approved": true (or false)
}
```

**If approved:**
```json
{
  "success": true,
  "message": "Documents generated successfully",
  "data": {
    "resumeDownloadUrl": "/api/documents/resume",
    "coverLetterDownloadUrl": "/api/documents/cover-letter"
  }
}
```

**If rejected:**
```json
{
  "success": true,
  "message": "Approval rejected"
}
```

## 6. Document Download Endpoints

### 6.1 Download Tailored Resume

```http
GET /api/documents/resume
```

**Response:** `application/vnd.openxmlformats-officedocument.wordprocessingml.document` file named "TailoredResume.docx"

**Conditions:** Only available if:
- Session approval status is "Approved"
- Generated resume path exists
- Guardrail validation passed

### 6.2 Download Cover Letter

```http
GET /api/documents/cover-letter
```

**Response:** `application/vnd.openxmlformats-officedocument.wordprocessingml.document` file named "CoverLetter.docx"

**Conditions:** Same as resume download.

## 7. Session Logout

```http
POST /api/session/logout
```

**Response:**
```json
{
  "success": true,
  "message": "Session ended successfully"
}
```

## 8. Error Handling Patterns

### 8.1 Standard Error Response

```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Optional error details"]
}
```

### 8.2 HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 400 | Bad request - validation error, missing files, business rule violation |
| 401 | Unauthorized - no active session |
| 404 | Not found - session or resource not found |
| 500 | Internal server error |

### 8.3 Common Error Scenarios

- **No active session:** `401 {"message": "No active session"}`
- **Session expired:** `404 {"message": "Session expired"}`
- **Session locked** (analysis in progress): `400 {"message": "Session is locked. Analysis already started."}`
- **Resume not uploaded:** `400 {"message": "Resume not uploaded"}`
- **JD not uploaded:** `400 {"message": "Job description not uploaded"}`
- **Invalid file type:** `400 {"message": "Invalid file", "errors": ["File type not supported"]}`
- **File too large:** `400 {"message": "File size exceeds 10MB limit"}`

## 9. Framework-Specific Examples

### 9.1 React + TypeScript (fetch API)

```typescript
import { useState, useEffect } from 'react';

interface Session {
  sessionId: string;
  targetRole: string;
  hasResume: boolean;
  hasJD: boolean;
  analysisComplete: boolean;
}

export function useResumeTailor() {
  const [session, setSession] = useState<Session | null>(null);
  const [status, setStatus] = useState<'idle' | 'uploading' | 'analyzing' | 'complete'>('idle');

  const startSession = async (username: string, password: string, targetRole: string) => {
    const response = await fetch('/api/session/start', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password, targetRole })
    });
    const data = await response.json();
    if (data.success && data.data.sessionId) {
      setSession({ sessionId: data.data.sessionId, targetRole, hasResume: false, hasJD: false, analysisComplete: false });
    }
    return data;
  };

  const uploadResume = async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await fetch('/api/resume/upload', {
      method: 'POST',
      body: formData
    });
    return response.json();
  };

  const uploadJd = async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await fetch('/api/jd/upload', {
      method: 'POST',
      body: formData
    });
    return response.json();
  };

  const startAnalysis = async () => {
    const response = await fetch('/api/analysis/start?targetRole=' + session?.targetRole, {
      method: 'POST'
    });
    return response.json();
  };

  const getResults = async () => {
    const response = await fetch('/api/analysis/result');
    return response.json();
  };

  return { session, status, startSession, uploadResume, uploadJd, startAnalysis, getResults };
}
```

### 9.2 Angular

```typescript
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ResumeTailorService {
  private baseUrl = 'http://localhost:5065/api';

  constructor(private http: HttpClient) {}

  startSession(username: string, password: string, targetRole: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/session/start`, { username, password, targetRole });
  }

  uploadResume(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/resume/upload`, formData);
  }

  uploadJd(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/jd/upload`, formData);
  }

  startAnalysis(targetRole: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/analysis/start?targetRole=${targetRole}`, {});
  }

  getResults(): Observable<any> {
    return this.http.get(`${this.baseUrl}/analysis/result`);
  }

  approveTailoring(approved: boolean): Observable<any> {
    return this.http.post(`${this.baseUrl}/tailoring/approve`, { approved });
  }

  downloadResume(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/documents/resume`, { responseType: 'blob' });
  }

  downloadCoverLetter(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/documents/cover-letter`, { responseType: 'blob' });
  }
}
```

### 9.3 Vue 3 (axios)

```javascript
import { ref, computed } from 'vue';
import axios from 'axios';

const baseUrl = 'http://localhost:5065/api';

const session = ref(null);
const status = ref('idle');
const results = ref(null);

export function useResumeTailor() {
  const startSession = async (username, password, targetRole) => {
    const response = await axios.post(`${baseUrl}/session/start`, { username, password, targetRole });
    if (response.data.success && response.data.data.sessionId) {
      session.value = {
        sessionId: response.data.data.sessionId,
        targetRole,
        hasResume: false,
        hasJD: false,
        analysisComplete: false
      };
    }
    return response.data;
  };

  const uploadFile = async (endpoint, file) => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await axios.post(`${baseUrl}/${endpoint}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return response.data;
  };

  const startAnalysis = async (targetRole) => {
    const response = await axios.post(`${baseUrl}/analysis/start?targetRole=${targetRole}`);
    return response.data;
  };

  const getResults = async () => {
    const response = await axios.get(`${baseUrl}/analysis/result`);
    results.value = response.data.data;
    return response.data;
  };

  const approve = async (approved) => {
    const response = await axios.post(`${baseUrl}/tailoring/approve`, { approved });
    return response.data;
  };

  const download = async (endpoint) => {
    const response = await axios.get(`${baseUrl}/${endpoint}`, { responseType: 'blob' });
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const a = document.createElement('a');
    a.href = url;
    a.download = endpoint === 'resume' ? 'TailoredResume.docx' : 'CoverLetter.docx';
    a.click();
    window.URL.revokeObjectURL(url);
  };

  return {
    session, status, results,
    startSession, uploadFile, startAnalysis, getResults, approve, download
  };
}
```

## 10. Dependency Checklist

**For any frontend framework, you need:**

1. **HTTP client library:**
   - `fetch` (built into modern browsers)
   - `axios` (popular third-party)
   - `HttpClient` (Angular built-in)
   - `vue-axios` or `fetch` (Vue)

2. **FormData support** (for file uploads):
   - All modern browsers support FormData natively
   - For older browsers, use `formdata` polyfill

3. **Blob support** (for document downloads):
   - `window.URL.createObjectURL()` 
   - `navigator.msSaveBlob()` for IE (if needed)

4. **Session management:**
   - HTTP-only cookies (recommended)
   - Or store sessionId in localStorage/redux/vuex
   - Include sessionId in `Authorization` header or as query parameter

5. **Error handling:**
   - Check `response.success` before using `response.data`
   - Check HTTP status codes
   - Display user-friendly error messages

## 11. UI Workflow Checklist

**Minimum viable frontend:**

```text
1. Session creation UI (username, password, target role input)
2. Resume upload zone (accept: .pdf, .docx, .md, .txt, max 10MB)
3. Job Description upload zone (same format/limits)
4. Analysis start button (disabled until both files uploaded)
5. Progress display (6 steps: Resume Intelligence, JD Intelligence, Skill Matching, Resume Tailoring, ATS Validation, Guardrail Validation)
6. Results display area (match score, matched/missing/partial skills, ATS score, guardrail PASS/FAIL)
7. Approval toggle (Approve/Reject buttons)
8. Download buttons for Resume.docx and CoverLetter.docx (disabled until approved)
9. Session logout link/button
```

## 12. API Reference Summary Table

| Endpoint | Method | Auth | Body | Response | Description |
|----------|--------|------|------|----------|-------------|
| `/api/session/start` | POST | None | `{username, password, targetRole}` | `{sessionId}` | Create new session |
| `/api/session/status` | GET | Session cookie | None | Session status | Check session state |
| `/api/resume/upload` | POST | Session | `multipart/form-data` (file) | `{fileName, fileType, size}` | Upload resume |
| `/api/jd/upload` | POST | Session | `multipart/form-data` (file) | `{fileName, fileType, size}` | Upload JD |
| `/api/analysis/start` | POST | Session | `?targetRole?` | `{isComplete, progressPercentage, completedSteps}` | Start analysis pipeline |
| `/api/analysis/status` | GET | Session cookie | None | Analysis status | Get real-time status |
| `/api/analysis/result` | GET | Session cookie | None | Full analysis results | Get complete results |
| `/api/tailoring/approve` | POST | Session | `{approved: bool}` | `{resumeDownloadUrl, coverLetterDownloadUrl}` | Human approval |
| `/api/documents/resume` | GET | Session cookie | None | DOCX file | Download tailored resume |
| `/api/documents/cover-letter` | GET | Session cookie | None | DOCX file | Download cover letter |
| `/api/session/logout` | POST | Session cookie | None | `{message}` | End session |

---