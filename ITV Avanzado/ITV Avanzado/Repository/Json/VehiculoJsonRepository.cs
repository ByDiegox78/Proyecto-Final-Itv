using System.Text.Json;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Config;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Factory;
using ITV_Avanzado.Repository.Common;
using Serilog;

namespace ITV_Avanzado.Repository.Json;

public class VehiculoJsonRepository : IVehiculosRepository{
    private readonly ILogger _logger = Log.ForContext<VehiculoJsonRepository>();
    private readonly Dictionary<int, Vehiculo> _porId = new();
    private int _idCounter;
    
    private readonly JsonSerializerOptions _jsonOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    private readonly string _filePath;
    
    public VehiculoJsonRepository(string filePath, bool dropData, bool seedData) {
        _filePath = filePath;
        EnsureDirectory();
        if (dropData && File.Exists(_filePath)) File.Delete(_filePath);
        if (File.Exists(_filePath)) Load();
        if (!seedData || _porId.Count != 0) return;
        foreach (var p in VehiculosFactory.Seed())
            Create(p);
    }
    public IEnumerable<Vehiculo> GetAll(int page, int pageSize, bool includeDeleted, string? campoBusqueda) {
        _logger.Debug(
            "Obteniendo vehiculos con paginación: página {Page}, tamaño {PageSize}, incluir borrados: {IncludeDeleted}",
            page, pageSize, includeDeleted);
        
        var consulta = _porId.Values.AsEnumerable();

        if (!includeDeleted)
            consulta = _porId.Select(e => e.Value)
                .Where(v => v.IsDeleted == false);
        

        if (!string.IsNullOrWhiteSpace(campoBusqueda)) {
            consulta = consulta.Where(v => 
                v.Matricula.Contains(campoBusqueda, StringComparison.OrdinalIgnoreCase) || 
                v.Marca.Contains(campoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                v.DniPropietario.Contains(campoBusqueda, StringComparison.OrdinalIgnoreCase)
            );
        }
        return consulta
            .OrderBy(v => v.Id) 
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public Vehiculo? GetById(int id) {
        _logger.Debug("Buscando vehiculo por su matricula: {Id}", id);
        return _porId.GetValueOrDefault(id);
    }

    public Result<Vehiculo, DomainError> Create(Vehiculo vehiculo) {
        _logger.Debug("Creando un vehiculo {Entity}", vehiculo);
        if (!CupoVehiculosPorDia(vehiculo.DniPropietario, vehiculo.FechaInspeccion)) {
            _logger.Warning("El propietario con dni: {dni} tiene 3 vehiculos para inspeccion para el mismo dia", vehiculo.DniPropietario);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));
        }
        if (ExisteCitaDuplicada(vehiculo.Matricula,vehiculo.FechaInspeccion)) {
            _logger.Warning("La matricula {Matriula} tiene una inspeccion resgistrada para hoy", vehiculo.Matricula);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaInspeccionDuplicada(vehiculo.Matricula));
        }
        var nuevo = vehiculo with {
            Id = ++_idCounter,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _porId[nuevo.Id] = nuevo;
        _logger.Information("Cita creada con ID {Id}", nuevo.Id);
        Save();
        return Result.Success<Vehiculo, DomainError>(nuevo);
    }

