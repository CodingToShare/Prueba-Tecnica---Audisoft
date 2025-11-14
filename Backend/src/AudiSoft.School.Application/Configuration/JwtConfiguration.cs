namespace AudiSoft.School.Application.Configuration;

/// <summary>
/// Configuración de JWT para la aplicación
/// </summary>
public class JwtConfiguration
{
    /// <summary>
    /// Sección en appsettings.json
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Clave secreta para firmar tokens
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Emisor del token
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audiencia del token
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo de expiración del token en minutos
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Tiempo de expiración del refresh token en días
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;

    /// <summary>
    /// Tolerancia de tiempo en minutos para validar tokens
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;

    /// <summary>
    /// Valida que la configuración tenga todos los campos requeridos
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException("JWT SecretKey is required");

        if (SecretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long for HMAC SHA256");

        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("JWT Issuer is required");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("JWT Audience is required");

        if (ExpiryMinutes <= 0)
            throw new InvalidOperationException("JWT ExpiryMinutes must be greater than 0");

        if (RefreshTokenExpiryDays <= 0)
            throw new InvalidOperationException("JWT RefreshTokenExpiryDays must be greater than 0");

        if (ClockSkewMinutes < 0)
            throw new InvalidOperationException("JWT ClockSkewMinutes must be greater than or equal to 0");
    }
}