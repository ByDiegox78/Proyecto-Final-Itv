using System.Text;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Config;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Factory;
using ITV_Avanzado.Repository.Common;
using Serilog;

namespace ITV_Avanzado.Repository.Binary;

public class VehiculoBinaryRepository : IVehiculosRepository {
    private const string FilePath = "Data/vehiculos_sec.dat";
    private readonly ILogger _logger = Log.ForContext<VehiculoBinaryRepository>();

    private int _nextId;
    
    private readonly Dictionary<string, int> _matricula = new();
    private readonly Dictionary<int, Vehiculo> _porId;
    private readonly Dictionary<string, HashSet<int>> _porDni = new();
    
    private readonly string _filePath;
    
    public VehiculoBinaryRepository(string filePath, bool dropData, bool seedData) {
        EnsureDataFolder();
        _filePath = filePath;
        if (dropData && File.Exists(_filePath)) {
            File.Delete(_filePath);
            _logger.Information("Archivo de datos borrado por solicitud (dropData)");
        }
        _porId = File.Exists(_filePath) ? Load() : new Dictionary<int, Vehiculo>();
        if (seedData && _porId.Count == 0) {
            _logger.Debug("Sembrando datos iniciales en el repositorio binario");
            foreach (var p in VehiculosFactory.Seed()) {
                Create(p);
            }
        }
    }
    
   

    private Dictionary<int, Vehiculo> Load() {
        if (!File.Exists(_filePath)) {
            return new Dictionary<int, Vehiculo>();
        }

        using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        var cantidad = reader.ReadInt32();
        _nextId = reader.ReadInt32();

        var vehiculos = new Dictionary<int, Vehiculo>();

        for (int i = 0; i < cantidad; i++) {
            var id = reader.ReadInt32();
            var matricula = reader.ReadString();
            var marca = reader.ReadString();
            var cilindrada = reader.ReadInt32();
            var motor = (Motor)reader.ReadInt32();
            var dni = reader.ReadString();
            var isDelete = reader.ReadBoolean();
            var createAt = DateTime.Parse(reader.ReadString());
            var updateAt = DateTime.Parse(reader.ReadString());
            var vehiculo = new Vehiculo{Id = id,Matricula = matricula, Marca = marca, Cilindrada = cilindrada, 
                TipoMotor = motor,DniPropietario = dni,IsDeleted = isDelete,CreatedAt = createAt,UpdatedAt = updateAt};
            vehiculos[id] = vehiculo;
            _matricula[matricula] = id;
            AgregarVehiculoDni(dni, id);
        }

        return vehiculos;
    }

    private void Save() {
        using var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream, Encoding.UTF8);
        
        writer.Write(_porId.Count);
        writer.Write(_nextId);

        foreach (var v in _porId.Values) {
            writer.Write(v.Id);
            writer.Write(v.Matricula);
            writer.Write(v.Marca);
            writer.Write(v.Cilindrada);
            writer.Write((int)v.TipoMotor);
            writer.Write(v.DniPropietario);
            writer.Write(v.IsDeleted);
            writer.Write(v.CreatedAt.ToString("O")); 
            writer.Write(v.UpdatedAt.ToString("O"));
            
        }
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
            .Take(pageSize);    }

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

        var id = ++_nextId;
        var nuevo = vehiculo with {
            Id = id,
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
        _nextId = 0;
        if (File.Exists(_filePath)) File.Delete(_filePath);
        _logger.Information("Repositorio BIN limpiado.");
        return true;
    }

    public Vehiculo? HardDelete(int id) {
        if (!_porId.Remove(id, out var vehiculo)) return null;

        _matricula.Remove(vehiculo.Matricula);
        QuitarVehiculoDni(vehiculo.DniPropietario, vehiculo.Id);
        Save();
        return vehiculo;
        
    }

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
        _logger.Information("Persona con ID {Id} restaurada correctamente", id);
        Save();
        return Result.Success<Vehiculo, DomainError>(restore);
    }
    private void EnsureDataFolder() {
        if (!Directory.Exists(AppConfig.DataFolder)) {
            Directory.CreateDirectory(AppConfig.DataFolder);
        }
    }
}