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

// Configure Serilog early with enhanced configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithCorrelationId()
    .Enrich.WithProperty("Application", "AudiSoft.School.Api")
    .Enrich.WithProperty("Version", typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0")
    .CreateLogger();

builder.Host.UseSerilog();

// Add early logging
Log.Information("Iniciando AudiSoft School API...");

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

// Ensure ILogger is available for services
builder.Services.AddLogging();

// Note: Serilog is configured as the host logger. Keep AddLogging for compatibility.
builder.Services.AddLogging();

var app = builder.Build();

// Add request logging middleware
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
    };
});

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

// Log application startup completion
Log.Information("AudiSoft School API iniciada correctamente en {Environment}", 
    app.Environment.EnvironmentName);

try
{
    Log.Information("Iniciando servidor web...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.Information("Cerrando aplicación...");
    Log.CloseAndFlush();
}
