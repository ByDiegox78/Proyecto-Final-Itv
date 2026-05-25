using System.Text;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Config;
using ITV_Avanzado.Error.Citas;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Factory;
using ITV_Avanzado.Repository.Common;
using Serilog;

namespace ITV_Avanzado.Repository.Binary;

public class CitaBinaryRepository : ICitaRepository {
    private const string FilePath = "Data/vehiculos_sec.dat";
    private readonly ILogger _logger = Log.ForContext<CitaBinaryRepository>();
    private int _nextId;
    
    private readonly Dictionary<int, Cita> _porId;
    private readonly string _filePath;
    
    
    public CitaBinaryRepository(string filePath, bool dropData, bool seedData) {
        EnsureDataFolder();
        _filePath = filePath;
        if (dropData && File.Exists(_filePath)) {
            File.Delete(_filePath);
            _logger.Information("Archivo de datos borrado por solicitud (dropData)");
        }
        _porId = File.Exists(_filePath) ? Load() : new Dictionary<int, Cita>();
        if (seedData && _porId.Count == 0) {
            _logger.Debug("Sembrando datos iniciales en el repositorio binario");
            foreach (var p in CitasFactory.Seed()) {
                Create(p);
            }
        }
    } 
    private Dictionary<int, Cita> Load() {
        if (!File.Exists(_filePath)) {
            return new Dictionary<int, Cita>();
        }
        using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream, Encoding.UTF8);
        var cantidad = reader.ReadInt32();
        _nextId = reader.ReadInt32();
        var vehiculos = new Dictionary<int, Cita>();
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
            var vehiculo = new Cita{Id = id,Matricula = matricula, Marca = marca, Cilindrada = cilindrada, 
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
    public IEnumerable<Cita> GetAll(int page, int pageSize, bool includeDeleted, string? campoBusqueda) {
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

    public Cita? GetById(int id) {
        _logger.Debug("Buscando vehiculo por su matricula: {Id}", id);
        return _porId.GetValueOrDefault(id);
    }

    public Result<Cita, DomainError> Create(Cita cita) {
        _logger.Debug("Creando un vehiculo {Entity}", cita);
        if (!CupoVehiculosPorDia(cita.DniPropietario, cita.FechaInspeccion)) {
            _logger.Warning("El propietario con dni: {dni} tiene 3 vehiculos para inspeccion para el mismo dia", cita.DniPropietario);
            return Result.Failure<Cita, DomainError>(CitaErrors.MaxCitasUsageDniError(cita.DniPropietario));
        }
        if (ExisteCitaDuplicada(cita.Matricula,cita.FechaInspeccion)) {
            _logger.Warning("La matricula {Matriula} tiene una inspeccion resgistrada para hoy", cita.Matricula);
            return Result.Failure<Cita, DomainError>(CitaErrors.MatriculaInspeccionDuplicada(cita.Matricula));
        }
        var id = ++_nextId;
        var nuevo = cita with {
            Id = id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _porId[nuevo.Id] = nuevo;
        Save();
        _logger.Information("Cita creada con ID {Id}", nuevo.Id);
        return Result.Success<Cita, DomainError>(nuevo);
    }

    public Result<Cita, DomainError> Update(int id, Cita cita) {
        _logger.Debug("Actualizando el vehiculo: {Entity}", cita);
        if (!_porId.TryGetValue(id, out var actual)) {
            _logger.Warning("No se puede actualizar: vehiculo con id {Id} no encontrada", id);
            return Result.Failure<Cita, DomainError>(CitaErrors.NotFound(id.ToString()));
        }
        if (!CupoVehiculosPorDia(cita.DniPropietario, cita.FechaInspeccion, id)) {
            _logger.Warning("El propietario con dni: {dni} tiene 3 vehiculos para inspeccion para el mismo dia", cita.DniPropietario);
            return Result.Failure<Cita, DomainError>(CitaErrors.MaxCitasUsageDniError(cita.DniPropietario));
        }
        if (ExisteCitaDuplicada(cita.Matricula,cita.FechaInspeccion,id)) {
            _logger.Warning("La matricula {matriula} tiene una inspeccion resgistrada para hoy", cita.Matricula);
            return Result.Failure<Cita, DomainError>(CitaErrors.MatriculaInspeccionDuplicada(cita.Matricula));
        }
        var actualizado = cita with {
            Id = id,
            CreatedAt = actual.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _porId[id] = actualizado;
        _logger.Information("Cita con ID {Id} actualizada correctamente", id);
        Save();
        return Result.Success<Cita, DomainError>(actualizado);
        
    }

    public Cita? Delete(int id, bool isLogic = true) {
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

    public IEnumerable<Cita>? GetByMatricula(string matricula, int page = 1, int pageSize = 10) {
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
    public Result<Cita, DomainError> Restore(int id) {
        if (!_porId.TryGetValue(id, out var actual)) {
            _logger.Warning("No se puede restaurar: vehiculo con id {Id} no encontrada", id);
            return Result.Failure<Cita, DomainError>(CitaErrors.NotFound(id.ToString()));
        }

        var restore = actual with {
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
        };
        _porId[id] = restore;
        _logger.Information("Persona con ID {Id} restaurada correctamente", id);
        Save();
        return Result.Success<Cita, DomainError>(restore);
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