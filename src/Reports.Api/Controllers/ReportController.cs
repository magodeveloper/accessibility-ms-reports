using Reports.Api.Helpers;
using Reports.Api.Metrics;
using Reports.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Reports.Application.Services.Report;
using Reports.Application.Services.UserContext;

namespace Reports.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _service;
    private readonly IUserContext _userContext;

    public ReportController(IReportService service, IUserContext userContext)
    {
        _service = service;
        _userContext = userContext;
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
        using (ReportsMetrics.MeasureQuery("get_all"))
        {
            // Validar autenticación
            if (!_userContext.IsAuthenticated)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var reports = await _service.GetAllAsync();
            ReportsMetrics.RecordQuery("get_all");

            var lang = LanguageHelper.GetRequestLanguage(Request);
            if (reports == null || !reports.Any())
            {
                return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
            }
            return Ok(new { message = Reports.Application.Localization.Get("Success_ReportList", lang), data = reports });
        }
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
        using (ReportsMetrics.MeasureQuery("get_by_analysis"))
        {
            var reports = await _service.GetByAnalysisIdAsync(analysisId);
            ReportsMetrics.RecordQuery("get_by_analysis");

            if (reports == null || !reports.Any())
            {
                var lang = LanguageHelper.GetRequestLanguage(Request);
                return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
            }
            return Ok(reports);
        }
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
        using (ReportsMetrics.MeasureQuery("get_by_date"))
        {
            var reports = await _service.GetByGenerationDateAsync(date);
            ReportsMetrics.RecordQuery("get_by_date");

            if (reports == null || !reports.Any())
            {
                var lang = LanguageHelper.GetRequestLanguage(Request);
                return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
            }
            return Ok(reports);
        }
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
        using (ReportsMetrics.MeasureQuery("get_by_format"))
        {
            var reports = await _service.GetByFormatAsync(format);
            ReportsMetrics.RecordQuery("get_by_format");

            if (reports == null || !reports.Any())
            {
                var lang = LanguageHelper.GetRequestLanguage(Request);
                return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
            }
            return Ok(reports);
        }
    }

    /// <summary>
    /// Crea un nuevo informe.
    /// </summary>
    /// <param name="dto">Datos del informe a crear</param>
    /// <response code="200">Informe creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    [AllowAnonymous]  // Permitir acceso desde Middleware con headers X-User-*
    [HttpPost]
    [ProducesResponseType(typeof(ReportDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] ReportDto dto)
    {
        // Validar autenticación
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        using (ReportsMetrics.MeasureReportGeneration(dto.Format.ToString()))
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                ReportsMetrics.RecordReportGenerated(dto.Format.ToString(), success: true);

                var lang = LanguageHelper.GetRequestLanguage(Request);
                return Ok(new { message = Reports.Application.Localization.Get("Success_ReportCreated", lang), data = created });
            }
            catch
            {
                ReportsMetrics.RecordReportGenerated(dto.Format.ToString(), success: false);
                throw;
            }
        }
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
        {
            ReportsMetrics.ReportsDeletionsTotal.Inc();
        }

        var lang = LanguageHelper.GetRequestLanguage(Request);
        if (deleted)
        {
            return Ok(new { message = Reports.Application.Localization.Get("Success_ReportDeleted", lang) });
        }

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
        if (deleted)
        {
            ReportsMetrics.ReportsDeletionsTotal.Inc();
        }

        var lang = LanguageHelper.GetRequestLanguage(Request);
        if (deleted)
        {
            return Ok(new { message = Reports.Application.Localization.Get("Success_AllReportsDeleted", lang) });
        }

        return NotFound(new { message = Reports.Application.Localization.Get("Error_ReportNotFound", lang) });
    }
}