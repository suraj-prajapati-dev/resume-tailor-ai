---
name: resume-tailoring
description: Generate tailored resume content based on analysis results. Rewords, reorders, and highlights existing experience to align with job description keywords. Never fabricates information. Use when asked to tailor a resume.
---

# Resume Tailoring Skill

## Purpose
Generate tailored resume content based on analysis results.

## Inputs
- Original resume text
- Resume Intelligence output
- JD Intelligence output
- Skill Matching output
- Target role

## Rules
1. ONLY reword, reorder, highlight existing experience
2. NEVER invent skills, experience, projects, metrics, employers, certifications
3. Use JD terminology ONLY when accurately representing existing experience
4. Prioritize JD's important keywords when supported by resume evidence
5. Optimize: professional summary, core competencies, technical skills, experience bullets, project descriptions, achievement wording, keyword placement, section ordering
6. Every change must be traceable to original resume

## Output Format
```json
{
  "tailoredResume": {
    "professionalSummary": "string",
    "coreCompetencies": ["string"],
    "technicalSkills": [
      {
        "category": "string",
        "skills": ["string"],
        "priority": "number"
      }
    ],
    "experience": [
      {
        "employer": "string",
        "title": "string",
        "startDate": "string",
        "endDate": "string",
        "isCurrent": "boolean",
        "bullets": [
          {
            "original": "string",
            "tailored": "string",
            "evidence": "string",
            "keywordsAdded": ["string"]
          }
        ]
      }
    ],
    "projects": [
      {
        "name": "string",
        "description": "string",
        "technologies": ["string"],
        "highlights": ["string"]
      }
    ],
    "education": [
      {
        "degree": "string",
        "field": "string",
        "institution": "string",
        "graduationYear": "string"
      }
    ],
    "certifications": [
      {
        "name": "string",
        "issuer": "string",
        "issueDate": "string"
      }
    ]
  },
  "changes": [
    {
      "section": "string",
      "changeType": "Reworded|Reordered|Highlighted|KeywordAdded|Combined",
      "original": "string",
      "modified": "string",
      "reason": "string",
      "evidence": "string"
    }
  ],
  "keywordsIntegrated": ["string"]
}
```

## Validation Rules
- All original resume data must be preserved (no deletion of real experience)
- Every tailored bullet must have evidence reference
- keywordsIntegrated must only include terms from JD that have resume evidence
- changes array must document every modification

## Failure Conditions
- Guardrail validation would fail (unsupported claims detected)
- Unable to generate valid JSON
- Original resume data corrupted