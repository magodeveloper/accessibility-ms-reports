using Reports.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Reports.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Reports.Application.Services.History;
using Reports.Application.Services.UserContext;

namespace Reports.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _service;
    private readonly IUserContext _userContext;

    public HistoryController(IHistoryService service, IUserContext userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    /// <summary>
    /// Obtiene todos los registros de historial.
    /// </summary>
    /// <response code="200">Lista completa de historial</response>
    /// <response code="404">No se encontraron registros de historial</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HistoryDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAll()
    {
        // Si el usuario no está autenticado, retornar Unauthorized
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        // Filtrar por userId del contexto autenticado
        var result = await _service.GetByUserIdAsync(_userContext.UserId);
        var lang = LanguageHelper.GetRequestLanguage(Request);
        if (result == null || !result.Any())
        {
            return NotFound(new { message = Reports.Application.Localization.Get("Error_HistoryNotFound", lang) });
        }

        return Ok(new { message = Reports.Application.Localization.Get("Success_HistoryList", lang), data = result });
    }

    /// <summary>
    /// Obtiene el historial por ID de usuario.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <response code="200">Lista de historial encontrada</response>
    /// <response code="404">No se encontró historial para el usuario</response>
    [HttpGet("by-user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<HistoryDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        // Si el usuario no está autenticado, retornar Unauthorized
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        // Validar que el usuario solo acceda a su propio historial (a menos que sea admin)
        if (!_userContext.IsAdmin && userId != _userContext.UserId)
        {
            return Forbid(); // 403 Forbidden
        }

        var result = await _service.GetByUserIdAsync(userId);
        if (result == null || !result.Any())
        {
            var lang = LanguageHelper.GetRequestLanguage(Request);
            return NotFound(new { message = Reports.Application.Localization.Get("Error_HistoryNotFound", lang) });
        }
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el historial por ID de análisis.
    /// </summary>
    /// <param name="analysisId">ID del análisis</param>
    /// <response code="200">Lista de historial encontrada</response>
    /// <response code="404">No se encontró historial para el análisis</response>
    [HttpGet("by-analysis/{analysisId}")]
    [ProducesResponseType(typeof(IEnumerable<HistoryDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByAnalysisId(int analysisId)
    {
        var result = await _service.GetByAnalysisIdAsync(analysisId);
        if (result == null || !result.Any())
        {
            var lang = LanguageHelper.GetRequestLanguage(Request);
            return NotFound(new { message = Reports.Application.Localization.Get("Error_HistoryNotFound", lang) });
        }
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo historial.
    /// </summary>
    /// <param name="dto">Datos del historial a crear</param>
    /// <response code="200">Historial creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>    
    [HttpPost]
    [ProducesResponseType(typeof(HistoryDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] HistoryDto dto)
    {
        // Si el usuario no está autenticado, retornar Unauthorized
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        // Usar el UserId del contexto autenticado (ignorar el del body)
        dto.UserId = _userContext.UserId;

        var created = await _service.CreateAsync(dto);
        var lang = LanguageHelper.GetRequestLanguage(Request);
        return Ok(new { message = Reports.Application.Localization.Get("Success_HistoryCreated", lang), data = created });
    }

    /// <summary>
    /// Elimina un historial por ID.
    /// </summary>
    /// <param name="id">ID del historial a eliminar</param>
    /// <response code="200">Historial eliminado exitosamente</response>
    /// <response code="404">No se encontró historial para el ID proporcionado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        var lang = LanguageHelper.GetRequestLanguage(Request);
        if (deleted)
        {
            return Ok(new { message = Reports.Application.Localization.Get("Success_HistoryDeleted", lang) });
        }

        return NotFound(new { message = Reports.Application.Localization.Get("Error_HistoryNotFound", lang) });
    }

    /// <summary>
    /// Elimina todos los registros de historial.
    /// </summary>
    /// <response code="200">Todos los registros de historial eliminados exitosamente</response>
    /// <response code="404">No se encontraron registros de historial para eliminar</response>
    [HttpDelete("all")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAll()
    {
        var deleted = await _service.DeleteAllAsync();
        var lang = LanguageHelper.GetRequestLanguage(Request);
        if (deleted)
        {
            return Ok(new { message = Reports.Application.Localization.Get("Success_AllHistoryDeleted", lang) });
        }

        return NotFound(new { message = Reports.Application.Localization.Get("Error_HistoryNotFound", lang) });
    }
}