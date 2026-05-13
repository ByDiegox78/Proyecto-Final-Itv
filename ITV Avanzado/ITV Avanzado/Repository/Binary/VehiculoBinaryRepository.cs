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
    
    private readonly Dictionary<int, Vehiculo> _porId;
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
            var fechaMatriculacion = DateTime.Parse(reader.ReadString());
            var fechaInspeccion = DateTime.Parse(reader.ReadString());
            var isDelete = reader.ReadBoolean();
            var createAt = DateTime.Parse(reader.ReadString());
            var updateAt = DateTime.Parse(reader.ReadString());
            var vehiculo = new Vehiculo{Id = id,Matricula = matricula, Marca = marca, Cilindrada = cilindrada, 
                TipoMotor = motor,DniPropietario = dni,FechaMatriculacion = fechaMatriculacion,FechaInspeccion = 
                fechaInspeccion,IsDeleted = isDelete,CreatedAt = createAt,UpdatedAt = updateAt};
            vehiculos[id] = vehiculo;
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
            writer.Write(v.FechaMatriculacion.ToString("O"));
            writer.Write(v.FechaInspeccion.ToString("O"));
            writer.Write(v.IsDeleted);
            writer.Write(v.CreatedAt.ToString("O")); 
            writer.Write(v.UpdatedAt.ToString("O"));
            
        }
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
        var id = ++_nextId;
        var nuevo = vehiculo with {
            Id = id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _porId[nuevo.Id] = nuevo;
        Save();
        _logger.Information("Cita creada con ID {Id}", nuevo.Id);
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
        _nextId = 0;
        if (File.Exists(_filePath)) File.Delete(_filePath);
        _logger.Information("Repositorio BIN limpiado.");
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
        _logger.Information("Persona con ID {Id} restaurada correctamente", id);
        Save();
        return Result.Success<Vehiculo, DomainError>(restore);
    }
    private void EnsureDataFolder() {
        if (!Directory.Exists(AppConfig.DataFolder)) {
            Directory.CreateDirectory(AppConfig.DataFolder);
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
    } private bool ExisteCitaDuplicada(string matricula, DateTime fecha, int idActual = -1) {
        return _porId.Values.Any(v =>
            v.Matricula == matricula &&
            v.FechaInspeccion.Date == fecha.Date &&
            v.Id != idActual &&
            !v.IsDeleted
        );

    }
}