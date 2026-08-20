---
name: guardrails
description: Validate that tailored resume contains no fabricated claims. Checks every claim against original resume evidence. Fails if any unsupported claims are detected. Use when asked to validate resume accuracy.
---

# Guardrails Skill

## Purpose
Validate that tailored resume contains no fabricated claims.

## Inputs
- Tailored resume content (JSON)
- Original resume text
- Resume Intelligence output
- Skill Matching output

## Rules
1. EVERY claim in tailored resume must trace to original resume evidence
2. Check: skills, technologies, experience, employers, job titles, projects, certifications, education, achievements, metrics, responsibilities, leadership claims, domain experience, cloud experience
3. Flag any claim that cannot be supported by original resume
4. Distinguish between rewording (allowed) and fabrication (forbidden)
5. Check for experience inflation (e.g., "exposure" → "expert")
6. Check for metric invention
7. Check for employer/project invention

## Output Format
```json
{
  "status": "PASS|FAIL",
  "claimValidations": [
    {
      "claim": "string",
      "section": "ProfessionalSummary|CoreCompetencies|TechnicalSkills|Experience|Projects|Education|Certifications",
      "supported": "boolean",
      "evidence": "string|null",
      "confidence": "number (0-1)",
      "issue": "string|null",
      "originalText": "string|null"
    }
  ],
  "unsupportedClaims": [
    {
      "claim": "string",
      "section": "string",
      "reason": "string",
      "suggestedCorrection": "string"
    }
  ],
  "fabricationDetected": "boolean",
  "experienceInflationDetected": "boolean",
  "metricInventionDetected": "boolean",
  "summary": "string"
}
```

## Validation Rules
- status must be PASS or FAIL
- If any unsupportedClaims exist, status must be FAIL
- Every claim in tailored resume must be validated
- confidence between 0-1
- All arrays can be empty but not null

## Failure Conditions
- Input JSON invalid
- Tailored resume contains unsupported claims
- JSON parsing fails

## Critical: If status is FAIL, document generation MUST NOT proceed