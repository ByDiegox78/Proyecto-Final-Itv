using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Config;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Repository.Common;
using Serilog;

namespace ITV_Avanzado.Repository.Memory;

public class VehiculosRespositoryMemory : IVehiculosRepository{
    
    private readonly ILogger _logger = Log.ForContext<VehiculosRespositoryMemory>();
    private readonly Dictionary<string, int> _matricula = new();
    private readonly Dictionary<int, Vehiculo> _porId = new();
    private readonly Dictionary<string, HashSet<int>> _porDni = new();
    private int _idCounter;

    public VehiculosRespositoryMemory() : this(AppConfig.DropData, AppConfig.SeedData) { }
    
    public VehiculosRespositoryMemory(bool dropData, bool seedData) {
        if (dropData) {
            _logger.Warning("Borrando datos en memoria...");
            DeleteAll();
        }

        if (seedData) {
            _logger.Information("Cargando datos de semilla...");
            foreach (var persona in VehiculosFactory.Seed()) Create(persona);
            _logger.Information("SeedData completado.");
        }
    }
    
    public IEnumerable<Vehiculo> GetAll(int page = 1, int pageSize = 5, bool includeDeleted = true) {
        _logger.Debug(
            "Obteniendo vehiculos con paginación: página {Page}, tamaño {PageSize}, incluir borrados: {IncludeDeleted}",
            page, pageSize, includeDeleted);
        
        var query = includeDeleted
            ? _porId.Values.AsEnumerable()
            : _porId.Values.Where(e => !e.IsDeleted);

        return query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public Vehiculo? GetById(int id) {
        _logger.Debug("Buscando vehiculo por su matricula: {Id}", id);
        return _porId.GetValueOrDefault(id);

    }
    public Result<Vehiculo, DomainError> Create(Vehiculo vehiculo) {
        _logger.Debug("Creando un vehiculo {Entity}", vehiculo);
        if (_matricula.ContainsKey(vehiculo.Matricula) || !VerificarCochePropietario(vehiculo.DniPropietario)) 
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.DniAlreadyExists("El vehiculo ya existe o el propietario tiene 3 vehiculos a su disposicion"));
        
        var nuevo = vehiculo with {
            Id = ++_idCounter,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _porId[nuevo.Id] = nuevo;
        _matricula[nuevo.Matricula] = nuevo.Id;
        AgregarVehiculoDni(nuevo.DniPropietario, nuevo.Id);
        _logger.Information("Persona creada con ID {Id}", nuevo.Id);
        return Result.Success<Vehiculo, DomainError>(nuevo);
    }

    public Result<Vehiculo, DomainError> Update(int id, Vehiculo vehiculo) {
        _logger.Debug("Actualizando el vehiculo: {Entity}", vehiculo);
        if (!_porId.TryGetValue(id, out var actual)) {
            _logger.Warning("No se puede actualizar: vehiculo con id {Id} no encontrada", id);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
        }
        
        if (vehiculo.Matricula != actual.Matricula && _matricula.TryGetValue(vehiculo.Matricula, out var otroId) && otroId != id) {
            _logger.Warning("No se puede actualizar el vehículo con id {Id} porque la matrícula {Matricula} ya está en uso por otro vehículo",
                id, vehiculo.Matricula); 
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaAlreadyExists(id.ToString()));

        }
        if (vehiculo.DniPropietario != actual.DniPropietario) {
            if (!VerificarCochePropietario(vehiculo.DniPropietario)) {
                _logger.Warning("El propietario con DNI {Dni} ya tiene 3 vehículos", vehiculo.DniPropietario);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));;
            }
            QuitarVehiculoDni(actual.DniPropietario,actual.Id);
            AgregarVehiculoDni(vehiculo.DniPropietario, id);
        }
        var actualizado = vehiculo with {
            Id = id,
            CreatedAt = actual.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _porId[id] = actualizado;
        if (actual.Matricula != actualizado.Matricula) {
            _matricula.Remove(actual.Matricula);
            _matricula[actualizado.Matricula] = id;
        }
        _logger.Information("Persona con ID {Id} actualizada correctamente", id);
        return Result.Success<Vehiculo, DomainError>(actualizado);
    }

    public Vehiculo? Delete(int id) {
        _logger.Debug("Eliminando vehiculo con id {Id}", id);
        if (!_porId.TryGetValue(id, out var vehiculo)) {
            _logger.Warning("No se puede eliminar: vehiculo con id {Id} no encontrado", id);
            return null;
        }
        var eliminado =  vehiculo with {
            IsDeleted = true,
            UpdatedAt = DateTime.UtcNow
        };
        _porId[id] = eliminado;
        return eliminado;
    }
    public Vehiculo? HardDelete(int id) {
        if (!_porId.Remove(id, out var vehiculo)) {
            _logger.Warning("No se puede eliminar: vehiculo con id {Id} no encontrado", id);
            return null;
        }

        _matricula.Remove(vehiculo.Matricula);
        QuitarVehiculoDni(vehiculo.DniPropietario, vehiculo.Id);
    
        return vehiculo;
    }

    public Vehiculo? GetByMatricula(string matricula) {
        _logger.Debug("Buscando vehiculo con matricula: {matricula}", matricula);
        return _matricula.TryGetValue(matricula, out var id) &&
               _porId.TryGetValue(id, out var vehiculo) && !vehiculo.IsDeleted
            ? vehiculo
            : null;    
    }

    public bool DeleteAll() {
        _logger.Warning("Eliminando permanentemente todos los vehiculos");
        _matricula.Clear();
        _porId.Clear();
        _porDni.Clear();
        _idCounter = 0;
        return true;
    }
    
    private bool VerificarCochePropietario(string dni) { 
        return !_porDni.TryGetValue(dni, out var list) || list.Count < 3;
    }
    private void AgregarVehiculoDni(string dni, int id) {
        if (!_porDni.TryGetValue(dni, out var lista)) {
            lista = new HashSet<int>();
            _porDni[dni] = lista;
        }
        lista.Add(id);
    }
    private void QuitarVehiculoDni(string dni, int id) {
        if (_porDni.TryGetValue(dni, out var lista)) {
            lista.Remove(id);
            if (lista.Count == 0) _porDni.Remove(dni);
        }
    }
}