using AudiSoft.School.Application.Configuration;
using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Domain.Exceptions;
using AudiSoft.School.Infrastructure.Persistence;
using AudiSoft.School.Infrastructure.Repositories;
using AudiSoft.School.Infrastructure.Services;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace AudiSoft.School.Tests.Unit.Services;

/// <summary>
/// Tests unitarios esenciales para AuthService - producción
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly AudiSoftSchoolDbContext _context;
    private readonly AuthService _authService;
    private readonly Mock<ILogger<AuthService>> _loggerMock;

    public AuthServiceTests()
    {
        // Crear base de datos en memoria para pruebas
        var options = new DbContextOptionsBuilder<AudiSoftSchoolDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AudiSoftSchoolDbContext(options);

        // Configurar mocks
        _loggerMock = new Mock<ILogger<AuthService>>();
        
        // Configurar JWT con la misma configuración que producción
        var jwtConfig = Options.Create(new JwtConfiguration
        {
            SecretKey = "super_secret_key_for_jwt_token_generation_minimum_256_bits_long_enough_for_security",
            Issuer = "AudiSoft.School.Api",
            Audience = "AudiSoft.School.Users",
            ExpiryMinutes = 60
        });

        // Configurar AutoMapper real
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Usuario, UsuarioDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => 
                    src.UsuarioRoles.Where(ur => !ur.IsDeleted && ur.Rol.IsActive)
                        .Select(ur => ur.Rol.Nombre).ToList()));
        });
        var mapper = mapperConfig.CreateMapper();

        // Crear repositorio real con contexto InMemory
        var usuarioRepository = new UsuarioRepository(_context);

        // Crear AuthService
        _authService = new AuthService(usuarioRepository, mapper, _loggerMock.Object, jwtConfig);

        // Sembrar datos de prueba
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Crear un usuario de prueba con el hash correcto
        var admin = new Usuario
        {
            Id = 1,
            UserName = "admin",
            Email = "admin@test.com",
            PasswordHash = HashPassword("Admin@123456"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test"
        };

        // Crear rol Admin
        var adminRole = new Rol
        {
            Id = 1,
            Nombre = "Admin",
            Descripcion = "Administrador",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test"
        };

        // Asignar rol al usuario
        var usuarioRol = new UsuarioRol
        {
            Id = 1,
            IdUsuario = 1,
            IdRol = 1,
            AsignadoEn = DateTime.UtcNow,
            AsignadoPor = "Test",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test"
        };

        _context.Usuarios.Add(admin);
        _context.Roles.Add(adminRole);
        _context.UsuarioRoles.Add(usuarioRol);
        _context.SaveChanges();
    }

    #region Tests Esenciales de Producción

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            UserName = "admin",
            Password = "Admin@123456"
        };

        // Act
        var result = await _authService.AuthenticateAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User!.UserName.Should().Be("admin");
        result.User.Email.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            UserName = "admin",
            Password = "WrongPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidEntityStateException>(
            () => _authService.AuthenticateAsync(loginRequest));
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentUser_ShouldThrowException()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            UserName = "nonexistent",
            Password = "AnyPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _authService.AuthenticateAsync(loginRequest));
    }

    [Theory]
    [InlineData("", "Password@123")]
    [InlineData("test", "Password@123")]
    [InlineData("username", "")]
    [InlineData("username", "test")]
    public async Task AuthenticateAsync_WithInvalidInput_ShouldThrowException(string userName, string password)
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            UserName = userName,
            Password = password
        };

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _authService.AuthenticateAsync(loginRequest));
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ShouldReturnClaims()
    {
        // Arrange - Crear un token JWT válido usando la MISMA configuración que el AuthService
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("super_secret_key_for_jwt_token_generation_minimum_256_bits_long_enough_for_security");
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Email, "admin@test.com"),
                new Claim("userId", "1")
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = "AudiSoft.School.Api",
            Audience = "AudiSoft.School.Users",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };
        var token = handler.CreateToken(descriptor);
        var tokenString = handler.WriteToken(token);

        // Act
        var claims = await _authService.ValidateTokenAsync(tokenString);

        // Assert
        claims.Should().NotBeNull();
        claims.Should().NotBeEmpty();
        claims.Should().ContainKey(ClaimTypes.Name);
        claims![ClaimTypes.Name].Should().Be("admin");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid.token.format")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid")]
    public async Task ValidateTokenAsync_WithInvalidToken_ShouldReturnNull(string invalidToken)
    {
        // Act
        var claims = await _authService.ValidateTokenAsync(invalidToken);

        // Assert
        claims.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Hash password usando el mismo método que AuthService
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var saltBytes = Encoding.UTF8.GetBytes("AudiSoft_School_Salt_2024");
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combinedBytes = new byte[saltBytes.Length + passwordBytes.Length];
        
        Array.Copy(saltBytes, 0, combinedBytes, 0, saltBytes.Length);
        Array.Copy(passwordBytes, 0, combinedBytes, saltBytes.Length, passwordBytes.Length);
        
        var hashBytes = sha256.ComputeHash(combinedBytes);
        return Convert.ToBase64String(hashBytes);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}