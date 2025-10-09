using Reports.Domain.Entities;
using Reports.Application.Dtos;
using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Reports.Application.Services.Report;

public class ReportService : IReportService
{
    private readonly ReportsDbContext _db;
    public ReportService(ReportsDbContext db) => _db = db;

    public async Task<IEnumerable<ReportDto>> GetAllAsync()
    {
        return await _db.Reports
            .AsNoTracking()  // Read-only optimization
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportDto>> GetByAnalysisIdAsync(int analysisId)
    {
        return await _db.Reports
            .AsNoTracking()  // Read-only optimization
            .Where(r => r.AnalysisId == analysisId)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportDto>> GetByGenerationDateAsync(DateTime date)
        => await _db.Reports
            .AsNoTracking()  // Read-only optimization
            .Where(r => r.GenerationDate.Year == date.Year &&
                        r.GenerationDate.Month == date.Month &&
                        r.GenerationDate.Day == date.Day)
            .Select(r => ToDto(r)).ToListAsync();

    public async Task<IEnumerable<ReportDto>> GetByFormatAsync(string format)
    {
        // Primero intentar con el nombre del enum (Excel, Html, Json, Pdf)
        if (Enum.TryParse<ReportFormat>(format, true, out var enumFormat))
        {
            return await _db.Reports
                .AsNoTracking()  // Read-only optimization
                .Where(r => r.Format == enumFormat)
                .Select(r => ToDto(r)).ToListAsync();
        }

        // Si no funciona, intentar con los valores JSON (excel, html, json, pdf)
        var lowerFormat = format.ToLower();
        var jsonEnumFormat = lowerFormat switch
        {
            "pdf" => ReportFormat.Pdf,
            "html" => ReportFormat.Html,
            "json" => ReportFormat.Json,
            "excel" => ReportFormat.Excel,
            _ => (ReportFormat?)null
        };

        if (jsonEnumFormat.HasValue)
        {
            return await _db.Reports
                .AsNoTracking()  // Read-only optimization
                .Where(r => r.Format == jsonEnumFormat.Value)
                .Select(r => ToDto(r)).ToListAsync();
        }

        return new List<ReportDto>();
    }

    public async Task<ReportDto> CreateAsync(ReportDto dto)
    {
        var entity = new Reports.Domain.Entities.Report
        {
            AnalysisId = dto.AnalysisId,
            Format = dto.Format, // Ya es ReportFormat, no necesita conversión
            FilePath = dto.FilePath,
            GenerationDate = dto.GenerationDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Reports.Add(entity);
        await _db.SaveChangesAsync();
        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Reports.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        _db.Reports.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllAsync()
    {
        var entities = await _db.Reports.ToListAsync();
        if (!entities.Any())
        {
            return false;
        }

        _db.Reports.RemoveRange(entities);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ReportDto ToDto(Reports.Domain.Entities.Report r) => new()
    {
        Id = r.Id,
        AnalysisId = r.AnalysisId,
        Format = r.Format, // Usar directamente el enum
        FilePath = r.FilePath ?? string.Empty, // Manejar null apropiadamente
        GenerationDate = r.GenerationDate,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}