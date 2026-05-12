using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Cache;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Repository.Common;
using ITV_Avanzado.Validator.Common;
using Serilog;

namespace ITV_Avanzado.Service.Citas;

public class CitasService(
    IVehiculosRepository repository,
    IValidator<Vehiculo> validator,
    ICache<int, Vehiculo> cache
    ) : ICitasService {
    private readonly ILogger _logger = Log.ForContext<CitasService>();

    
    public IEnumerable<Vehiculo> GetAll(int page = 1, int pageSize = 10, bool includeDeleted = true, string? campoBusqueda = null) {
        return repository.GetAll(page, pageSize, includeDeleted, campoBusqueda);
    }
    public Result<Vehiculo, DomainError> GetById(int id) {
        if (cache.Get(id) is { } cached) 
            return Result.Success<Vehiculo, DomainError>(cached);
        if (repository.GetById(id) is { } v) {
            cache.Add(id, v);

            return Result.Success<Vehiculo, DomainError>(v);
        }
        return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
    }
    public Result<IEnumerable<Vehiculo>, DomainError> GetByMatricula(string matricula) {
        if (repository.GetByMatricula(matricula) is { } v) {
            return Result.Success<IEnumerable<Vehiculo>, DomainError>(v);
        }
        return Result.Failure<IEnumerable<Vehiculo>, DomainError>(VehiculoErrors.NotFound(matricula));
    }
    public Result<Vehiculo, DomainError> Save(Vehiculo cita) {
        return ValidarCita(cita)
            .Ensure(v => !repository.GetByMatricula(v.Matricula).Any(x => x.FechaInspeccion.Date == v.FechaInspeccion.Date),
                v => VehiculoErrors.MatriculaInspeccionDuplicada(v.Matricula))
            .Ensure(v => repository.GetAll(1, int.MaxValue, false, null)
                    .Count(x => x.DniPropietario == v.DniPropietario && x.FechaInspeccion.Date == v.FechaInspeccion.Date) < 3,
                v => VehiculoErrors.MaxVehiculosUsageDniError(v.DniPropietario))
            .Bind(v => repository.Create(v))
            .Tap(creada => cache.Add(creada.Id, creada));
    }
    public Result<Vehiculo, DomainError> Update(int id, Vehiculo cita) {
        return CheckExists(id)
            .Tap(p => {
                cache.Remove(id);
            })
            .Bind(_ => ValidarCita(cita))
            .Ensure(
                v => !repository.GetByMatricula(v.Matricula).Any(x => x.FechaInspeccion.Date == v.FechaInspeccion.Date),
                v => VehiculoErrors.MatriculaInspeccionDuplicada(v.Matricula))
            .Ensure(v => repository.GetAll(1, int.MaxValue, false, null)
                    .Count(x => x.DniPropietario == v.DniPropietario &&
                                x.FechaInspeccion.Date == v.FechaInspeccion.Date) < 3,
                v => VehiculoErrors.MaxVehiculosUsageDniError(v.DniPropietario))
            .Bind(p => repository.Update(id, p));
    }
    public Result<Vehiculo, DomainError> Delete(int id, bool isLogical = true) {
        return CheckExists(id)
            .Tap(p => {
                cache.Remove(id);
            })
            .Map(p => repository.Delete(id, isLogical)!);
    }
    public bool DeleteAll() {
        _logger.Warning("Eliminando todas las citas del sistema");
        return repository.DeleteAll();
    }
    public Result<Vehiculo, DomainError> Restore(int id) {
        _logger.Information("Restaurando cita con ID {Id}", id);
        return repository.Restore(id);
    }
    public IEnumerable<Vehiculo> GetCitasOrderBy(TipoOrdenamiento ordenamiento, int page = 1, int pageSize = 10, bool includeDeleted = true) {
        var citas = repository.GetAll(1, int.MaxValue, includeDeleted, null);
        var listaOrdenada = TipoDeOrdenamiento(citas, ordenamiento);
        return listaOrdenada
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }
    private Result<Vehiculo, DomainError> ValidarCita(Vehiculo vehiculo) {
        _logger.Debug("Validando cita tipo");
        var val = validator.Validar(vehiculo);
        return val.IsFailure 
            ? Result.Failure<Vehiculo, DomainError>(val.Error) 
            : Result.Success<Vehiculo, DomainError>(vehiculo);
    }
    private Result<Vehiculo, DomainError> CheckExists(int id) {
        return repository.GetById(id) is { } v
            ? Result.Success<Vehiculo, DomainError>(v)
            : Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
    }

    private IEnumerable<Vehiculo> TipoDeOrdenamiento(IEnumerable<Vehiculo> lista, TipoOrdenamiento orden) {
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