---
name: ats-validation
description: Validate tailored resume for ATS compatibility. Checks formatting, keyword coverage, keyword stuffing, section structure, and job title alignment. Use when asked to validate ATS compatibility.
---

# ATS Validation Skill

## Purpose
Validate tailored resume for ATS compatibility.

## Inputs
- Tailored resume content (JSON)
- JD Intelligence output (JSON)
- Original resume text

## Rules
1. Check ATS readability (simple formatting, no tables, no images, no complex layouts)
2. Validate keyword coverage against JD requirements
3. Check for keyword stuffing (excessive repetition)
4. Verify section structure (standard headings)
5. Check job title alignment
6. Validate skills alignment
7. Check experience alignment
8. Identify critical missing keywords
9. Flag unsupported claims
10. Check for formatting risks (headers/footers, unusual symbols)

## Output Format
```json
{
  "atsScore": "number (0-100)",
  "keywordCoverage": "number (0-100)",
  "criticalMissingKeywords": ["string"],
  "potentialKeywordStuffing": [
    {
      "keyword": "string",
      "count": "number",
      "threshold": "number",
      "severity": "Low|Medium|High"
    }
  ],
  "formattingRisks": [
    {
      "issue": "string",
      "severity": "Low|Medium|High",
      "location": "string",
      "recommendation": "string"
    }
  ],
  "sectionStructure": {
    "hasContactInfo": "boolean",
    "hasProfessionalSummary": "boolean",
    "hasExperience": "boolean",
    "hasSkills": "boolean",
    "hasEducation": "boolean",
    "hasCertifications": "boolean",
    "issues": ["string"]
  },
  "jobTitleAlignment": {
    "score": "number (0-100)",
    "targetTitle": "string",
    "candidateTitles": ["string"],
    "recommendation": "string"
  },
  "skillsAlignment": {
    "matched": "number",
    "missing": "number",
    "coverage": "number (0-100)"
  },
  "recommendations": ["string"],
  "isAtsFriendly": "boolean"
}
```

## Validation Rules
- atsScore between 0-100
- keywordCoverage between 0-100
- All arrays can be empty but not null
- isAtsFriendly must be boolean

## Failure Conditions
- Input JSON invalid
- Resume content empty
- JSON parsing fails