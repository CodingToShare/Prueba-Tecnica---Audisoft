using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.Extensions;
using AudiSoft.School.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AudiSoft.School.Tests.Unit.Extensions;

/// <summary>
/// Pruebas unitarias para IQueryableExtensions
/// </summary>
public class IQueryableExtensionsTests
{
    /// <summary>
    /// Crea un conjunto de datos de prueba para las extensiones IQueryable
    /// </summary>
    private IQueryable<TestEntity> CreateTestQuery()
    {
        return new List<TestEntity>
        {
            new() { Id = 1, Nombre = "Juan Pérez", Valor = 10.5m, Fecha = DateTime.Parse("2023-01-01") },
            new() { Id = 2, Nombre = "María García", Valor = 25.0m, Fecha = DateTime.Parse("2023-02-01") },
            new() { Id = 3, Nombre = "Carlos López", Valor = 15.75m, Fecha = DateTime.Parse("2023-03-01") },
            new() { Id = 4, Nombre = "Ana Martín", Valor = 30.25m, Fecha = DateTime.Parse("2023-04-01") },
            new() { Id = 5, Nombre = "Luis González", Valor = 5.5m, Fecha = DateTime.Parse("2023-05-01") }
        }.AsQueryable();
    }

    [Fact]
    public void ApplyFilter_WithSimpleFieldAndValue_ShouldFilterCorrectly()
    {
        // Arrange
        var query = CreateTestQuery();

        // Act
        var result = query.ApplyFilter(null, "Nombre", "Juan").ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Nombre.Should().Be("Juan Pérez");
    }

    [Fact]
    public void ApplyFilter_WithAdvancedExpression_ShouldFilterCorrectly()
    {
        // Arrange
        var query = CreateTestQuery();

        // Act - Buscar por nombre que contenga "ar" O valor mayor a 25
        var result = query.ApplyFilter("Nombre:ar|Valor>25", null, null).ToList();

        // Assert
        result.Should().HaveCount(3); // María (ar), Carlos (ar), Ana (>25)
        result.Should().Contain(x => x.Nombre == "María García");
        result.Should().Contain(x => x.Nombre == "Carlos López");
        result.Should().Contain(x => x.Nombre == "Ana Martín");
    }

    [Fact]
    public void ApplyFilter_WithAndConditions_ShouldFilterCorrectly()
    {
        // Arrange
        var query = CreateTestQuery();

        // Act - Buscar nombre que contenga "ar" Y valor menor a 20
        var result = query.ApplyFilter("Nombre:ar;Valor<20", null, null).ToList();

        // Assert
        result.Should().HaveCount(1); // Solo Carlos López
        result[0].Nombre.Should().Be("Carlos López");
    }

    [Fact]
    public void ApplyFilter_WithNumericComparison_ShouldFilterCorrectly()
    {
        // Arrange
        var query = CreateTestQuery();

        // Act
        var result = query.ApplyFilter("Valor>=15", null, null).ToList();

        // Assert
        result.Should().HaveCount(3); // María, Carlos, Ana
        result.All(x => x.Valor >= 15).Should().BeTrue();
    }

    [Fact]
    public void ApplyFilter_WithComplexOrAndConditions_ShouldFilterCorrectly()
    {
        // Arrange
        var query = CreateTestQuery();

        // Act - (Nombre contiene Juan Y Valor > 5) O (Nombre contiene Ana)
        var result = query.ApplyFilter("Nombre:Juan;Valor>5|Nombre:Ana", null, null).ToList();

        // Assert
        result.Should().HaveCount(2); // Juan y Ana
        result.Should().Contain(x => x.Nombre == "Juan Pérez");
        result.Should().Contain(x => x.Nombre == "Ana Martín");
    }

    [Fact]
    public void ApplySorting_WithValidField_ShouldSortCorrectly()
    {
        // Arrange
        var query = CreateTestQuery();

        // Act - Ordenar por nombre ascendente
        var result = query.ApplySorting("Nombre", false).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Nombre.Should().Be("Ana Martín");
        result[1].Nombre.Should().Be("Carlos López");
        result[2].Nombre.Should().Be("Juan Pérez");
        result[3].Nombre.Should().Be("Luis González");
        result[4].Nombre.Should().Be("María García");
    }

    [Fact]
    public void ApplySorting_WithDescendingOrder_ShouldSortCorrectly()
    {
        // Arrange
        var query = CreateTestQuery();

        // Act - Ordenar por ID descendente
        var result = query.ApplySorting("Id", true).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Id.Should().Be(5);
        result[1].Id.Should().Be(4);
        result[2].Id.Should().Be(3);
        result[3].Id.Should().Be(2);
        result[4].Id.Should().Be(1);
    }

    [Fact]
    public async Task ApplyPagingAsync_WithValidParams_ShouldPaginateCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedDatabase(context);

        var query = context.TestEntities.AsQueryable();
        var queryParams = new QueryParams { Page = 2, PageSize = 2 };

        // Act
        var result = await query.ApplyPagingAsync(queryParams, x => new TestEntityDto 
        { 
            Id = x.Id, 
            Nombre = x.Nombre, 
            Valor = x.Valor 
        });

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ApplyPagingAsync_WithFirstPage_ShouldReturnFirstItems()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedDatabase(context);

        var query = context.TestEntities.OrderBy(x => x.Id);
        var queryParams = new QueryParams { Page = 1, PageSize = 3 };

        // Act
        var result = await query.ApplyPagingAsync(queryParams, x => new TestEntityDto 
        { 
            Id = x.Id, 
            Nombre = x.Nombre, 
            Valor = x.Valor 
        });

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(3);
        result.Items.Should().HaveCount(3);
        var itemsList = result.Items.ToList();
        itemsList[0].Id.Should().Be(1);
        itemsList[1].Id.Should().Be(2);
        itemsList[2].Id.Should().Be(3);
    }

    [Fact]
    public async Task ApplyPagingAsync_WithLargePage_ShouldLimitPageSize()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedDatabase(context);

        var query = context.TestEntities.AsQueryable();
        var queryParams = new QueryParams 
        { 
            Page = 1, 
            PageSize = 1000, // Muy grande
            MaxPageSize = 100 // Límite
        };

        // Act
        var result = await query.ApplyPagingAsync(queryParams, x => new TestEntityDto 
        { 
            Id = x.Id, 
            Nombre = x.Nombre, 
            Valor = x.Valor 
        });

        // Assert
        result.Should().NotBeNull();
        result.PageSize.Should().Be(100); // Debería estar limitado
        result.Items.Should().HaveCount(5); // Todos los elementos disponibles
    }

    #region Helper Methods

    /// <summary>
    /// Crea un contexto de base de datos en memoria para las pruebas
    /// </summary>
    private TestDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    /// <summary>
    /// Inicializa la base de datos con datos de prueba
    /// </summary>
    private async Task SeedDatabase(TestDbContext context)
    {
        await context.TestEntities.AddRangeAsync(CreateTestQuery().ToList());
        await context.SaveChangesAsync();
    }

    #endregion
}

#region Clases de Apoyo

/// <summary>
/// Entidad de prueba para testear las extensiones IQueryable
/// </summary>
public class TestEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Fecha { get; set; }
}

/// <summary>
/// DTO de prueba para proyecciones
/// </summary>
public class TestEntityDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

/// <summary>
/// Contexto de base de datos para las pruebas
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    
    public DbSet<TestEntity> TestEntities { get; set; }
}

#endregion