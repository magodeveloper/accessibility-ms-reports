using Reports.Application.Dtos;

namespace Reports.Application.Services.History;

public interface IHistoryService
{
    Task<IEnumerable<HistoryDto>> GetByUserIdAsync(int userId);
    Task<IEnumerable<HistoryDto>> GetByAnalysisIdAsync(int analysisId);
    Task<HistoryDto> CreateAsync(HistoryDto dto);
    Task<bool> DeleteAsync(int id);
}