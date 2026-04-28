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
    
    public IEnumerable<Vehiculo> GetAll(int page = 1, int pageSize = 5, bool includeDeleted = true) {
        try {
            var query = includeDeleted
                ? _context.Vehiculos.AsQueryable()
                : _context.Vehiculos.Where(p => !p.IsDeleted);

            var entities = query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return entities.ToModel();
        }
        catch (Exception e) {
            _logger.Error(e, "Error al obtener Vehiculos");
            return Enumerable.Empty<Vehiculo>();
        }    }

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
        if (_context.Vehiculos.Any(p => p.Matricula == vehiculo.Matricula)) {
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaAlreadyExists("La matricula del vehiculo ya se en encuenta en uso"));

        }
        if (!VerificarCochePropietario(vehiculo.DniPropietario)) {
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError("El propietario no puede incluir este vehiculo porque ya tiene 3 a su disposicion"));
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
        if (ExisteMatricula(vehiculo.Matricula)) {
            _logger.Warning("No se puede actualizar el vehículo con id {Id} porque la matrícula {Matricula} ya está en uso por otro vehículo",
                id, vehiculo.Matricula); 
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaAlreadyExists(vehiculo.Matricula));
        }
        if (!VerificarCochePropietario(vehiculo.DniPropietario))          {
            _logger.Warning("El propietario con DNI {Dni} ya tiene 3 vehículos", vehiculo.DniPropietario);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));
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

    public Vehiculo? Delete(int id) {
        try {
            var entity = _context.Vehiculos.FirstOrDefault(p => p.Id == id);
            if (entity == null)
                return null;
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
            return GetById(id);
        }
        catch (Exception e) {
            _logger.Error(e, "Error al eliminar Vehiculo");
            return null;
        }
    }

    public Vehiculo? GetByMatricula(string matricula) {
        return _context.Vehiculos.FirstOrDefault(v => v.Matricula == matricula).ToModel();
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

    public Vehiculo? HardDelete(int id) {
        try {
            var entity = _context.Vehiculos.FirstOrDefault(p => p.Id == id);
            if (entity == null)
                return null;
            _context.Vehiculos.Remove(entity);
            _context.SaveChanges();
            return entity.ToModel();
        } catch (Exception e) {
            _logger.Error(e, "Error al eliminar Vehiculo");
            return null;
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
    private bool VerificarCochePropietario(string dni) {
        try {
            return _context.Vehiculos.Count(v => v.Dni == dni & !v.IsDeleted) < 3;
        }
        catch (Exception e) {
            _logger.Error(e, "Error al verificar vehículos del propietario");
            return false;
        }
    }

    private bool ExisteMatricula(string matricula) {
        return _context.Vehiculos
            .Any(v => v.Matricula == matricula && !v.IsDeleted);
    }
}