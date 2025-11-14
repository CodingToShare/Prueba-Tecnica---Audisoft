using AudiSoft.School.Api.Middleware;
using AudiSoft.School.Application.Configuration;
using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Application.Mappings;
using AudiSoft.School.Application.Services;
using AudiSoft.School.Application.Validators;
using AudiSoft.School.Infrastructure.Persistence;
using AudiSoft.School.Infrastructure.Repositories;
using AudiSoft.School.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

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
        Description = "API REST para gestión de estudiantes, profesores y notas con autenticación JWT",
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

    // Configuración de seguridad JWT para Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
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
builder.Services.AddScoped<IValidator<LoginRequestDto>, LoginRequestDtoValidator>();

// Register DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AudiSoftSchoolDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.MigrationsAssembly("AudiSoft.School.Infrastructure")));

// Register Repositories
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddScoped<INotaRepository, NotaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IUsuarioRolRepository, UsuarioRolRepository>();

// Register Application Services
builder.Services.AddScoped<EstudianteService>();
builder.Services.AddScoped<ProfesorService>();
builder.Services.AddScoped<NotaService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<RolService>();

// Ensure ILogger is available for services
builder.Services.AddLogging();

// Register Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT
builder.Services.Configure<JwtConfiguration>(
    builder.Configuration.GetSection(JwtConfiguration.SectionName));

// Configure JWT Authentication
var jwtConfig = builder.Configuration.GetSection(JwtConfiguration.SectionName).Get<JwtConfiguration>();
if (jwtConfig != null)
{
    jwtConfig.Validate(); // Validar configuración

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false; // Solo para desarrollo
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtConfig.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(jwtConfig.ClockSkewMinutes)
        };

        // Configurar logging para eventos de autenticación
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT authentication failed: {Exception}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst("userId")?.Value;
                Log.Debug("JWT token validated for user: {UserId}", userId);
                return Task.CompletedTask;
            }
        };
    });
}
else
{
    throw new InvalidOperationException("JWT configuration is missing or invalid");
}

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    // Política por defecto: requiere autenticación
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Políticas específicas por rol
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("ProfesorOnly", policy =>
        policy.RequireRole("Profesor"));

    options.AddPolicy("EstudianteOnly", policy =>
        policy.RequireRole("Estudiante"));

    options.AddPolicy("ProfesorOrAdmin", policy =>
        policy.RequireRole("Profesor", "Admin"));
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Log application startup completion
Log.Information("AudiSoft School API iniciada correctamente en {Environment}", 
    app.Environment.EnvironmentName);

// Seed data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AudiSoftSchoolDbContext>();
    
    try
    {
        Log.Information("Ejecutando seeder de datos iniciales...");
        await DataSeeder.SeedAsync(context);
        Log.Information("Seeder ejecutado correctamente");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Error al ejecutar seeder de datos iniciales");
    }
}

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
