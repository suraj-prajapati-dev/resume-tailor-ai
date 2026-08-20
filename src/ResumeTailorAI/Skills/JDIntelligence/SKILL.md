---
name: jd-intelligence
description: Analyze job description and extract structured requirements. Identifies required skills, experience level, responsibilities, and ATS keywords. Use when asked to analyze a job description.
---

# JD Intelligence Skill

## Purpose
Analyze job description and extract structured requirements.

## Inputs
- Raw job description text (string)
- Target role (string)

## Rules
1. Extract explicit requirements only
2. Classify each requirement as Must Have, Should Have, Nice To Have, or Unknown
3. Identify implicit vs explicit requirements
4. Flag ambiguous requirements
5. Extract ATS keywords and phrases
6. Do not infer requirements not stated

## Output Format
```json
{
  "targetRole": "string",
  "company": "string",
  "location": "string",
  "employmentType": "Full-time|Part-time|Contract|Remote|Hybrid",
  "experienceRequired": {
    "minYears": "number",
    "maxYears": "number|null",
    "level": "Junior|Mid|Senior|Lead|Principal|Architect"
  },
  "requiredSkills": [
    {
      "name": "string",
      "category": "Technical|Soft|Domain|Tool|Framework|Language|Database|Cloud|Architecture",
      "priority": "Must Have|Should Have|Nice To Have|Unknown",
      "isExplicit": "boolean",
      "isAmbiguous": "boolean",
      "context": "string",
      "atsPhrases": ["string"]
    }
  ],
  "responsibilities": ["string"],
  "domainRequirements": ["string"],
  "educationRequirements": [
    {
      "degree": "string",
      "field": "string",
      "required": "boolean"
    }
  ],
  "certificationRequirements": [
    {
      "name": "string",
      "required": "boolean"
    }
  ],
  "cloudRequirements": ["string"],
  "architectureRequirements": ["string"],
  "leadershipRequirements": ["string"],
  "keywords": ["string"],
  "atsPhrases": ["string"]
}
```

## Validation Rules
- Priority must be one of: Must Have, Should Have, Nice To Have, Unknown
- isExplicit and isAmbiguous must be boolean
- All arrays can be empty but not null

## Failure Conditions
- JD text is empty or too short (< 100 characters)
- Unable to identify target role
- JSON parsing fails