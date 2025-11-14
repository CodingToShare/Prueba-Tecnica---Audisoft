using AudiSoft.School.Application.Configuration;
using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Domain.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AudiSoft.School.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de autenticación JWT
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtConfiguration _jwtConfig;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IMapper mapper,
        ILogger<AuthService> logger,
        IOptions<JwtConfiguration> jwtConfig)
    {
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
        _logger = logger;
        _jwtConfig = jwtConfig.Value;
        _tokenHandler = new JwtSecurityTokenHandler();

        // Validar configuración JWT
        _jwtConfig.Validate();
    }

    /// <summary>
    /// Autentica un usuario con sus credenciales
    /// </summary>
    public async Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto loginRequest)
    {
        _logger.LogInformation("Iniciando autenticación para usuario: {UserName}", loginRequest.UserName);

        try
        {
            // Buscar usuario por username o email
            var usuario = await _usuarioRepository.Query()
                .Include(u => u.UsuarioRoles.Where(ur => !ur.IsDeleted && 
                    (ur.ValidoHasta == null || ur.ValidoHasta > DateTime.UtcNow)))
                    .ThenInclude(ur => ur.Rol)
                .Include(u => u.Profesor)
                .Include(u => u.Estudiante)
                .FirstOrDefaultAsync(u => 
                    (u.UserName == loginRequest.UserName || u.Email == loginRequest.UserName) 
                    && u.IsActive);

            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado o inactivo: {UserName}", loginRequest.UserName);
                throw new EntityNotFoundException("Usuario", "UserName/Email", loginRequest.UserName);
            }

            // Verificar contraseña
            if (!VerifyPassword(loginRequest.Password, usuario.PasswordHash))
            {
                _logger.LogWarning("Contraseña incorrecta para usuario: {UserName}", loginRequest.UserName);
                throw new InvalidEntityStateException("Credenciales inválidas");
            }

            // Auto-asignar IdProfesor/IdEstudiante si no están asignados
            var roles = usuario.UsuarioRoles.Where(ur => ur.Rol.IsActive).Select(ur => ur.Rol.Nombre).ToList();

            // Si es profesor pero no tiene IdProfesor asignado, y tiene Profesor incluido
            if (roles.Contains("Profesor") && !usuario.IdProfesor.HasValue && usuario.Profesor != null)
            {
                usuario.IdProfesor = usuario.Profesor.Id;
                _logger.LogInformation("Auto-asignado IdProfesor {ProfesorId} al usuario {UserName}", usuario.Profesor.Id, usuario.UserName);
            }

            // Si es estudiante pero no tiene IdEstudiante asignado, y tiene Estudiante incluido
            if (roles.Contains("Estudiante") && !usuario.IdEstudiante.HasValue && usuario.Estudiante != null)
            {
                usuario.IdEstudiante = usuario.Estudiante.Id;
                _logger.LogInformation("Auto-asignado IdEstudiante {EstudianteId} al usuario {UserName}", usuario.Estudiante.Id, usuario.UserName);
            }

            // Actualizar último login
            usuario.LastLoginAt = DateTime.UtcNow;
            usuario.UpdatedAt = DateTime.UtcNow;
            usuario.UpdatedBy = $"Auth:{usuario.UserName}";
            await _usuarioRepository.UpdateAsync(usuario);

            // Generar token (pasar el usuario original, no mapeado, para que tenga todos los datos)
            var accessToken = await GenerateTokenAsync(usuario);
            
            // Calcular expiración
            var expiryTime = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryMinutes);

            _logger.LogInformation("Autenticación exitosa para usuario: {UserName} (ID: {UserId})", 
                usuario.UserName, usuario.Id);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = _jwtConfig.ExpiryMinutes * 60, // Convertir a segundos
                ExpiresAt = expiryTime,
                User = new UserInfoDto
                {
                    Id = usuario.Id,
                    UserName = usuario.UserName,
                    Email = usuario.Email,
                    Roles = usuario.UsuarioRoles
                        .Where(ur => ur.Rol.IsActive)
                        .Select(ur => ur.Rol.Nombre)
                        .ToList(),
                    IdProfesor = usuario.IdProfesor,
                    IdEstudiante = usuario.IdEstudiante,
                    NombreCompleto = usuario.Profesor?.Nombre ?? usuario.Estudiante?.Nombre,
                    LastLoginAt = usuario.LastLoginAt
                }
            };
        }
        catch (Exception ex) when (!(ex is EntityNotFoundException || ex is InvalidEntityStateException))
        {
            _logger.LogError(ex, "Error durante autenticación de usuario: {UserName}", loginRequest.UserName);
            throw new InvalidEntityStateException("Error interno durante autenticación");
        }
    }

    /// <summary>
    /// Genera un nuevo token JWT para un usuario
    /// </summary>
    public async Task<string> GenerateTokenAsync(Usuario usuario)
    {
        _logger.LogDebug("Generando token JWT para usuario: {UserId}", usuario.Id);

        try
        {
            // Obtener roles del usuario
            var roles = await _usuarioRepository.Query()
                .Where(u => u.Id == usuario.Id)
                .SelectMany(u => u.UsuarioRoles)
                .Where(ur => !ur.IsDeleted && ur.Rol.IsActive && 
                    (ur.ValidoHasta == null || ur.ValidoHasta > DateTime.UtcNow))
                .Select(ur => ur.Rol.Nombre)
                .ToListAsync();

            // Crear claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.UserName),
                new("userId", usuario.Id.ToString()),
                new("userName", usuario.UserName)
            };

            // Agregar email si existe
            if (!string.IsNullOrEmpty(usuario.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, usuario.Email));
            }

            // Agregar roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Agregar IDs de profesor/estudiante si existen
            if (usuario.IdProfesor.HasValue)
            {
                claims.Add(new Claim("idProfesor", usuario.IdProfesor.Value.ToString()));
                _logger.LogDebug("Agregado claim idProfesor: {IdProfesor} para usuario: {UserId}", usuario.IdProfesor.Value, usuario.Id);
            }

            if (usuario.IdEstudiante.HasValue)
            {
                claims.Add(new Claim("idEstudiante", usuario.IdEstudiante.Value.ToString()));
                _logger.LogDebug("Agregado claim idEstudiante: {IdEstudiante} para usuario: {UserId}", usuario.IdEstudiante.Value, usuario.Id);
            }

            // Crear key y credentials
            var key = Encoding.UTF8.GetBytes(_jwtConfig.SecretKey);
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256);

            // Crear descriptor del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryMinutes),
                SigningCredentials = signingCredentials,
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow
            };

            // Crear y escribir token
            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            _logger.LogDebug("Token JWT generado exitosamente para usuario: {UserId}", usuario.Id);

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar token JWT para usuario: {UserId}", usuario.Id);
            throw new InvalidOperationException("Error al generar token de autenticación", ex);
        }
    }

    /// <summary>
    /// Valida un token JWT y retorna los claims
    /// </summary>
    public async Task<Dictionary<string, string>?> ValidateTokenAsync(string token)
    {
        _logger.LogDebug("Validando token JWT");

        try
        {
            var key = Encoding.UTF8.GetBytes(_jwtConfig.SecretKey);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(_jwtConfig.ClockSkewMinutes)
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Token JWT inválido: algoritmo incorrecto");
                return null;
            }

            // Extraer claims
            var claims = principal.Claims.ToDictionary(x => x.Type, x => x.Value);

            _logger.LogDebug("Token JWT validado exitosamente");

            return await Task.FromResult(claims);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token JWT inválido: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar token JWT");
            return null;
        }
    }

    /// <summary>
    /// Revoca todos los tokens de un usuario (implementación básica)
    /// </summary>
    public async Task RevokeUserTokensAsync(int usuarioId)
    {
        _logger.LogInformation("Revocando tokens para usuario: {UserId}", usuarioId);

        try
        {
            // En una implementación completa, aquí se podría:
            // 1. Mantener una lista de tokens revocados en caché/BD
            // 2. Incrementar un "security timestamp" en el usuario
            // 3. Usar refresh tokens con blacklist

            // Por ahora, solo actualizamos el timestamp del usuario
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario != null)
            {
                usuario.UpdatedAt = DateTime.UtcNow;
                usuario.UpdatedBy = $"TokenRevoke:{usuario.UserName}";
                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Tokens revocados para usuario: {UserId}", usuarioId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revocar tokens del usuario: {UserId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Verifica una contraseña contra su hash usando el mismo método que UsuarioService
    /// </summary>
    private static bool VerifyPassword(string password, string hash)
    {
        using var sha256 = SHA256.Create();
        var saltBytes = Encoding.UTF8.GetBytes("AudiSoft_School_Salt_2024");
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combinedBytes = new byte[saltBytes.Length + passwordBytes.Length];
        
        Array.Copy(saltBytes, 0, combinedBytes, 0, saltBytes.Length);
        Array.Copy(passwordBytes, 0, combinedBytes, saltBytes.Length, passwordBytes.Length);
        
        var hashBytes = sha256.ComputeHash(combinedBytes);
        var passwordHash = Convert.ToBase64String(hashBytes);
        
        return passwordHash == hash;
    }
}