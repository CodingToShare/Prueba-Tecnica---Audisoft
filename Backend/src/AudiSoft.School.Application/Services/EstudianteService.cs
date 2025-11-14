using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Extensions;
using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Application.Validators;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Domain.Exceptions;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Application.Services;

/// <summary>
/// Servicio de aplicación para Estudiantes.
/// Orquesta la lógica de negocio relacionada con estudiantes.
/// </summary>
public class EstudianteService
{
    private readonly IEstudianteRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateEstudianteDto> _validator;
    private readonly IValidator<UpdateEstudianteDto>? _updateValidator;

    public EstudianteService(
        IEstudianteRepository repository,
        IMapper mapper,
        IValidator<CreateEstudianteDto> validator,
        IValidator<UpdateEstudianteDto>? updateValidator = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Obtiene un estudiante por su ID.
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <returns>DTO del estudiante o null si no existe</returns>
    public async Task<EstudianteDto?> GetByIdAsync(int id)
    {
        var estudiante = await _repository.GetByIdAsync(id);
        return estudiante == null ? null : _mapper.Map<EstudianteDto>(estudiante);
    }

    /// <summary>
    /// Obtiene todos los estudiantes.
    /// </summary>
    /// <returns>Lista de DTOs de estudiantes</returns>
    public async Task<List<EstudianteDto>> GetAllAsync()
    {
        var estudiantes = await _repository.GetAllAsync();
        return _mapper.Map<List<EstudianteDto>>(estudiantes);
    }

    public async Task<PagedResult<EstudianteDto>> GetPagedAsync(QueryParams queryParams)
    {
        var query = _repository.Query().AsNoTracking().Cast<Estudiante>();
        query = query.ApplyFilter(queryParams.Filter, queryParams.FilterField, queryParams.FilterValue);
        query = query.ApplySorting(queryParams.SortField, queryParams.SortDesc);
        return await query.ApplyPagingAsync<Estudiante, EstudianteDto>(queryParams, e => _mapper.Map<EstudianteDto>(e));
    }

    /// <summary>
    /// Crea un nuevo estudiante.
    /// </summary>
    /// <param name="dto">DTO con datos del estudiante</param>
    /// <returns>DTO del estudiante creado</returns>
    /// <exception cref="ValidationException">Si los datos no son válidos</exception>
    public async Task<EstudianteDto> CreateAsync(CreateEstudianteDto dto)
    {
        // Validar entrada
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidEntityStateException($"Validación fallida: {errors}");
        }

        var estudiante = new Estudiante { Nombre = dto.Nombre };
        var created = await _repository.AddAsync(estudiante);
        return _mapper.Map<EstudianteDto>(created);
    }

    /// <summary>
    /// Actualiza un estudiante existente.
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <param name="dto">DTO con datos actualizados</param>
    /// <returns>DTO del estudiante actualizado</returns>
    /// <exception cref="EntityNotFoundException">Si el estudiante no existe</exception>
    /// <exception cref="ValidationException">Si los datos no son válidos</exception>
    public async Task<EstudianteDto> UpdateAsync(int id, UpdateEstudianteDto dto)
    {
        // Validar entrada
        if (_updateValidator != null)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new InvalidEntityStateException($"Validación fallida: {errors}");
            }
        }

        var estudiante = await _repository.GetByIdAsync(id);
        if (estudiante == null)
            throw new EntityNotFoundException(nameof(Estudiante), id);

        estudiante.Nombre = dto.Nombre;
        estudiante.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _repository.UpdateAsync(estudiante);
        return _mapper.Map<EstudianteDto>(updated);
    }

    /// <summary>
    /// Elimina (marca como eliminado) un estudiante.
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <exception cref="EntityNotFoundException">Si el estudiante no existe</exception>
    public async Task DeleteAsync(int id)
    {
        var estudiante = await _repository.GetByIdAsync(id);
        if (estudiante == null)
            throw new EntityNotFoundException(nameof(Estudiante), id);

        // Soft delete
        estudiante.IsDeleted = true;
        estudiante.DeletedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(estudiante);
    }
}

