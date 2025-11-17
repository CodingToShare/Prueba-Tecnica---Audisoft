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
/// Servicio de aplicación para Profesores.
/// Orquesta la lógica de negocio relacionada con profesores.
/// </summary>
public class ProfesorService
{
    private readonly IProfesorRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProfesorDto> _validator;
    private readonly IValidator<UpdateProfesorDto>? _updateValidator;

    public ProfesorService(
        IProfesorRepository repository,
        IMapper mapper,
        IValidator<CreateProfesorDto> validator,
        IValidator<UpdateProfesorDto>? updateValidator = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Obtiene un profesor por su ID.
    /// </summary>
    /// <param name="id">ID del profesor</param>
    /// <returns>DTO del profesor o null si no existe</returns>
    public async Task<ProfesorDto?> GetByIdAsync(int id)
    {
        var profesor = await _repository.GetByIdAsync(id);
        return profesor == null ? null : _mapper.Map<ProfesorDto>(profesor);
    }

    /// <summary>
    /// Obtiene todos los profesores.
    /// </summary>
    /// <returns>Lista de DTOs de profesores</returns>
    public async Task<List<ProfesorDto>> GetAllAsync()
    {
        var profesores = await _repository.GetAllAsync();
        return _mapper.Map<List<ProfesorDto>>(profesores);
    }

    public async Task<PagedResult<ProfesorDto>> GetPagedAsync(QueryParams queryParams)
    {
        var query = _repository.Query().AsNoTracking().Cast<Profesor>();
        query = query.ApplyFilter(queryParams.Filter, queryParams.FilterField, queryParams.FilterValue);
        query = query.ApplySorting(queryParams.SortField, queryParams.SortDesc);
        return await query.ApplyPagingAsync<Profesor, ProfesorDto>(queryParams, p => _mapper.Map<ProfesorDto>(p));
    }

    /// <summary>
    /// Crea un nuevo profesor.
    /// </summary>
    /// <param name="dto">DTO con datos del profesor</param>
    /// <param name="createdBy">Usuario que crea el profesor</param>
    /// <returns>DTO del profesor creado</returns>
    /// <exception cref="ValidationException">Si los datos no son válidos</exception>
    public async Task<ProfesorDto> CreateAsync(CreateProfesorDto dto, string? createdBy = null)
    {
        // Validar entrada
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidEntityStateException($"Validación fallida: {errors}");
        }

        var profesor = new Profesor 
        { 
            Nombre = dto.Nombre,
            CreatedBy = createdBy
        };
        var created = await _repository.AddAsync(profesor);
        return _mapper.Map<ProfesorDto>(created);
    }

    /// <summary>
    /// Actualiza un profesor existente.
    /// </summary>
    /// <param name="id">ID del profesor</param>
    /// <param name="dto">DTO con datos actualizados</param>
    /// <param name="updatedBy">Usuario que actualiza el profesor</param>
    /// <returns>DTO del profesor actualizado</returns>
    /// <exception cref="EntityNotFoundException">Si el profesor no existe</exception>
    /// <exception cref="ValidationException">Si los datos no son válidos</exception>
    public async Task<ProfesorDto> UpdateAsync(int id, UpdateProfesorDto dto, string? updatedBy = null)
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

        var profesor = await _repository.GetByIdAsync(id);
        if (profesor == null)
            throw new EntityNotFoundException(nameof(Profesor), id);

        profesor.Nombre = dto.Nombre;
        profesor.UpdatedAt = DateTime.UtcNow;
        profesor.UpdatedBy = updatedBy;
        
        var updated = await _repository.UpdateAsync(profesor);
        return _mapper.Map<ProfesorDto>(updated);
    }

    /// <summary>
    /// Elimina (marca como eliminado) un profesor.
    /// </summary>
    /// <param name="id">ID del profesor</param>
    /// <param name="deletedBy">Usuario que elimina el profesor</param>
    /// <exception cref="EntityNotFoundException">Si el profesor no existe</exception>
    public async Task DeleteAsync(int id, string? deletedBy = null)
    {
        var profesor = await _repository.GetByIdAsync(id);
        if (profesor == null)
            throw new EntityNotFoundException(nameof(Profesor), id);

        // Soft delete
        profesor.IsDeleted = true;
        profesor.DeletedAt = DateTime.UtcNow;
        profesor.DeletedBy = deletedBy;
        await _repository.UpdateAsync(profesor);
    }
}
