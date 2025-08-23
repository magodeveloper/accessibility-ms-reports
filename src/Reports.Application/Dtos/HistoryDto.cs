namespace Reports.Application.Dtos;

public class HistoryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AnalysisId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}