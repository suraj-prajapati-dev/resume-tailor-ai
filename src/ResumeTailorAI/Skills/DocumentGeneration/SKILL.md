---
name: document-generation
description: Generate ATS-friendly DOCX resume and cover letter from tailored resume content. Requires guardrail validation to pass before generation. Use when asked to generate final documents.
---

# Document Generation Skill

## Purpose
Generate ATS-friendly DOCX resume and cover letter.

## Inputs
- Tailored resume content (JSON)
- Original resume text
- JD Intelligence output
- Target role
- Guardrail result (must be PASS)

## Rules
1. Generate clean, professional, ATS-friendly DOCX
2. No graphics, images, complex tables, unusual symbols
3. Use standard fonts (Calibri, Arial)
4. Simple section structure with clear headings
5. Cover letter: 350-500 words, concise, no generic AI language
6. Cover letter based on verified experience only
7. Both documents must pass guardrail validation

## Resume DOCX Structure
1. Contact Information (name, email, phone, location, LinkedIn)
2. Professional Summary
3. Core Competencies (bulleted)
4. Technical Skills (categorized)
5. Professional Experience (reverse chronological)
6. Projects (if applicable)
7. Education
8. Certifications (if applicable)

## Cover Letter Structure
1. Header (contact info, date, employer)
2. Salutation
3. Opening (role applied for, how found)
4. Relevant experience paragraph
5. Technical strengths paragraph
6. Architecture/leadership paragraph
7. Why candidate fits role
8. Closing
9. Signature

## Output Format
```json
{
  "resumeDocxPath": "string",
  "coverLetterDocxPath": "string",
  "resumeWordCount": "number",
  "coverLetterWordCount": "number",
  "generationTimestamp": "string (ISO 8601)"
}
```

## Validation Rules
- Both files must exist and be valid DOCX
- coverLetterWordCount between 350-500
- Guardrail status must be PASS before generation
- Files must be in temporary session directory

## Failure Conditions
- Guardrail status is FAIL
- Document generation fails
- Output directory not writable
- Word count outside acceptable range