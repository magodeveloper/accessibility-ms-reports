namespace Reports.Application.Dtos;

public class ReportDto
{
    public int Id { get; set; }
    public int AnalysisId { get; set; }
    public string Format { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public DateTime GenerationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}