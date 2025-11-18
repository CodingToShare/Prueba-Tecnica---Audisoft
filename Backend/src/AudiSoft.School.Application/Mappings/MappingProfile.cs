using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Domain.Entities;
using AutoMapper;

namespace AudiSoft.School.Application.Mappings;

/// <summary>
/// Perfil de AutoMapper para mapear entidades a DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Estudiante mappings
        CreateMap<Estudiante, EstudianteDto>().ReverseMap();
        CreateMap<CreateEstudianteDto, Estudiante>();

        // Profesor mappings
        CreateMap<Profesor, ProfesorDto>().ReverseMap();
        CreateMap<CreateProfesorDto, Profesor>();

        // Nota mappings
        CreateMap<Nota, NotaDto>()
            .ForMember(dest => dest.NombreProfesor, opt => opt.MapFrom(src => src.Profesor != null ? src.Profesor.Nombre : null))
            .ForMember(dest => dest.NombreEstudiante, opt => opt.MapFrom(src => src.Estudiante != null ? ExtractNombreSinGrado(src.Estudiante.Nombre) : null))
            .ForMember(dest => dest.Grado, opt => opt.MapFrom(src => src.Estudiante != null ? ExtractGrado(src.Estudiante.Nombre) : null))
            .ForMember(dest => dest.Materia, opt => opt.MapFrom(src => src.Nombre))
            .ReverseMap();
        CreateMap<CreateNotaDto, Nota>();

        // Usuario mappings
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.NombreProfesor, opt => opt.MapFrom(src => src.Profesor != null ? src.Profesor.Nombre : null))
            .ForMember(dest => dest.NombreEstudiante, opt => opt.MapFrom(src => src.Estudiante != null ? src.Estudiante.Nombre : null))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UsuarioRoles.Where(ur => !ur.IsDeleted).Select(ur => ur.Rol.Nombre).ToList()));
        CreateMap<CreateUsuarioDto, Usuario>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Se manejará en el servicio
            .ForMember(dest => dest.UsuarioRoles, opt => opt.Ignore()); // Se manejará en el servicio

        // Rol mappings
        CreateMap<Rol, RolDto>()
            .ForMember(dest => dest.UsuarioCount, opt => opt.MapFrom(src => src.UsuarioRoles.Count(ur => !ur.IsDeleted)));
        CreateMap<CreateRolDto, Rol>();
    }

    /// <summary>
    /// Extrae el grado del nombre del estudiante.
    /// Formato esperado: "Nombre - Grado°"
    /// Devuelve: "9°"
    /// </summary>
    private string? ExtractGrado(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return null;
        
        var match = System.Text.RegularExpressions.Regex.Match(nombre, @"-\s*(\d+)°?");
        if (match.Success)
        {
            var numero = match.Groups[1].Value;
            return numero + "°"; // Siempre devolver con el símbolo °
        }
        return null;
    }

    /// <summary>
    /// Extrae el nombre sin el grado.
    /// Formato esperado: "Nombre - Grado°"
    /// </summary>
    private string ExtractNombreSinGrado(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return nombre;
        
        var result = System.Text.RegularExpressions.Regex.Replace(nombre, @"\s*-\s*\d+°?$", "").Trim();
        return result;
    }
}
