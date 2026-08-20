---
name: skill-matching
description: Compare resume skills against job description requirements using semantic matching. Calculates overall fit score with category breakdown. Use when asked to match candidate skills to job requirements.
---

# Skill Matching Skill

## Purpose
Compare resume skills against job description requirements.

## Inputs
- Resume Intelligence output (JSON)
- JD Intelligence output (JSON)

## Rules
1. For each JD requirement, find best match in resume skills
2. Use semantic matching, not just exact string matching
3. Consider evidence from resume, not just skill names
4. Classify match as: Matched, Partially Matched, Missing, Unknown
5. Calculate confidence based on evidence strength
6. Never claim a match without supporting evidence
7. Calculate overall fit score with category breakdown

## Output Format
```json
{
  "overallMatchScore": "number (0-100)",
  "categoryScores": {
    "technicalSkills": "number",
    "experience": "number",
    "architecture": "number",
    "leadership": "number",
    "domain": "number",
    "atsKeywords": "number"
  },
  "skillMatches": [
    {
      "jdSkill": "string",
      "jdPriority": "Must Have|Should Have|Nice To Have|Unknown",
      "jdCategory": "string",
      "resumeSkill": "string|null",
      "resumeEvidence": "string|null",
      "match": "Matched|Partially Matched|Missing|Unknown",
      "confidence": "number (0-1)",
      "notes": "string"
    }
  ],
  "matchedSkills": ["string"],
  "partialMatches": [
    {
      "skill": "string",
      "gap": "string",
      "recommendation": "string"
    }
  ],
  "missingSkills": [
    {
      "skill": "string",
      "priority": "Must Have|Should Have|Nice To Have",
      "action": "Do not add to resume|Highlight transferable|Learn before applying"
    }
  ],
  "experienceMatch": {
    "requiredYears": "number",
    "candidateYears": "number",
    "match": "Exceeds|Meets|Below|Unknown",
    "details": "string"
  },
  "summary": "string"
}
```

## Validation Rules
- overallMatchScore between 0-100
- categoryScores between 0-100
- match must be one of: Matched, Partially Matched, Missing, Unknown
- confidence between 0-1
- All arrays can be empty but not null

## Failure Conditions
- Input JSON is invalid
- No JD requirements to match against
- JSON parsing fails