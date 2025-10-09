using Reports.Domain.Entities;
using Reports.Application.Dtos;
using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Reports.Application.Services.History;

public class HistoryService : IHistoryService
{
    private readonly ReportsDbContext _db;
    public HistoryService(ReportsDbContext db) => _db = db;

    public async Task<IEnumerable<HistoryDto>> GetAllAsync()
    {
        return await _db.History
            .AsNoTracking()  // Read-only optimization
            .Select(h => ToDto(h)).ToListAsync();
    }

    public async Task<IEnumerable<HistoryDto>> GetByUserIdAsync(int userId)
    {
        return await _db.History
            .AsNoTracking()  // Read-only optimization
            .Where(h => h.UserId == userId)
            .Select(h => ToDto(h)).ToListAsync();
    }

    public async Task<IEnumerable<HistoryDto>> GetByAnalysisIdAsync(int analysisId)
    {
        return await _db.History
            .AsNoTracking()  // Read-only optimization
            .Where(h => h.AnalysisId == analysisId)
            .Select(h => ToDto(h)).ToListAsync();
    }

    public async Task<HistoryDto> CreateAsync(HistoryDto dto)
    {
        var entity = new Reports.Domain.Entities.History
        {
            UserId = dto.UserId,
            AnalysisId = dto.AnalysisId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.History.Add(entity);
        await _db.SaveChangesAsync();
        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.History.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        _db.History.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllAsync()
    {
        var entities = await _db.History.ToListAsync();
        if (!entities.Any())
        {
            return false;
        }

        _db.History.RemoveRange(entities);
        await _db.SaveChangesAsync();
        return true;
    }

    private static HistoryDto ToDto(Reports.Domain.Entities.History h) => new()
    {
        Id = h.Id,
        UserId = h.UserId,
        AnalysisId = h.AnalysisId,
        CreatedAt = h.CreatedAt,
        UpdatedAt = h.UpdatedAt
    };
}