using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Domain.Entities;

namespace AudiSoft.School.Application.Interfaces;

/// <summary>
/// Servicio de autenticación para gestión de tokens JWT
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Autentica un usuario con sus credenciales
    /// </summary>
    /// <param name="loginRequest">Credenciales del usuario</param>
    /// <returns>Respuesta con token JWT e información del usuario</returns>
    Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto loginRequest);

    /// <summary>
    /// Valida un token JWT y retorna los claims
    /// </summary>
    /// <param name="token">Token JWT a validar</param>
    /// <returns>Claims del token si es válido, null si es inválido</returns>
    Task<Dictionary<string, string>?> ValidateTokenAsync(string token);

    /// <summary>
    /// Genera un nuevo token JWT para un usuario
    /// </summary>
    /// <param name="usuario">Usuario para el cual generar el token</param>
    /// <returns>Token JWT generado</returns>
    Task<string> GenerateTokenAsync(Usuario usuario);

    /// <summary>
    /// Revoca todos los tokens de un usuario (logout)
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    Task RevokeUserTokensAsync(int usuarioId);
}