namespace Reports.Domain.Entities;

public class Report
{
    public int Id { get; set; }
    public int AnalysisId { get; set; }
    public ReportFormat Format { get; set; }
    public string FilePath { get; set; } = null!;
    public DateTime GenerationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}