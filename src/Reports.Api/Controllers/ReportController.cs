using Microsoft.AspNetCore.Mvc;
using Reports.Application.Dtos;
using Microsoft.Extensions.Localization;
using Reports.Application.Services.Report;

namespace Reports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ReportController : ControllerBase
{
    private readonly IReportService _service;
    private readonly IStringLocalizer<ReportController> _localizer;
    public ReportController(IReportService service, IStringLocalizer<ReportController> localizer)
    {
        _service = service;
        _localizer = localizer;
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
            return NotFound(new { message = _localizer["Error_ReportNotFound"] });
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
            return NotFound(new { message = _localizer["Error_ReportNotFound"] });
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
            return NotFound(new { message = _localizer["Error_ReportNotFound"] });
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
        return Ok(new { message = _localizer["Success_ReportCreated"], data = created });
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
        if (deleted)
            return Ok(new { message = _localizer["Success_ReportDeleted"] });
        return NotFound(new { message = _localizer["Error_ReportNotFound"] });
    }
}