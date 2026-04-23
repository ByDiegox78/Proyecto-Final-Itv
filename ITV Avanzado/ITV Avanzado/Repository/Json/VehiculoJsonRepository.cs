using System.Text.Json;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Factory;
using ITV_Avanzado.Repository.Common;
using Serilog;

namespace ITV_Avanzado.Repository.Json;

public class VehiculoJsonRepository : IVehiculosRepository{
    private readonly ILogger _logger = Log.ForContext<VehiculoJsonRepository>();
    private readonly Dictionary<string, int> _matricula = new();
    private readonly Dictionary<int, Vehiculo> _porId = new();
    private readonly Dictionary<string, HashSet<int>> _porDni = new();

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

        if (seedData && _porId.Count == 0)
            foreach (var p in VehiculosFactory.Seed())
                Create(p);
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
        if (_matricula.ContainsKey(vehiculo.Matricula)) {
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaAlreadyExists("La matricula del vehiculo ya se en encuenta en uso"));
        }  
        if (!VerificarCochePropietario(vehiculo.DniPropietario)) {
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.DniAlreadyExists("El propietario no puede incluir este vehiculo porque ya tiene 3 a su disposicion"));
        }
        var nuevo = vehiculo with {
            Id = ++_idCounter,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _porId[nuevo.Id] = nuevo;
        _matricula[nuevo.Matricula] = nuevo.Id;
        AgregarVehiculoDni(nuevo.DniPropietario, nuevo.Id);
        Save();
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
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaAlreadyExists(vehiculo.Matricula));

        }
        if (vehiculo.DniPropietario != actual.DniPropietario) {
            if (!VerificarCochePropietario(vehiculo.DniPropietario)) {
                _logger.Warning("El propietario con DNI {Dni} ya tiene 3 vehículos", vehiculo.DniPropietario);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));
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
        Save();
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
        Save();
        return eliminado;
    }

    public Vehiculo? GetByMatricula(string matricula) {
        _logger.Debug("Buscando vehiculo con matricula: {matricula}", matricula);
        return _matricula.TryGetValue(matricula, out var id) &&
               _porId.TryGetValue(id, out var vehiculo) && !vehiculo.IsDeleted
            ? vehiculo
            : null; 
        
    }

    public bool DeleteAll() {
        _porId.Clear();
        _matricula.Clear(); 
        _porDni.Clear();
        _idCounter = 0;
        if (File.Exists(_filePath)) File.Delete(_filePath);
        
        _logger.Information("Repositorio JSON limpiado.");
        return true;
    }

    public Vehiculo? HardDelete(int id) {
        if (!_porId.TryGetValue(id, out var vehiculo)) {
            _logger.Warning("No se puede eliminar: vehiculo con id {Id} no encontrado", id);
            return null;
        }
        _porId.Remove(id); 
        _matricula.Remove(vehiculo.Matricula);
        QuitarVehiculoDni(vehiculo.DniPropietario, vehiculo.Id);
        Save();
        return vehiculo;    }

    public Result<Vehiculo, DomainError> Restore(int id) {
        if (!_porId.TryGetValue(id, out var actual)) {
            _logger.Warning("No se puede restaurar: vehiculo con id {Id} no encontrada", id);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
        }

        var restore = actual with {
            Id = actual.Id,
            Matricula = actual.Matricula,
            Marca = actual.Marca,
            Cilindrada = actual.Cilindrada,
            CreatedAt = actual.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            DniPropietario = actual.DniPropietario,
            IsDeleted = false,
            TipoMotor = actual.TipoMotor
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
                if (!v.IsDeleted) {
                    _matricula[v.Matricula] = v.Id;
                    AgregarVehiculoDni(v.DniPropietario, v.Id);
                }
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
    
    
    private bool VerificarCochePropietario(string dni) => 
        !_porDni.TryGetValue(dni, out var list) || list.Count < 3;
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