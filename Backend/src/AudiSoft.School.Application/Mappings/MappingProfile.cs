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
        CreateMap<Nota, NotaDto>().ReverseMap();
        CreateMap<CreateNotaDto, Nota>();
    }
}
