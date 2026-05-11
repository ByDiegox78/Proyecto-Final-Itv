using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Entity;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Factory;
using ITV_Avanzado.Mapper;
using ITV_Avanzado.Repository.Common;
using Serilog;

namespace ITV_Avanzado.Repository.EfCore;

public class VehiculoEfCoreRepository : IVehiculosRepository {
    
    private readonly AppDbContext _context;
    private readonly ILogger _logger = Log.ForContext<VehiculoEfCoreRepository>();

    public VehiculoEfCoreRepository(AppDbContext context, bool dropData = false, bool seedData = false) {
        _context = context;
        if (dropData) _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        if (seedData && !_context.Vehiculos.Any()) {
            _logger.Information("Sembrando datos de Vehiculos...");
            foreach (var v in VehiculosFactory.Seed()) {
                Create(v);
            }
        }
    }
    
    public IEnumerable<Vehiculo> GetAll(int page, int pageSize, bool includeDeleted, string campoBusqueda) {
        var consulta = _context.Vehiculos.AsQueryable();

        if (!includeDeleted) {
            consulta = consulta.Where(v => v.IsDeleted == false);
        }

        if (!string.IsNullOrWhiteSpace(campoBusqueda)) {
            consulta = consulta.Where(v => 
                v.Matricula.Contains(campoBusqueda) || 
                v.Marca.Contains(campoBusqueda) ||
                v.Dni.Contains(campoBusqueda) ||
                v.Cilindrada.ToString().Contains(campoBusqueda)
            );
        }

        return consulta
            .OrderBy(v => v.Id) 
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsEnumerable()
            .Select(e => e.ToModel()!);  
    }

    public Vehiculo? GetById(int id) {
        try {
            var entity = _context.Vehiculos.FirstOrDefault(p => p.Id == id);
            return entity.ToModel();
        }
        catch (Exception e) {
            _logger.Error(e, "Error al obtener vehiculo por ID {Id}", id);
            return null;
        }
    }

    public Result<Vehiculo, DomainError> Create(Vehiculo vehiculo) {
        if (!CupoVehiculosPorDia(vehiculo.DniPropietario, vehiculo.FechaInspeccion)) {
            _logger.Warning("El propietario con dni: {dni} tiene 3 vehiculos para inspeccion para el mismo dia", vehiculo.DniPropietario);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));
        }
        if (ExisteCitaDuplicada(vehiculo.Matricula,vehiculo.FechaInspeccion)) {
            _logger.Warning("La matricula {matriula} tiene una inspeccion resgistrada para hoy", vehiculo.Matricula);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaInspeccionDuplicada(vehiculo.Matricula));
        }
        vehiculo = vehiculo with {
            Id = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
        };
        try {
            var entity = vehiculo.ToEntity();
            _context.Vehiculos.Add(entity);
            _context.SaveChanges();
            return Result.Success<Vehiculo, DomainError>(GetById(entity.Id)!);

        } catch (Exception e) {
            _logger.Error(e, "Error al crear Vehiculo");
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.DatabaseError(e.Message));
        }
    }

    public Result<Vehiculo, DomainError> Update(int id, Vehiculo vehiculo) {
        var entity = _context.Vehiculos.FirstOrDefault(P => P.Id == id);
        if (entity == null) {
            _logger.Warning("No se puede actualizar: vehiculo con id {Id} no encontrada", id);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
        }
        var existingModel = entity.ToModel();
        if (existingModel== null) {
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
        entity.Matricula = vehiculo.Matricula;
        entity.Marca = vehiculo.Marca;
        entity.Cilindrada = vehiculo.Cilindrada;
        entity.Motor = (int)vehiculo.TipoMotor;
        entity.Dni = vehiculo.DniPropietario;
        entity.UpdatedAt = DateTime.UtcNow;
        
        try {
            _context.SaveChanges();
            return Result.Success<Vehiculo, DomainError>(GetById(id)!);
        } catch (Exception e) {
            _logger.Error(e, "Error al actualizar vehiculo");
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.DatabaseError(e.Message));
        }
        
    }

    public Vehiculo? Delete(int id, bool isLogic = true) {
        try {
            var entity = _context.Vehiculos.FirstOrDefault(p => p.Id == id);
            if (entity == null)
                return null;
            if (isLogic) {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return GetById(id);
            }
            _context.Vehiculos.Remove(entity);
            _context.SaveChanges();
            return entity.ToModel();
        }
        catch (Exception e) {
            _logger.Error(e, "Error al eliminar Vehiculo");
            return null;
        }
    }
    public IEnumerable<Vehiculo>? GetByMatricula(string matricula) {
        var sql = _context.Vehiculos.Where(c => c.Matricula == matricula && !c.IsDeleted);
        var list = new List<Vehiculo>();
        foreach (var s in sql) {
            list.Add(s.ToModel());   
        }
        return list;
    }
    public bool DeleteAll() {
        try {
            _context.Vehiculos.RemoveRange(_context.Vehiculos);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al eliminar todas los vehiculos");
            return false;
        }
    }
    public Result<Vehiculo, DomainError> Restore(int id) {
        try {
            var entity = _context.Vehiculos.FirstOrDefault(P => P.Id == id);
            if (entity == null) {
                _logger.Warning("No se puede actualizar: vehiculo con id {Id} no encontrada", id);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
            }
            entity.IsDeleted = false; 
            entity.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
            _logger.Information("Vehiculo con ID {Id} restaurado correctamente", id);
            return Result.Success<Vehiculo, DomainError>(entity.ToModel()!);
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }
    private bool CupoVehiculosPorDia(string dni, DateTime fecha, int idActual = -1) {
        var count = _context.Vehiculos.Count(v =>
            v.Dni == dni &&
            v.FechaInspeccion.Date == fecha.Date &&
            v.Id != idActual &&
            !v.IsDeleted
        );
        return count < 3;
    }
    private bool ExisteCitaDuplicada(string matricula, DateTime fecha, int idActual = -1) {
        return _context.Vehiculos.Any(v =>
            v.Matricula == matricula &&
            v.FechaInspeccion.Date == fecha.Date &&
            v.Id != idActual &&
            !v.IsDeleted
        );
    }
}