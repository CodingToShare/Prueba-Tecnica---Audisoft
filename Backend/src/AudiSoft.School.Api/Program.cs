using AudiSoft.School.Api.Middleware;
using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Application.Mappings;
using AudiSoft.School.Application.Services;
using AudiSoft.School.Application.Validators;
using AudiSoft.School.Infrastructure.Persistence;
using AudiSoft.School.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/audisoft-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();

// Configure Swagger/OpenAPI (Swashbuckle) with full documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AudiSoft School API",
        Version = "v1.0",
        Description = "API REST para gestión de estudiantes, profesores y notas",
        Contact = new OpenApiContact
        {
            Name = "AudiSoft Team",
            Email = "support@audisoft.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT"
        }
    });

    // Incluir archivos XML de documentación
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register FluentValidation validators
builder.Services.AddScoped<IValidator<CreateEstudianteDto>, CreateEstudianteDtoValidator>();
builder.Services.AddScoped<IValidator<CreateProfesorDto>, CreateProfesorDtoValidator>();
builder.Services.AddScoped<IValidator<CreateNotaDto>, CreateNotaDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateNotaDto>, UpdateNotaDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateEstudianteDto>, UpdateEstudianteDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateProfesorDto>, UpdateProfesorDtoValidator>();

// Register DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AudiSoftSchoolDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.MigrationsAssembly("AudiSoft.School.Infrastructure")));

// Register Repositories
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddScoped<INotaRepository, NotaRepository>();

// Register Application Services
builder.Services.AddScoped<EstudianteService>();
builder.Services.AddScoped<ProfesorService>();
builder.Services.AddScoped<NotaService>();

// Note: Serilog is configured as the host logger. Keep AddLogging for compatibility.
builder.Services.AddLogging();

var app = builder.Build();

// Register and use exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AudiSoft School API v1.0");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
