using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Cache;
using ITV_Avanzado.Error.Citas;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Repository.Common;
using ITV_Avanzado.Validator.Common;
using Serilog;

namespace ITV_Avanzado.Service.Citas;

public class CitasService(
    ICitaRepository repository,
    IValidator<Cita> validator,
    ICache<int, Cita> cache
    ) : ICitasService {
    private readonly ILogger _logger = Log.ForContext<CitasService>();

    
    /// <inheritdoc cref="ICitasService.GetAll(int, int, bool, string)" />
    public IEnumerable<Cita> GetAll(int page = 1, int pageSize = 10, bool includeDeleted = true, string? campoBusqueda = null) {
        var res = repository.GetAll(page, pageSize, includeDeleted, campoBusqueda);
        return res;
    }
    /// <inheritdoc cref="ICitasService.GetById(int)" />
    public Result<Cita, DomainError> GetById(int id) {
        if (cache.Get(id) is { } cached) 
            return Result.Success<Cita, DomainError>(cached);
        if (repository.GetById(id) is { } v) {
            cache.Add(id, v);

            return Result.Success<Cita, DomainError>(v);
        }
        return Result.Failure<Cita, DomainError>(CitaErrors.NotFound(id.ToString()));
    }
    /// <inheritdoc cref="ICitasService.GetByMatricula(string)" />
    public Result<IEnumerable<Cita>, DomainError> GetByMatricula(string matricula) {
        if (repository.GetByMatricula(matricula) is { } v) {
            return Result.Success<IEnumerable<Cita>, DomainError>(v);
        }
        return Result.Failure<IEnumerable<Cita>, DomainError>(CitaErrors.NotFound(matricula));
    }
    /// <inheritdoc cref="ICitasService.Save(Cita)" />
    public Result<Cita, DomainError> Save(Cita cita) {
        return ValidarCita(cita)
            .Bind(v => repository.Create(v))
            .Tap(creada => cache.Add(creada.Id, creada));
    }
    /// <inheritdoc cref="ICitasService.Update(int, Cita)" />
    public Result<Cita, DomainError> Update(int id, Cita cita) {
        return CheckExists(id)
            .Tap(p => {
                cache.Remove(id);
            })
            .Bind(_ => ValidarCita(cita))
            .Bind(p => repository.Update(id, p));
    }
    /// <inheritdoc cref="ICitasService.Delete(int, bool)" />
    public Result<Cita, DomainError> Delete(int id, bool isLogical = true) {
        return CheckExists(id)
            .Tap(p => {
                cache.Remove(id);
            })
            .Map(p => repository.Delete(id, isLogical)!);
    }
    /// <inheritdoc cref="ICitasService.DeleteAll()" />
    public bool DeleteAll() {
        _logger.Warning("Eliminando todas las citas del sistema");
        return repository.DeleteAll();
    }
    /// <inheritdoc cref="ICitasService.Restore(int)" />
    public Result<Cita, DomainError> Restore(int id) {
        _logger.Information("Restaurando cita con ID {Id}", id);
        return repository.Restore(id);
    }
    /// <inheritdoc cref="ICitasService.GetCitasOrderBy(TipoOrdenamiento, int, int, bool)" />
    public IEnumerable<Cita> GetCitasOrderBy(TipoOrdenamiento ordenamiento, int page = 1, int pageSize = 10, bool includeDeleted = true) {
        var citas = repository.GetAll(1, int.MaxValue, includeDeleted, null!);
        var listaOrdenada = TipoDeOrdenamiento(citas, ordenamiento);
        return listaOrdenada
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }
    private Result<Cita, DomainError> ValidarCita(Cita cita) {
        _logger.Debug("Validando cita tipo");
        var val = validator.Validar(cita);
        return val.IsFailure 
            ? Result.Failure<Cita, DomainError>(val.Error) 
            : Result.Success<Cita, DomainError>(cita);
    }
    private Result<Cita, DomainError> CheckExists(int id) {
        return repository.GetById(id) is { } v
            ? Result.Success<Cita, DomainError>(v)
            : Result.Failure<Cita, DomainError>(CitaErrors.NotFound(id.ToString()));
    }

    private IEnumerable<Cita> TipoDeOrdenamiento(IEnumerable<Cita> lista, TipoOrdenamiento orden) {
        return orden switch {
            TipoOrdenamiento.Matricula => lista.OrderBy(c => c.Matricula),
            TipoOrdenamiento.Dni => lista.OrderBy(c => c.DniPropietario),
            TipoOrdenamiento.Marca => lista.OrderBy(c => c.Marca),
            TipoOrdenamiento.FechaItv => lista.OrderBy(c => c.FechaInspeccion),
            TipoOrdenamiento.Cilindrada => lista.OrderBy(c => c.Cilindrada),
            _ => lista.OrderBy(c => c.Id)
        };
    }
}