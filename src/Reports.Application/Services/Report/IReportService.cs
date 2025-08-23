using Reports.Application.Dtos;

namespace Reports.Application.Services.Report;

public interface IReportService
{
    Task<IEnumerable<ReportDto>> GetByAnalysisIdAsync(int analysisId);
    Task<IEnumerable<ReportDto>> GetByGenerationDateAsync(DateTime date);
    Task<IEnumerable<ReportDto>> GetByFormatAsync(string format);
    Task<ReportDto> CreateAsync(ReportDto dto);
    Task<bool> DeleteAsync(int id);
}