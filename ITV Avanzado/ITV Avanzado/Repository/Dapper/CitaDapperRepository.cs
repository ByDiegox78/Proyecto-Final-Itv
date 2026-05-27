using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using GestionItv.Models;
using ITV_Avanzado.Entity;
using ITV_Avanzado.Error.Citas;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Factory;
using ITV_Avanzado.Mapper;
using ITV_Avanzado.Repository.Common;
using Serilog;
using Microsoft.Data.Sqlite;


namespace ITV_Avanzado.Repository.Dapper;

public class CitaDapperRepository : ICitaRepository {
    private readonly IDbConnection _connection;
    private readonly ILogger _logger = Log.ForContext<CitaDapperRepository>();
    private readonly Action? _onDispose;
    
    public CitaDapperRepository(IDbConnection connection, Action? onDispose = null, bool dropData = false,
        bool seedData = false) {
        _connection = connection;
        _onDispose = onDispose;
        EnsureTable(dropData);

        if (seedData && CountTotal() == 0) Seed();
    }
    
   
    /// <inheritdoc cref="ICitaRepository.GetAll(int, int, bool, string)" />
    public IEnumerable<Cita> GetAll(int page, int pageSize, bool includeDeleted, string? campoBusqueda) {
        const string sql = @"
                SELECT 
                    Id, 
                    Matricula, 
                    Marca, 
                    Cilindrada, 
                    Motor, 
                    Dni,
                    FechaMatriculacion, 
                    FechaInspeccion, 
                    IsDeleted, 
                    CreatedAt, 
                    UpdatedAt
                FROM Citas 
                WHERE (@IncludeDeleted = 1 OR IsDeleted = 0)
                  AND (@Busqueda IS NULL OR (
                      Matricula LIKE '%' || @Busqueda || '%' OR
                      Marca LIKE '%' || @Busqueda || '%' OR
                      Dni LIKE '%' || @Busqueda || '%'
                  ))
                ORDER BY Id 
                LIMIT @Limit OFFSET @Offset";

        try {

            var parameters = new {
                Busqueda = string.IsNullOrWhiteSpace(campoBusqueda) ? null : campoBusqueda,
                IncludeDeleted = includeDeleted ? 1 : 0,
                Limit = pageSize,
                Offset = (page - 1) * pageSize
            };

            var entities = _connection.Query<CitaEntity>(sql, parameters);
            return entities.ToModel();        }
        catch (Exception ex) {
            _logger.Error(ex, "Error en GetAll Dapper");
            return Enumerable.Empty<Cita>();
        }
    }

