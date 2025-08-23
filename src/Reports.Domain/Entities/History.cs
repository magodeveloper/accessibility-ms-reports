namespace Reports.Domain.Entities;

public class History
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AnalysisId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}