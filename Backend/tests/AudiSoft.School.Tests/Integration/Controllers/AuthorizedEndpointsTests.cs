using AudiSoft.School.Application.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace AudiSoft.School.Tests.Integration.Controllers;

/// <summary>
/// Tests de integración para funcionalidades CRUD básicas del sistema
/// </summary>
public class AuthorizedEndpointsTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthorizedEndpointsTests(TestWebApplicationFactory factory)
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

    #region Estudiantes CRUD Tests

    [Fact]
    public async Task GetEstudiantes_WithAdminToken_ShouldReturnStudentsList()
    {
        // Arrange
        var token = await GetTokenAsync("admin", "Admin@123456");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/estudiantes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateEstudiante_WithAdminToken_ShouldCreateSuccessfully()
    {
        // Arrange
        var token = await GetTokenAsync("admin", "Admin@123456");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateEstudianteDto
        {
            Nombre = "Estudiante Prueba CRUD"
        };

        var json = JsonSerializer.Serialize(createDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/estudiantes", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var estudiante = JsonSerializer.Deserialize<EstudianteDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        estudiante.Should().NotBeNull();
        estudiante!.Nombre.Should().Be("Estudiante Prueba CRUD");
        estudiante.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Notas CRUD Tests

    [Fact]
    public async Task GetNotas_WithProfesorToken_ShouldReturnNotasList()
    {
        // Arrange
        var token = await GetTokenAsync("maria.garcia", "Profesor@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/notas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateNota_WithProfesorToken_ShouldCreateSuccessfully()
    {
        // Arrange - Crear estudiante primero
        var adminToken = await GetTokenAsync("admin", "Admin@123456");
        var estudianteId = await CreateTestEstudianteAsync(adminToken);

        // Usar token de profesor para crear nota
        var profesorToken = await GetTokenAsync("maria.garcia", "Profesor@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profesorToken);

        var createNotaDto = new CreateNotaDto
        {
            Nombre = "Evaluación Test CRUD",
            Valor = 4.5m,
            IdProfesor = 2, // María García es el segundo profesor
            IdEstudiante = estudianteId
        };

        var json = JsonSerializer.Serialize(createNotaDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/notas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var nota = JsonSerializer.Deserialize<NotaDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        nota.Should().NotBeNull();
        nota!.Nombre.Should().Be("Evaluación Test CRUD");
        nota.Valor.Should().Be(4.5m);
    }

    #endregion

    #region Profesores Tests

    [Fact]
    public async Task GetProfesores_WithAdminToken_ShouldReturnProfesoresList()
    {
        // Arrange
        var token = await GetTokenAsync("admin", "Admin@123456");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/profesores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
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

    private async Task<int> CreateTestEstudianteAsync(string adminToken)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createDto = new CreateEstudianteDto
        {
            Nombre = "Test Estudiante Para Pruebas"
        };

        var json = JsonSerializer.Serialize(createDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/estudiantes", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var estudiante = JsonSerializer.Deserialize<EstudianteDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return estudiante?.Id ?? throw new InvalidOperationException("No se pudo crear el estudiante de prueba");
    }

    #endregion
}