    public Cita? GetById(int id) {
        try {
            var sql = "SELECT * FROM Citas WHERE Id = @Id";
            var entities = _connection.QueryFirstOrDefault<CitaEntity>(sql, new { Id = id });
            return entities.ToModel();
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
        var entity = cita.ToEntity();
        try {
            var sql = @"
            INSERT INTO Citas (Matricula, Marca, Cilindrada, Motor, Dni, FechaMatriculacion, FechaInspeccion, IsDeleted, CreatedAt, UpdatedAt)
            VALUES 
            (@Matricula, @Marca, @Cilindrada, @Motor, @Dni, @FechaMatriculacion, @FechaInspeccion, @IsDeleted, @CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid()";
            entity.Id = _connection.ExecuteScalar<int>(sql, entity);
            return Result.Success<Cita, DomainError>(GetById(entity.Id)!);
        }
        catch (Exception e) {
            _logger.Error(e, "Error al crear vehiculo");
            return Result.Failure<Cita, DomainError>(CitaErrors.DatabaseError(e.Message));
        }
    }

    public Result<Cita, DomainError> Update(int id, Cita cita) {
        var existing = GetById(id);
        if (existing == null) {
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
        
        cita = cita with {
            Id = id,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = existing.IsDeleted
        };
        var entity = cita.ToEntity();
        try {
            var sql = @"UPDATE Citas SET Matricula = @Matricula,  Marca = @Marca,Cilindrada = @Cilindrada,Motor = @Motor,
            Dni = @Dni, IsDeleted = @IsDeleted, CreatedAt = @CreatedAt, UpdatedAt = @UpdatedAt
            WHERE Id = @Id;";
            _connection.Execute(sql, entity);
            return Result.Success<Cita, DomainError>(GetById(id)!);

        }
        catch (Exception) {
            return Result.Failure<Cita, DomainError>(
                CitaErrors.DatabaseError("Error al actualizar el vehículo"));
        }
    }

    public Cita? Delete(int id, bool isLogic = true) {
        try {
            var existing = GetById(id);
            if (existing == null)
                return null;
            if (isLogic) {
                var sql = "UPDATE Citas SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";            
                _connection.Execute(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
                return GetById(id);
            }
            var sqlHard = "DELETE FROM Citas WHERE Id = @Id";
            _connection.Execute(sqlHard, new { Id = id });
            return existing;
        }
        catch (Exception e) {
            _logger.Error(e, "Error al eliminar vehiculo");
            return null;
        }
    }
    public IEnumerable<Cita>? GetByMatricula(string matricula, int page = 1, int pageSize = 10) {
        try {
            var sql = @"SELECT * FROM Citas WHERE Matricula = @Matricula AND IsDeleted = 0
                ORDER BY Id LIMIT @Limit OFFSET @Offset";
            var entity = _connection.Query<CitaEntity>(sql, new { Matricula = matricula, Limit = pageSize, Offset = (page - 1) * pageSize });
            return entity.ToModel();
            
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al obtener vehiculo por Matricula {Matricula}", matricula);
            return null;
        }
    }
    public bool DeleteAll() {
        try {
            _connection.Execute("DELETE FROM Citas");
            return true;
        } catch (Exception ex) {
            _logger.Error(ex, "Error al eliminar todos los vehiculos");
            return false;
        }
    }

    public Result<Cita, DomainError> Restore(int id) {
        try {
            var existing = GetById(id);
            if (existing == null)
                return Result.Failure<Cita, DomainError>(CitaErrors.NotFound(id.ToString()));

            var sql = "UPDATE Citas SET IsDeleted = 0, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            _connection.Execute(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });

            _logger.Information("Vehiculo con ID {Id} restaurado correctamente", id);
            return Result.Success<Cita, DomainError>(GetById(id)!);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al restaurar vehiculo");
            return Result.Failure<Cita, DomainError>(CitaErrors.DatabaseError(ex.Message));
        }
    }

    private void EnsureTable(bool dropData) {
        if (_connection.State != ConnectionState.Broken) {
            _connection.Open();
        }

        if (dropData) _connection.Execute("DROP TABLE IF EXISTS Citas");
        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Citas (
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
        foreach (var p in CitasFactory.Seed()) Create(p);
    }
    private int CountTotal() {
        return _connection.ExecuteScalar<int>("SELECT COUNT(1) FROM Citas");
    }

    private bool CupoVehiculosPorDia(string dni, DateTime fecha, int idActual = -1) {
       
        var sql = @"SELECT COUNT(*) FROM Citas WHERE Dni = @Dni AND DATE(FechaInspeccion) = DATE(@Fecha)AND Id != @IdActual AND IsDeleted = 0";

        var count = _connection.ExecuteScalar<int>(sql, new { Dni = dni, Fecha = fecha, IdActual = idActual });
        return count < 3;
    } 
    private bool ExisteCitaDuplicada(string matricula, DateTime fecha, int idActual = -1) {
        var sql = @"SELECT COUNT(*) FROM Citas WHERE Matricula = @Matricula AND DATE(FechaInspeccion) = DATE(@Fecha)AND Id != @IdActual AND IsDeleted = 0";

        var count = _connection.ExecuteScalar<int>(sql, new { Matricula = matricula, Fecha = fecha, IdActual = idActual });
        return count > 0;
    }
}