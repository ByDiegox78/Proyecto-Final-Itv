using System.Data;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Config;
using ITV_Avanzado.Entity;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Factory;
using ITV_Avanzado.Mapper;
using ITV_Avanzado.Repository.Common;
using Microsoft.Data.Sqlite;
using Serilog;

namespace ITV_Avanzado.Repository.Ado;

public class VehiculoAdoRepository : IVehiculosRepository{
    private readonly ILogger _logger = Log.ForContext<VehiculoAdoRepository>();
    private readonly string _connectionString;
    
    public VehiculoAdoRepository() : this(AppConfig.DropData, AppConfig.SeedData) { }

    public VehiculoAdoRepository(bool dropData, bool seedData) {
        _logger.Debug("Iniciando Repositorio Ado");
        _connectionString = AppConfig.ConnectionString;
        EnsureDataFolder();
        EnsureTable();
        if (dropData) {
                _logger.Warning("Borrando todos los datos...");
                DeleteAll();
        } 
        if (seedData) {
            _logger.Information("Cargando datos de semilla..."); 
            foreach (var v in VehiculosFactory.Seed()) { 
                Create(v);
            } 
            _logger.Information("Datos cargados exitosamente");
        }
    }
    private SqliteConnection CreateConnection() => new(_connectionString);
    public IEnumerable<Vehiculo> GetAll(int page, int pageSize, bool includeDeleted, string campoBusqueda) {
        _logger.Debug("GetAll: pag {Page}, size {Size}", page, pageSize);
        var lista = new List<Vehiculo>();
        try {
            using var connection = CreateConnection();
            connection.Open();
            const string sql = @"
            SELECT * FROM Vehiculos 
            WHERE (@IncludeDeleted = 1 OR IsDeleted = 0)
              AND (@Busqueda IS NULL OR (
                  Matricula LIKE '%' || @Busqueda || '%' OR
                  Marca LIKE '%' || @Busqueda || '%' OR
                  Dni LIKE '%' || @Busqueda || '%'
              ))
            ORDER BY Id 
            LIMIT @Limit OFFSET @Offset";

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Busqueda", string.IsNullOrWhiteSpace(campoBusqueda) ? DBNull.Value : campoBusqueda);
            command.Parameters.AddWithValue("@IncludeDeleted", includeDeleted ? 1 : 0);
            command.Parameters.AddWithValue("@Limit", pageSize);
            command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

            using var reader = command.ExecuteReader();
            while (reader.Read()) {
                lista.Add(ReadEntity(reader).ToModel()!);
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error en GetAll ADO.NET");
        }

        return lista;
    }
    public Vehiculo? GetById(int id) {
        _logger.Debug("Obteniendo vehiculo con id: {Id}", id);
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Vehiculos WHERE Id = @Id";
        command.Parameters.Add(new SqliteParameter("Id", id));
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadEntity(reader).ToModel() : null;
    }
    public Result<Vehiculo, DomainError> Create(Vehiculo vehiculo) {
        _logger.Debug("Creando vehículo con matrícula: {Matricula}", vehiculo.Matricula);
        if (!CupoVehiculosPorDia(vehiculo.DniPropietario, vehiculo.FechaInspeccion)) {
            _logger.Warning("El propietario con dni: {dni} tiene 3 vehiculos para inspeccion para el mismo dia", vehiculo.DniPropietario);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));
        }
        if (ExisteCitaDuplicada(vehiculo.Matricula,vehiculo.FechaInspeccion)) {
            _logger.Warning("La matricula {matriula} tiene una inspeccion resgistrada para hoy", vehiculo.Matricula);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaInspeccionDuplicada(vehiculo.Matricula));
        }
        
        var vehiculoEntity = vehiculo.ToEntity();
        vehiculoEntity.CreatedAt = DateTime.UtcNow;
        vehiculoEntity.UpdatedAt = DateTime.UtcNow;
        vehiculoEntity.IsDeleted = false;
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Vehiculos (
            Matricula, Marca, Cilindrada, Motor, Dni,FechaMatriculacion,FechaInspeccion, IsDeleted, CreatedAt, UpdatedAt) 
            VALUES (
            @Matricula, @Marca, @Cilindrada, @Motor, @Dni,@FechaMatriculacion,@FechaInspeccion, @IsDeleted, @CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid();";
        AddParameters(command, vehiculoEntity);
        vehiculoEntity.Id = Convert.ToInt32(command.ExecuteScalar());
        var model = vehiculoEntity.ToModel();
        if (model == null) {
            return Result.Failure<Vehiculo, DomainError>(
                VehiculoErrors.DatabaseError("Error al mapear el vehículo")
            );
        }
        return Result.Success<Vehiculo, DomainError>(model);
    }
    public Result<Vehiculo, DomainError> Update(int id, Vehiculo vehiculo) {
        var exists = GetById(id);
        if (exists == null)            
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
        
        if (!CupoVehiculosPorDia(vehiculo.DniPropietario, vehiculo.FechaInspeccion, id)) {
            _logger.Warning("El propietario con dni: {dni} tiene 3 vehiculos para inspeccion para el mismo dia", vehiculo.DniPropietario);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));
        }
        if (ExisteCitaDuplicada(vehiculo.Matricula,vehiculo.FechaInspeccion,id)) {
            _logger.Warning("La matricula {matriula} tiene una inspeccion resgistrada para hoy", vehiculo.Matricula);
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaInspeccionDuplicada(vehiculo.Matricula));
        }
        var vEntity = vehiculo.ToEntity();
        vEntity.Id = id;
        vEntity.CreatedAt = exists.CreatedAt;
        vEntity.UpdatedAt = DateTime.UtcNow;
        vEntity.IsDeleted = false;
        
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Vehiculos 
            SET Matricula = @Matricula, Marca = @Marca, Cilindrada = @Cilindrada, Motor = @Motor, Dni = @Dni,FechaMatriculacion = @FechaMatriculacion, 
                FechaInspeccion = @FechaInspeccion, IsDeleted = @IsDeleted, CreatedAt = @CreatedAt, UpdatedAt = @UpdatedAt
            WHERE Id = @Id;";
        AddParameters(command, vEntity, vEntity.Id);
        command.ExecuteNonQuery();
        return Result.Success<Vehiculo, DomainError>(vEntity.ToModel());
    }
    public Vehiculo? Delete(int id, bool isLogic = true) {
        var exists = GetById(id);
        if (exists == null) return null;
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        if (isLogic) {
            command.CommandText = "UPDATE Vehiculos SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            command.Parameters.Add(new SqliteParameter("@Id", id));
            command.Parameters.Add(new SqliteParameter("@UpdatedAt", DateTime.UtcNow.ToString("o")));
            command.ExecuteNonQuery();
            return exists;
        }    
        command.CommandText = "DELETE FROM Vehiculos WHERE Id = @Id";
        command.Parameters.Add(new SqliteParameter("@Id", id));
        command.ExecuteNonQuery();
        return exists;
    }
    public bool DeleteAll() {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Vehiculos;";
        command.ExecuteNonQuery();
        return true;
    }
    public Result<Vehiculo, DomainError> Restore(int id) {
        var exists = GetById(id);
        if (exists == null) 
            return Result.Failure<Vehiculo, DomainError>(
                VehiculoErrors.NotFound($"No existe el vehículo con id {id}"));
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Vehiculos SET IsDeleted = 0, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        command.Parameters.Add(new SqliteParameter("@Id", id));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", DateTime.UtcNow.ToString("o")));
        command.ExecuteNonQuery();
        var updated = GetById(id);
        return Result.Success<Vehiculo, DomainError>(updated);
    }
    public IEnumerable<Vehiculo>? GetByMatricula(string matricula) {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Vehiculos WHERE Matricula = @Matricula AND IsDeleted = 0";
        command.Parameters.Add(new SqliteParameter("@Matricula", matricula));
        var list = new List<Vehiculo>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) {
            list.Add(ReadEntity(reader).ToModel());
        }
        return list;    
    }
    private void EnsureTable() {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
        DROP TABLE IF EXISTS Vehiculos;
        CREATE TABLE Vehiculos(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Matricula TEXT NOT NULL,
            Marca TEXT NOT NULL,
            Cilindrada INTEGER NOT NULL,
            Motor INTEGER NOT NULL,
            Dni TEXT NOT NULL,
            FechaMatriculacion Text NOT NULL,
            FechaInspeccion Text NOT NULL,  
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL,
            IsDeleted INTEGER NOT NULL
            );";
        command.ExecuteNonQuery();
    }
    private void EnsureDataFolder() {
        if (!Directory.Exists(AppConfig.DataFolder)) {
            Directory.CreateDirectory(AppConfig.DataFolder);
        }
    }
    private VehiculoEntity ReadEntity(SqliteDataReader reader) {
        return new VehiculoEntity {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Matricula = reader.GetString(reader.GetOrdinal("matricula")),
            Marca = reader.GetString(reader.GetOrdinal("marca")),
            Cilindrada = reader.GetInt32(reader.GetOrdinal("cilindrada")),
            Motor = reader.GetInt32(reader.GetOrdinal("motor")),
            Dni = reader.GetString(reader.GetOrdinal("dni")),
            FechaInspeccion = DateTime.Parse(reader.GetString(reader.GetOrdinal("fechainspeccion"))),
            FechaMatriculacion = DateTime.Parse(reader.GetString(reader.GetOrdinal("fechamatriculacion"))),
            IsDeleted = reader.GetInt32(reader.GetOrdinal("isdeleted")) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("createdat"))),
            UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("updatedat")))
        };
    }
    private void AddParameters(IDbCommand command, VehiculoEntity entity, int? id = null) { 
        if (id.HasValue) {
            command.Parameters.Add(new SqliteParameter("@Id", id.Value));
        }
        command.Parameters.Add(new SqliteParameter("@Matricula", entity.Matricula));
        command.Parameters.Add(new SqliteParameter("@Marca", entity.Marca));
        command.Parameters.Add(new SqliteParameter("@Cilindrada", entity.Cilindrada));
        command.Parameters.Add(new SqliteParameter("@Motor", entity.Motor));
        command.Parameters.Add(new SqliteParameter("@Dni", entity.Dni));
        command.Parameters.Add(new SqliteParameter("@FechaMatriculacion", entity.FechaMatriculacion.ToString("s")));
        command.Parameters.Add(new SqliteParameter("@FechaInspeccion", entity.FechaInspeccion.ToString("s")));
        command.Parameters.Add(new SqliteParameter("@IsDeleted", entity.IsDeleted ? 1 : 0));
        command.Parameters.Add(new SqliteParameter("@CreatedAt", entity.CreatedAt.ToString("s")));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", entity.UpdatedAt.ToString("s")));
    }
    //Unicidad de Cita: Un mismo vehículo (matrícula) no puede tener programada más de una inspección el mismo día.
    //Cupo por Propietario: Un mismo DNI no puede tener más de tres vehículos registrados para inspección en la misma fecha.
    private bool CupoVehiculosPorDia(string dni, DateTime fecha, int idActual = -1) {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText= @"SELECT COUNT(*) 
        FROM Vehiculos 
        WHERE Dni = @Dni 
        AND DATE(FechaInspeccion) = DATE(@Fecha)
        AND Id != @IdActual
        AND IsDeleted = 0";
        command.Parameters.Add(new SqliteParameter("Dni", dni));
        command.Parameters.Add(new SqliteParameter("@Fecha", fecha));
        command.Parameters.Add(new SqliteParameter("@IdActual", idActual));
        return Convert.ToInt32(command.ExecuteScalar()) < 3; 
    } 
    private bool ExisteCitaDuplicada(string matricula, DateTime fecha, int idActual = -1) {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText= @"SELECT COUNT(*) 
        FROM Vehiculos 
        WHERE Matricula = @Matricula 
        AND DATE(FechaInspeccion) = DATE(@Fecha)
        AND Id != @IdActual
        AND IsDeleted = 0";
        command.Parameters.Add(new SqliteParameter("Matricula", matricula));
        command.Parameters.Add(new SqliteParameter("@Fecha", fecha));
        command.Parameters.Add(new SqliteParameter("@IdActual", idActual));
        return Convert.ToInt32(command.ExecuteScalar()) > 0; 
    }
}