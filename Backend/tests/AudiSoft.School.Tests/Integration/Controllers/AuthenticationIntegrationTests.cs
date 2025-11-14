using AudiSoft.School.Application.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace AudiSoft.School.Tests.Integration.Controllers;

/// <summary>
/// Tests de integración enfocados en autenticación y autorización real de producción
/// </summary>
public class AuthenticationIntegrationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.CleanDatabaseAsync();
        _client.Dispose();
    }

    #region Authentication Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var loginDto = new LoginRequestDto
        {
            UserName = "admin",
            Password = "Admin@123456"
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();
        loginResponse.User.Should().NotBeNull();
        loginResponse.User!.UserName.Should().Be("admin");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401()
    {
        // Arrange
        var loginDto = new LoginRequestDto
        {
            UserName = "admin",
            Password = "wrongpassword"
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/estudiantes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminEndpoint_WithAdminToken_ShouldReturn200()
    {
        // Arrange
        var token = await GetTokenAsync("admin", "Admin@123456");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/estudiantes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminEndpoint_WithNonAdminToken_ShouldReturn403()
    {
        // Arrange
        var token = await GetTokenAsync("maria.garcia", "Profesor@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Intentar crear estudiante (solo Admin puede)
        var createDto = new CreateEstudianteDto { Nombre = "Test Student" };
        var json = JsonSerializer.Serialize(createDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/estudiantes", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ProfesorEndpoint_WithProfesorToken_ShouldReturn200()
    {
        // Arrange
        var token = await GetTokenAsync("maria.garcia", "Profesor@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Profesor puede ver estudiantes
        var response = await _client.GetAsync("/api/v1/estudiantes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EstudianteEndpoint_WithEstudianteToken_ShouldReturn403ForStudentsList()
    {
        // Arrange
        var token = await GetTokenAsync("juan.perez", "Estudiante@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Estudiante NO puede ver lista de estudiantes
        var response = await _client.GetAsync("/api/v1/estudiantes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NotasEndpoint_WithEstudianteToken_ShouldReturn200()
    {
        // Arrange
        var token = await GetTokenAsync("juan.perez", "Estudiante@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Estudiante puede ver sus propias notas
        var response = await _client.GetAsync("/api/v1/notas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Helper Methods

    private async Task<string> GetTokenAsync(string userName, string password)
    {
        var loginDto = new LoginRequestDto
        {
            UserName = userName,
            Password = password
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return loginResponse?.AccessToken ?? throw new InvalidOperationException("No se pudo obtener el token");
    }

    #endregion
}