    public Result<Vehiculo, DomainError> Update(int id, Vehiculo vehiculo) {
        _logger.Debug("Actualizando el vehiculo: {Entity}", vehiculo);
        if (!_porId.TryGetValue(id, out var actual)) {
            _logger.Warning("No se puede actualizar: vehiculo con id {Id} no encontrada", id);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
        }
        if (!CupoVehiculosPorDia(vehiculo.DniPropietario, vehiculo.FechaInspeccion, id)) {
            _logger.Warning("El propietario con dni: {dni} tiene 3 vehiculos para inspeccion para el mismo dia", vehiculo.DniPropietario);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));
        }
        if (ExisteCitaDuplicada(vehiculo.Matricula,vehiculo.FechaInspeccion,id)) {
            _logger.Warning("La matricula {matriula} tiene una inspeccion resgistrada para hoy", vehiculo.Matricula);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaInspeccionDuplicada(vehiculo.Matricula));
        }
        var actualizado = vehiculo with {
            Id = id,
            CreatedAt = actual.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _porId[id] = actualizado;
        _logger.Information("Cita con ID {Id} actualizada correctamente", id);
        Save();
        return Result.Success<Vehiculo, DomainError>(actualizado);
    }

    public Vehiculo? Delete(int id, bool isLogic = true) {
        _logger.Debug("Eliminando vehiculo con id {Id}", id);
        if (!_porId.TryGetValue(id, out var vehiculo)) {
            _logger.Warning("No se puede eliminar: vehiculo con id {Id} no encontrado", id);
            return null;
        }
        if (isLogic) {
            var eliminado =  vehiculo with {
                IsDeleted = true,
                UpdatedAt = DateTime.UtcNow
            };
            _porId[id] = eliminado;
            Save();
            return eliminado;
        }
        _porId.Remove(id); 
        Save();
        return vehiculo;
    }

    public IEnumerable<Vehiculo>? GetByMatricula(string matricula, int page = 1, int pageSize = 10) {
        _logger.Debug("Buscando citas con matricula: {matricula}", matricula);
        return _porId.Values.Where(c => c.Matricula == matricula && !c.IsDeleted)
            .OrderBy(v => v.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    } 
    public bool DeleteAll() {
        _porId.Clear();
        _idCounter = 0;
        if (File.Exists(_filePath)) File.Delete(_filePath);
        
        _logger.Information("Repositorio JSON limpiado.");
        return true;
    }
    public Result<Vehiculo, DomainError> Restore(int id) {
        if (!_porId.TryGetValue(id, out var actual)) {
            _logger.Warning("No se puede restaurar: vehiculo con id {Id} no encontrada", id);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
        }
        var restore = actual with {
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
        };
        _porId[id] = restore;
        Save();
        _logger.Information("Persona con ID {Id} restaurada correctamente", id);
        return Result.Success<Vehiculo, DomainError>(restore);
    }
    
    private void Load() {
        try {
            if (!File.Exists(_filePath)) {
                _logger.Information("El archivo Json no existe.");
                return;
            }
            var json = File.ReadAllText(_filePath);
            var vehiculos = JsonSerializer.Deserialize<List<Vehiculo>>(json, _jsonOptions);
            if (vehiculos == null) return;
            foreach (var v in vehiculos) {
                _porId[v.Id] = v;
                if (v.Id > _idCounter) _idCounter = v.Id;
            }
        }
        catch (Exception e) {
            _logger.Error(e, "Error al cargar el archivo JSON.");

        }
    }
    private void EnsureDirectory() {
        var dir = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrEmpty(dir) || Directory.Exists(dir)) return;
        _logger.Debug("Creando directorio: {Dir}", dir);
        Directory.CreateDirectory(dir);
    }
    private void Save() {
        try {
            var vehiculos = _porId.Values.ToList();
            var json = JsonSerializer.Serialize(vehiculos, _jsonOptions);
            File.WriteAllText(_filePath,json);
            _logger.Debug("Datos en el json: ", vehiculos.Count);
        }
        catch (Exception e) {
            _logger.Error(e, "Error de guardado");
            throw;
        }
    } 
    private bool CupoVehiculosPorDia(string dni, DateTime fecha, int idActual = -1) {
        var count = _porId.Values.Count(v =>
            v.DniPropietario == dni &&
            v.FechaInspeccion.Date == fecha.Date &&
            v.Id != idActual &&
            !v.IsDeleted
        );
        return count < 3;
    }
       
        
    private bool ExisteCitaDuplicada(string matricula, DateTime fecha, int idActual = -1) {
        return _porId.Values.Any(v =>
            v.Matricula == matricula &&
            v.FechaInspeccion.Date == fecha.Date &&
            v.Id != idActual &&
            !v.IsDeleted
        );

    }
}