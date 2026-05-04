using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using GestionItv.Models;
using ITV_Avanzado.Entity;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Factory;
using ITV_Avanzado.Mapper;
using ITV_Avanzado.Repository.Common;
using Serilog;
using Microsoft.Data.Sqlite;


namespace ITV_Avanzado.Repository.Dapper;

public class VehiculoDapperRepository : IVehiculosRepository {
    private readonly IDbConnection _connection;
    private readonly ILogger _logger = Log.ForContext<VehiculoDapperRepository>();
    private readonly Action? _onDispose;
    
    public VehiculoDapperRepository(IDbConnection connection, Action? onDispose = null, bool dropData = false,
        bool seedData = false) {
        _connection = connection;
        _onDispose = onDispose;
        EnsureTable(dropData);

        if (seedData && CountTotal() == 0) Seed();
    }
    
   
    public IEnumerable<Vehiculo> GetAll(int page = 1, int pageSize = 5, bool includeDeleted = true) {
        try {
            var sql = includeDeleted
                ? "SELECT * FROM Vehiculos ORDER BY Id LIMIT @PageSize OFFSET @Offset"
                : "SELECT * FROM Vehiculos WHERE IsDeleted = 0 ORDER BY Id LIMIT @PageSize OFFSET @Offset";
            var entities = _connection
                .Query<VehiculoEntity>(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize }).ToList();
            return entities.Select(VehiculoMapper.ToModel).OfType<Vehiculo>().ToList();
        }
        catch (Exception e) {
            _logger.Error(e, "Error al obtener Vehiculos");
            return [];
        }
    }

    public Vehiculo? GetById(int id) {
        try {
            var sql = "SELECT * FROM Vehiculos WHERE Id = @Id";
            var entities = _connection.QueryFirstOrDefault<VehiculoEntity>(sql, new { Id = id });
            return entities.ToModel();
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
        var entity = vehiculo.ToEntity();
        try {
            var sql = @"
            INSERT INTO Vehiculos (Matricula, Marca, Cilindrada, Motor, Dni, FechaMatriculacion, FechaInspeccion, IsDeleted, CreatedAt, UpdatedAt)
            VALUES 
            (@Matricula, @Marca, @Cilindrada, @Motor, @Dni, @FechaMatriculacion, @FechaInspeccion, @IsDeleted, @CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid()";
            entity.Id = _connection.ExecuteScalar<int>(sql, entity);
            return Result.Success<Vehiculo, DomainError>(GetById(entity.Id)!);
        }
        catch (Exception e) {
            _logger.Error(e, "Error al crear vehiculo");
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.DatabaseError(e.Message));
        }
    }

    public Result<Vehiculo, DomainError> Update(int id, Vehiculo vehiculo) {
        var existing = GetById(id);
        if (existing == null) {
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
        
        vehiculo = vehiculo with {
            Id = id,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = existing.IsDeleted
        };
        var entity = vehiculo.ToEntity();
        try {
            var sql = @"UPDATE Vehiculos SET Matricula = @Matricula,  Marca = @Marca,Cilindrada = @Cilindrada,Motor = @Motor,
            Dni = @Dni, IsDeleted = @IsDeleted, CreatedAt = @CreatedAt, UpdatedAt = @UpdatedAt
            WHERE Id = @Id;";
            _connection.Execute(sql, entity);
            return Result.Success<Vehiculo, DomainError>(GetById(id)!);

        }
        catch (Exception e) {
            return Result.Failure<Vehiculo, DomainError>(
                VehiculoErrors.DatabaseError("Error al actualizar el vehículo"));
        }
    }

    public Vehiculo? Delete(int id, bool isLogic = true) {
        try {
            var existing = GetById(id);
            if (existing == null)
                return null;
            if (isLogic) {
                var sql = "UPDATE Vehiculos SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";            
                _connection.Execute(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
                return GetById(id);
            }
            var sqlHard = "DELETE FROM Vehiculos WHERE Id = @Id";
            _connection.Execute(sqlHard, new { Id = id });
            return existing;
        }
        catch (Exception e) {
            _logger.Error(e, "Error al eliminar vehiculo");
            return null;
        }
    }
    public IEnumerable<Vehiculo>? GetByMatricula(string matricula) {
        try {
            var sql = "SELECT * FROM Vehiculos WHERE Matricula = @Matricula AND IsDeleted = 0";
            var entity = _connection.Query<VehiculoEntity>(sql, new { Matricula = matricula });
            return entity.ToModel();
            
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al obtener vehiculo por Matricula {Matricula}", matricula);
            return null;
        }
    }
    public bool DeleteAll() {
        try {
            _connection.Execute("DELETE FROM Vehiculos");
            return true;
        } catch (Exception ex) {
            _logger.Error(ex, "Error al eliminar todos los vehiculos");
            return false;
        }
    }

    public Result<Vehiculo, DomainError> Restore(int id) {
        try {
            var existing = GetById(id);
            if (existing == null)
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));

            var sql = "UPDATE Vehiculos SET IsDeleted = 0, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            _connection.Execute(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });

            _logger.Information("Vehiculo con ID {Id} restaurado correctamente", id);
            return Result.Success<Vehiculo, DomainError>(GetById(id)!);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al restaurar vehiculo");
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.DatabaseError(ex.Message));
        }
    }

    private void EnsureTable(bool dropData) {
        if (_connection.State != ConnectionState.Broken) {
            _connection.Open();
        }

        if (dropData) _connection.Execute("DROP TABLE IF EXISTS Vehiculos");
        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Vehiculos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Matricula TEXT NOT NULL,
                Marca TEXT NOT NULL,
                Cilindrada INTEGER NOT NULL,
                Motor INTEGER NOT NULL,
                Dni TEXT NOT NULL,
                FechaMatriculacion TEXT NOT NULL,
                FechaInspeccion TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                IsDeleted INTEGER NOT NULL
            )");
    }
    private void Seed() {
        foreach (var p in VehiculosFactory.Seed()) Create(p);
    }
    private int CountTotal() {
        return _connection.ExecuteScalar<int>("SELECT COUNT(1) FROM Vehiculos");
    }

    private bool CupoVehiculosPorDia(string dni, DateTime fecha, int idActual = -1) {
       
        var sql = @"SELECT COUNT(*) FROM Vehiculos WHERE Dni = @Dni AND DATE(FechaInspeccion) = DATE(@Fecha)AND Id != @IdActual AND IsDeleted = 0";

        var count = _connection.ExecuteScalar<int>(sql, new { Dni = dni, Fecha = fecha, IdActual = idActual });
        return count < 3;
    } 
    private bool ExisteCitaDuplicada(string matricula, DateTime fecha, int idActual = -1) {
        var sql = @"SELECT COUNT(*) FROM Vehiculos WHERE Matricula = @Matricula AND DATE(FechaInspeccion) = DATE(@Fecha)AND Id != @IdActual AND IsDeleted = 0";

        var count = _connection.ExecuteScalar<int>(sql, new { Matricula = matricula, Fecha = fecha, IdActual = idActual });
        return count > 0;
    }
}