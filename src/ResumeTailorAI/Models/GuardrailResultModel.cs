using System.Text.Json.Serialization;

namespace ResumeTailorAI.Models;

public class GuardrailResultModel
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "FAIL";

    [JsonPropertyName("claimValidations")]
    public List<ClaimValidation> ClaimValidations { get; set; } = new();

    [JsonPropertyName("unsupportedClaims")]
    public List<UnsupportedClaim> UnsupportedClaims { get; set; } = new();

    [JsonPropertyName("fabricationDetected")]
    public bool FabricationDetected { get; set; }

    [JsonPropertyName("experienceInflationDetected")]
    public bool ExperienceInflationDetected { get; set; }

    [JsonPropertyName("metricInventionDetected")]
    public bool MetricInventionDetected { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}

public class ClaimValidation
{
    [JsonPropertyName("claim")]
    public string Claim { get; set; } = string.Empty;

    [JsonPropertyName("section")]
    public string Section { get; set; } = string.Empty;

    [JsonPropertyName("supported")]
    public bool Supported { get; set; }

    [JsonPropertyName("evidence")]
    public string? Evidence { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("issue")]
    public string? Issue { get; set; }

    [JsonPropertyName("originalText")]
    public string? OriginalText { get; set; }
}

public class UnsupportedClaim
{
    [JsonPropertyName("claim")]
    public string Claim { get; set; } = string.Empty;

    [JsonPropertyName("section")]
    public string Section { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("suggestedCorrection")]
    public string SuggestedCorrection { get; set; } = string.Empty;
}