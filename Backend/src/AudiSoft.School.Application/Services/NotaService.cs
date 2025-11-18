using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Application.Validators;
using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Domain.Exceptions;
using AutoMapper;
using FluentValidation;

namespace AudiSoft.School.Application.Services;

/// <summary>
/// Servicio de aplicación para Notas.
/// Orquesta la lógica de negocio relacionada con notas, incluyendo validaciones de referencias.
/// </summary>
public class NotaService
{
    private readonly INotaRepository _repository;
    private readonly IEstudianteRepository _estudianteRepository;
    private readonly IProfesorRepository _profesorRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateNotaDto> _validator;
    private readonly IValidator<UpdateNotaDto>? _updateValidator;

    public NotaService(
        INotaRepository repository,
        IEstudianteRepository estudianteRepository,
        IProfesorRepository profesorRepository,
        IMapper mapper,
        IValidator<CreateNotaDto> validator,
        IValidator<UpdateNotaDto>? updateValidator = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _estudianteRepository = estudianteRepository ?? throw new ArgumentNullException(nameof(estudianteRepository));
        _profesorRepository = profesorRepository ?? throw new ArgumentNullException(nameof(profesorRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Obtiene una nota por su ID.
    /// </summary>
    /// <param name="id">ID de la nota</param>
    /// <returns>DTO de la nota o null si no existe</returns>
    public async Task<NotaDto?> GetByIdAsync(int id)
    {
        var nota = await _repository.GetByIdAsync(id);
        return nota == null ? null : _mapper.Map<NotaDto>(nota);
    }

    /// <summary>
    /// Obtiene todas las notas.
    /// </summary>
    /// <returns>Lista de DTOs de notas</returns>
    public async Task<List<NotaDto>> GetAllAsync()
    {
        var notas = await _repository.GetAllAsync();
        return _mapper.Map<List<NotaDto>>(notas);
    }

    /// <summary>
    /// Obtiene notas con soporte de filtros, ordenamiento y paginación.
    /// </summary>
    public async Task<PagedResult<NotaDto>> GetPagedAsync(QueryParams queryParams)
    {
        var query = _repository.Query().AsNoTracking();
        // Include related entities for projection
        query = query.Include(n => n.Profesor).Include(n => n.Estudiante);

        query = query.ApplyFilter(queryParams.Filter, queryParams.FilterField, queryParams.FilterValue);
        query = query.ApplySorting(queryParams.SortField, queryParams.SortDesc);

        var paged = await query.ApplyPagingAsync<Nota, NotaDto>(queryParams, n => _mapper.Map<NotaDto>(n));
        return paged;
    }

    public async Task<PagedResult<NotaDto>> GetByProfesorPagedAsync(int idProfesor, QueryParams queryParams)
    {
        var query = _repository.Query().AsNoTracking();
        query = query.Where(n => n.IdProfesor == idProfesor);
        query = query.Include(n => n.Profesor).Include(n => n.Estudiante);
        query = query.ApplyFilter(queryParams.Filter, queryParams.FilterField, queryParams.FilterValue);
        query = query.ApplySorting(queryParams.SortField, queryParams.SortDesc);
        return await query.ApplyPagingAsync<Nota, NotaDto>(queryParams, n => _mapper.Map<NotaDto>(n));
    }

    public async Task<PagedResult<NotaDto>> GetByEstudiantePagedAsync(int idEstudiante, QueryParams queryParams)
    {
        var query = _repository.Query().AsNoTracking();
        query = query.Where(n => n.IdEstudiante == idEstudiante);
        query = query.Include(n => n.Profesor).Include(n => n.Estudiante);
        query = query.ApplyFilter(queryParams.Filter, queryParams.FilterField, queryParams.FilterValue);
        query = query.ApplySorting(queryParams.SortField, queryParams.SortDesc);
        return await query.ApplyPagingAsync<Nota, NotaDto>(queryParams, n => _mapper.Map<NotaDto>(n));
    }

    /// <summary>
    /// Obtiene notas eliminadas (soft delete) para auditoría.
    /// </summary>
    public async Task<PagedResult<NotaDto>> GetDeletedPagedAsync(QueryParams queryParams)
    {
        var query = _repository.Query().AsNoTracking();
        // Mostrar solo eliminadas - deshabilitar filtro global
        query = query.IgnoreQueryFilters().Where(n => n.IsDeleted);
        query = query.Include(n => n.Profesor).Include(n => n.Estudiante);
        query = query.ApplyFilter(queryParams.Filter, queryParams.FilterField, queryParams.FilterValue);
        query = query.ApplySorting(queryParams.SortField, queryParams.SortDesc);
        return await query.ApplyPagingAsync<Nota, NotaDto>(queryParams, n => _mapper.Map<NotaDto>(n));
    }

    /// <summary>
    /// Obtiene todas las notas de un profesor específico.
    /// </summary>
    /// <param name="idProfesor">ID del profesor</param>
    /// <returns>Lista de DTOs de notas del profesor</returns>
    public async Task<List<NotaDto>> GetByProfesorAsync(int idProfesor)
    {
        var notas = await _repository.GetByProfesorAsync(idProfesor);
        return _mapper.Map<List<NotaDto>>(notas);
    }

    /// <summary>
    /// Obtiene todas las notas de un estudiante específico.
    /// </summary>
    /// <param name="idEstudiante">ID del estudiante</param>
    /// <returns>Lista de DTOs de notas del estudiante</returns>
    public async Task<List<NotaDto>> GetByEstudianteAsync(int idEstudiante)
    {
        var notas = await _repository.GetByEstudianteAsync(idEstudiante);
        return _mapper.Map<List<NotaDto>>(notas);
    }

    /// <summary>
    /// Crea una nueva nota validando que existan el profesor y estudiante.
    /// </summary>
    /// <param name="dto">DTO con datos de la nota</param>
    /// <param name="createdBy">Usuario que crea la nota</param>
    /// <returns>DTO de la nota creada</returns>
    /// <exception cref="ValidationException">Si los datos no son válidos</exception>
    /// <exception cref="EntityNotFoundException">Si el profesor o estudiante no existen</exception>
    public async Task<NotaDto> CreateAsync(CreateNotaDto dto, string? createdBy = null)
    {
        // Validar entrada
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidEntityStateException($"Validación fallida: {errors}");
        }

        // Validar que el Profesor existe
        var profesor = await _profesorRepository.GetByIdAsync(dto.IdProfesor);
        if (profesor == null)
            throw new EntityNotFoundException(nameof(Profesor), dto.IdProfesor);

        // Validar que el Estudiante existe
        var estudiante = await _estudianteRepository.GetByIdAsync(dto.IdEstudiante);
        if (estudiante == null)
            throw new EntityNotFoundException(nameof(Estudiante), dto.IdEstudiante);

        var nota = new Nota
        {
            Nombre = dto.Nombre,
            Valor = dto.Valor,
            IdProfesor = dto.IdProfesor,
            IdEstudiante = dto.IdEstudiante,
            CreatedBy = createdBy
        };

        var created = await _repository.AddAsync(nota);
        return _mapper.Map<NotaDto>(created);
    }

    /// <summary>
    /// Actualiza una nota existente validando que existan el profesor y estudiante.
    /// </summary>
    /// <param name="id">ID de la nota</param>
    /// <param name="dto">DTO con datos actualizados</param>
    /// <param name="updatedBy">Usuario que actualiza la nota</param>
    /// <returns>DTO de la nota actualizada</returns>
    /// <exception cref="EntityNotFoundException">Si la nota, profesor o estudiante no existen</exception>
    /// <exception cref="ValidationException">Si los datos no son válidos</exception>
    public async Task<NotaDto> UpdateAsync(int id, UpdateNotaDto dto, string? updatedBy = null)
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

        var nota = await _repository.GetByIdAsync(id);
        if (nota == null)
            throw new EntityNotFoundException(nameof(Nota), id);

        // Validar que el Profesor existe
        var profesor = await _profesorRepository.GetByIdAsync(dto.IdProfesor);
        if (profesor == null)
            throw new EntityNotFoundException(nameof(Profesor), dto.IdProfesor);

        // Validar que el Estudiante existe
        var estudiante = await _estudianteRepository.GetByIdAsync(dto.IdEstudiante);
        if (estudiante == null)
            throw new EntityNotFoundException(nameof(Estudiante), dto.IdEstudiante);

        nota.Nombre = dto.Nombre;
        nota.Valor = dto.Valor;
        nota.IdProfesor = dto.IdProfesor;
        nota.IdEstudiante = dto.IdEstudiante;
        nota.UpdatedAt = DateTime.UtcNow;
        nota.UpdatedBy = updatedBy;

        var updated = await _repository.UpdateAsync(nota);
        return _mapper.Map<NotaDto>(updated);
    }

    /// <summary>
    /// Elimina (marca como eliminado) una nota.
    /// </summary>
    /// <param name="id">ID de la nota</param>
    /// <param name="deletedBy">Usuario que elimina la nota</param>
    /// <exception cref="EntityNotFoundException">Si la nota no existe</exception>
    public async Task DeleteAsync(int id, string? deletedBy = null)
    {
        var nota = await _repository.GetByIdAsync(id);
        if (nota == null)
            throw new EntityNotFoundException(nameof(Nota), id);

        // Soft delete
        nota.IsDeleted = true;
        nota.DeletedAt = DateTime.UtcNow;
        nota.DeletedBy = deletedBy;
        await _repository.UpdateAsync(nota);
    }
}
