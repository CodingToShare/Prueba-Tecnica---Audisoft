using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace AudiSoft.School.Api.Controllers;

/// <summary>
/// Controlador de autenticación y autorización
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Endpoints para autenticación y gestión de sesiones JWT")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Constructor del controlador de autenticación
    /// </summary>
    /// <param name="authService">Servicio de autenticación</param>
    /// <param name="logger">Logger para el controlador</param>
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Autentica un usuario con sus credenciales y devuelve un token JWT
    /// </summary>
    /// <param name="loginRequest">Credenciales del usuario</param>
    /// <returns>Token JWT e información del usuario</returns>
    /// <response code="200">Autenticación exitosa</response>
    /// <response code="400">Credenciales inválidas o datos mal formados</response>
    /// <response code="401">Usuario no autorizado</response>
    /// <response code="404">Usuario no encontrado</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Autenticar usuario",
        Description = "Autentica las credenciales del usuario y devuelve un token JWT para acceder a endpoints protegidos.",
        OperationId = "AuthenticateUser",
        Tags = new[] { "Autenticación" }
    )]
    [SwaggerResponse(200, "Login exitoso", typeof(LoginResponseDto))]
    [SwaggerResponse(401, "Credenciales inválidas")]
    [SwaggerResponse(400, "Datos de entrada malformados")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        _logger.LogInformation("Solicitud de login recibida para usuario: {UserName}", loginRequest.UserName);

        try
        {
            var loginResponse = await _authService.AuthenticateAsync(loginRequest);

            _logger.LogInformation("Login exitoso para usuario: {UserName}", loginRequest.UserName);

            return Ok(loginResponse);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Login fallido para usuario: {UserName}", loginRequest.UserName);
            
            // No revelar información específica del error por seguridad
            return Unauthorized(new { Message = "Credenciales inválidas" });
        }
    }

    /// <summary>
    /// Valida un token JWT
    /// </summary>
    /// <param name="request">Solicitud con el token a validar</param>
    /// <returns>Información del token si es válido</returns>
    /// <response code="200">Token válido</response>
    /// <response code="401">Token inválido o expirado</response>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequestDto request)
    {
        _logger.LogDebug("Solicitud de validación de token recibida");

        try
        {
            var claims = await _authService.ValidateTokenAsync(request.Token);

            if (claims == null)
            {
                return Unauthorized(new { Message = "Token inválido" });
            }

            return Ok(new { Valid = true, Claims = claims });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al validar token");
            return Unauthorized(new { Message = "Token inválido" });
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario actual (revoca tokens)
    /// </summary>
    /// <returns>Resultado del logout</returns>
    /// <response code="200">Logout exitoso</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Logout fallido: UserId no encontrado en claims");
            return Unauthorized(new { Message = "Usuario no válido" });
        }

        _logger.LogInformation("Solicitud de logout para usuario: {UserId}", userId);

        try
        {
            await _authService.RevokeUserTokensAsync(userId);

            _logger.LogInformation("Logout exitoso para usuario: {UserId}", userId);

            return Ok(new { Message = "Sesión cerrada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante logout para usuario: {UserId}", userId);
            return Ok(new { Message = "Error durante logout, pero sesión considerada cerrada" });
        }
    }

    /// <summary>
    /// Obtiene información del usuario actual autenticado
    /// </summary>
    /// <returns>Información del usuario</returns>
    /// <response code="200">Información del usuario obtenida</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var userInfo = new
            {
                Id = User.FindFirst("userId")?.Value,
                UserName = User.FindFirst("userName")?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                IdProfesor = User.FindFirst("idProfesor")?.Value,
                IdEstudiante = User.FindFirst("idEstudiante")?.Value
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información del usuario actual");
            return BadRequest(new { Message = "Error al obtener información del usuario" });
        }
    }
}

/// <summary>
/// DTO para solicitud de validación de token
/// </summary>
public class ValidateTokenRequestDto
{
    /// <summary>
    /// Token JWT a validar
    /// </summary>
    public string Token { get; set; } = string.Empty;
}