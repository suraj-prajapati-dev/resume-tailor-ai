---
name: resume-intelligence
description: Extract structured candidate information from resume text. Parse and extract contact info, skills, experience, projects, education, and certifications. Use when asked to analyze a resume.
---

# Resume Intelligence Skill

## Purpose
Extract structured candidate information from resume text.

## Inputs
- Raw resume text (string)
- File name (string)
- File type (string)

## Rules
1. Extract ONLY information explicitly stated in the resume
2. Never infer or assume experience not directly stated
3. Every skill must have an evidence reference from the original text
4. Preserve exact employment dates, employer names, job titles
5. Distinguish between "worked with", "expert in", "knowledge of"
6. Extract measurable achievements with metrics when present
7. Identify certifications with issuing authority and date

## Output Format
```json
{
  "candidate": {
    "name": "string",
    "email": "string",
    "phone": "string",
    "location": "string",
    "linkedin": "string",
    "github": "string",
    "portfolio": "string"
  },
  "professionalSummary": "string",
  "totalExperienceYears": "number",
  "currentRole": "string",
  "currentEmployer": "string",
  "employmentHistory": [
    {
      "employer": "string",
      "title": "string",
      "startDate": "string",
      "endDate": "string",
      "isCurrent": "boolean",
      "responsibilities": ["string"],
      "achievements": ["string"],
      "technologies": ["string"]
    }
  ],
  "skills": [
    {
      "name": "string",
      "category": "Technical|Soft|Domain|Tool|Framework|Language|Database|Cloud|Architecture",
      "proficiency": "Expert|Advanced|Intermediate|Beginner|Exposure",
      "yearsOfExperience": "number|null",
      "evidence": "string",
      "confidence": "number (0-1)"
    }
  ],
  "projects": [
    {
      "name": "string",
      "description": "string",
      "technologies": ["string"],
      "role": "string",
      "achievements": ["string"]
    }
  ],
  "education": [
    {
      "degree": "string",
      "field": "string",
      "institution": "string",
      "graduationYear": "string",
      "honors": "string"
    }
  ],
  "certifications": [
    {
      "name": "string",
      "issuer": "string",
      "issueDate": "string",
      "expiryDate": "string|null",
      "credentialId": "string"
    }
  ],
  "keywords": ["string"]
}
```

## Validation Rules
- All string fields must be non-null (use empty string if not found)
- Confidence scores must be between 0 and 1
- Employment dates must be parseable
- Skills must have non-empty evidence

## Failure Conditions
- Resume text is empty or too short (< 100 characters)
- Unable to parse candidate name
- JSON parsing fails