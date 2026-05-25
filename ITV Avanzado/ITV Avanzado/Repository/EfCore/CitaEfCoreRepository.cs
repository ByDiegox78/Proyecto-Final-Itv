using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Entity;
using ITV_Avanzado.Error.Citas;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Factory;
using ITV_Avanzado.Mapper;
using ITV_Avanzado.Repository.Common;
using Serilog;

namespace ITV_Avanzado.Repository.EfCore;

public class CitaEfCoreRepository : ICitaRepository {
    
    private readonly AppDbContext _context;
    private readonly ILogger _logger = Log.ForContext<CitaEfCoreRepository>();

    public CitaEfCoreRepository(AppDbContext context, bool dropData = false, bool seedData = false) {
        _context = context;
        if (dropData) _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        if (seedData && !_context.Citas.Any()) {
            _logger.Information("Sembrando datos de Vehiculos...");
            foreach (var v in CitasFactory.Seed()) {
                Create(v);
            }
        }
    }
    
    public IEnumerable<Cita> GetAll(int page, int pageSize, bool includeDeleted, string? campoBusqueda) {
        var consulta = _context.Citas.AsQueryable();

        if (!includeDeleted) {
            consulta = consulta.Where(v => v.IsDeleted == false);
        }

        if (!string.IsNullOrWhiteSpace(campoBusqueda)) {
            consulta = consulta.Where(v => 
                v.Matricula.Contains(campoBusqueda) || 
                v.Marca.Contains(campoBusqueda) ||
                v.Dni.Contains(campoBusqueda));
        }

        return consulta
            .OrderBy(v => v.Id) 
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsEnumerable()
            .Select(e => e.ToModel()!);  
    }

    public Cita? GetById(int id) {
        try {
            var entity = _context.Citas.FirstOrDefault(p => p.Id == id);
            return entity.ToModel();
        }
        catch (Exception e) {
            _logger.Error(e, "Error al obtener vehiculo por ID {Id}", id);
            return null;
        }
    }

    public Result<Cita, DomainError> Create(Cita cita) {
        if (!CupoVehiculosPorDia(cita.DniPropietario, cita.FechaInspeccion)) {
            _logger.Warning("El propietario con dni: {dni} tiene 3 vehiculos para inspeccion para el mismo dia", cita.DniPropietario);
            return Result.Failure<Cita, DomainError>(CitaErrors.MaxCitasUsageDniError(cita.DniPropietario));
        }
        if (ExisteCitaDuplicada(cita.Matricula,cita.FechaInspeccion)) {
            _logger.Warning("La matricula {matriula} tiene una inspeccion resgistrada para hoy", cita.Matricula);
            return Result.Failure<Cita, DomainError>(CitaErrors.MatriculaInspeccionDuplicada(cita.Matricula));
        }
        cita = cita with {
            Id = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
        };
        try {
            var entity = cita.ToEntity();
            _context.Citas.Add(entity);
            _context.SaveChanges();
            return Result.Success<Cita, DomainError>(GetById(entity.Id)!);

        } catch (Exception e) {
            _logger.Error(e, "Error al crear Vehiculo");
            return Result.Failure<Cita, DomainError>(CitaErrors.DatabaseError(e.Message));
        }
    }

    public Result<Cita, DomainError> Update(int id, Cita cita) {
        var entity = _context.Citas.FirstOrDefault(P => P.Id == id);
        if (entity == null) {
            _logger.Warning("No se puede actualizar: vehiculo con id {Id} no encontrada", id);
            return Result.Failure<Cita, DomainError>(CitaErrors.NotFound(id.ToString()));
        }
        var existingModel = entity.ToModel();
        if (existingModel== null) {
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
        entity.Matricula = cita.Matricula;
        entity.Marca = cita.Marca;
        entity.Cilindrada = cita.Cilindrada;
        entity.Motor = (int)cita.TipoMotor;
        entity.Dni = cita.DniPropietario;
        entity.UpdatedAt = DateTime.UtcNow;
        
        try {
            _context.SaveChanges();
            return Result.Success<Cita, DomainError>(GetById(id)!);
        } catch (Exception e) {
            _logger.Error(e, "Error al actualizar vehiculo");
            return Result.Failure<Cita, DomainError>(CitaErrors.DatabaseError(e.Message));
        }
        
    }

    public Cita? Delete(int id, bool isLogic = true) {
        try {
            var entity = _context.Citas.FirstOrDefault(p => p.Id == id);
            if (entity == null)
                return null;
            if (isLogic) {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return GetById(id);
            }
            _context.Citas.Remove(entity);
            _context.SaveChanges();
            return entity.ToModel();
        }
        catch (Exception e) {
            _logger.Error(e, "Error al eliminar Vehiculo");
            return null;
        }
    }
    public IEnumerable<Cita>? GetByMatricula(string matricula, int page = 1, int pageSize = 10) {
        var list = _context.Citas
            .Where(c => c.Matricula == matricula && !c.IsDeleted)
            .OrderBy(v => v.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsEnumerable()
            .Select(e => e.ToModel()!)
            .ToList();
        return list;
    }
    public bool DeleteAll() {
        try {
            _context.Citas.RemoveRange(_context.Citas);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al eliminar todas los vehiculos");
            return false;
        }
    }
    public Result<Cita, DomainError> Restore(int id) {
        try {
            var entity = _context.Citas.FirstOrDefault(P => P.Id == id);
            if (entity == null) {
                _logger.Warning("No se puede actualizar: vehiculo con id {Id} no encontrada", id);
                return Result.Failure<Cita, DomainError>(CitaErrors.NotFound(id.ToString()));
            }
            entity.IsDeleted = false; 
            entity.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
            _logger.Information("Vehiculo con ID {Id} restaurado correctamente", id);
            return Result.Success<Cita, DomainError>(entity.ToModel()!);
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }
    private bool CupoVehiculosPorDia(string dni, DateTime fecha, int idActual = -1) {
        var count = _context.Citas.Count(v =>
            v.Dni == dni &&
            v.FechaInspeccion.Date == fecha.Date &&
            v.Id != idActual &&
            !v.IsDeleted
        );
        return count < 3;
    }
    private bool ExisteCitaDuplicada(string matricula, DateTime fecha, int idActual = -1) {
        return _context.Citas.Any(v =>
            v.Matricula == matricula &&
            v.FechaInspeccion.Date == fecha.Date &&
            v.Id != idActual &&
            !v.IsDeleted
        );
    }
}