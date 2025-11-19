using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RealtimeFormApp;

public class TissueSampleDescriptor
{
    [Required]
    [Display(Name = "Case ID")]
    public string CaseId { get; set; } = "";

    [Required]
    [Display(Name = "Sample Type")]
    public string SampleType { get; set; } = "";

    [Display(Name = "Patient Age")]
    public int? PatientAge { get; set; }

    [Display(Name = "Collection Date")]
    [DataType(DataType.Date)]
    public DateTime? CollectionDate { get; set; }

    [Display(Name = "Measurements")]
    public List<string> Measurements { get; set; } = new();

    [Display(Name = "Pathological Assessment")]
    public PathologicalAssessment Assessment { get; set; } = PathologicalAssessment.Pending;

    [Display(Name = "Additional Notes")]
    public string Notes { get; set; } = "";

    [Display(Name = "Staining Methods")]
    public List<string> StainingMethods { get; set; } = new();

    [Display(Name = "Microscopic Findings")]
    public string MicroscopicFindings { get; set; } = "";

    [Display(Name = "Diagnosis")]
    public string Diagnosis { get; set; } = "";

    [Display(Name = "Recommendations")]
    public string Recommendations { get; set; } = "";
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PathologicalAssessment
{
    Pending,
    Benign,
    [Display(Name = "Pre-cancerous")]
    PreCancerous,
    Cancerous
}
