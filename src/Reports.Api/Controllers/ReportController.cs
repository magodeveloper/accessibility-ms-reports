using Microsoft.AspNetCore.Mvc;
using Reports.Application.Dtos;
using Reports.Application.Services.Report;

namespace Reports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ReportController : ControllerBase
{
    private readonly IReportService _service;
    public ReportController(IReportService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene todos los informes.
    /// </summary>
    /// <response code="200">Lista completa de informes</response>
    /// <response code="404">No se encontraron informes</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReportDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAll()
    {
        var reports = await _service.GetAllAsync();
        var lang = Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
        if (reports == null || !reports.Any())
            return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
        return Ok(new { message = Reports.Application.Localization.Get("Success_ReportList", lang), data = reports });
    }

    /// <summary>
    /// Obtiene el informe por ID de análisis.
    /// </summary>
    /// <param name="analysisId">ID del análisis</param>
    /// <response code="200">Informe encontrado</response>
    /// <response code="404">No se encontró informe para el análisis</response>
    [HttpGet("by-analysis/{analysisId}")]
    [ProducesResponseType(typeof(IEnumerable<ReportDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByAnalysisId(int analysisId)
    {
        var reports = await _service.GetByAnalysisIdAsync(analysisId);
        if (reports == null || !reports.Any())
        {
            var lang = Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
            return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
        }
        return Ok(reports);
    }

    /// <summary>
    /// Obtiene el informe por fecha de generación.
    /// </summary>
    /// <param name="date">Fecha de generación</param>
    /// <response code="200">Informe encontrado</response>
    /// <response code="404">No se encontró informe para la fecha de generación</response>
    [HttpGet("by-date/{date}")]
    [ProducesResponseType(typeof(IEnumerable<ReportDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByGenerationDate(DateTime date)
    {
        var reports = await _service.GetByGenerationDateAsync(date);
        if (reports == null || !reports.Any())
        {
            var lang = Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
            return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
        }
        return Ok(reports);
    }

    /// <summary>
    /// Obtiene el informe por formato.
    /// </summary>
    /// <param name="format">Formato del informe</param>
    /// <response code="200">Informe encontrado</response>
    /// <response code="404">No se encontró informe para el formato proporcionado</response>
    [HttpGet("by-format/{format}")]
    [ProducesResponseType(typeof(IEnumerable<ReportDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByFormat(string format)
    {
        var reports = await _service.GetByFormatAsync(format);
        if (reports == null || !reports.Any())
        {
            var lang = Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
            return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
        }
        return Ok(reports);
    }

    /// <summary>
    /// Crea un nuevo informe.
    /// </summary>
    /// <param name="dto">Datos del informe a crear</param>
    /// <response code="200">Informe creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReportDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] ReportDto dto)
    {
        var created = await _service.CreateAsync(dto);
        var lang = Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
        return Ok(new { message = Reports.Application.Localization.Get("Success_ReportCreated", lang), data = created });
    }

    /// <summary>
    /// Elimina un informe por ID.
    /// </summary>
    /// <param name="id">ID del informe a eliminar</param>
    /// <response code="200">Informe eliminado exitosamente</response>
    /// <response code="404">No se encontró informe para el ID proporcionado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        var lang = Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
        if (deleted)
            return Ok(new { message = Reports.Application.Localization.Get("Success_ReportDeleted", lang) });
        return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
    }

    /// <summary>
    /// Elimina todos los informes.
    /// </summary>
    /// <response code="200">Todos los informes eliminados exitosamente</response>
    /// <response code="404">No se encontraron informes para eliminar</response>
    [HttpDelete("all")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAll()
    {
        var deleted = await _service.DeleteAllAsync();
        var lang = Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
        if (deleted)
            return Ok(new { message = Reports.Application.Localization.Get("Success_AllReportsDeleted", lang) });
        return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
    }
}