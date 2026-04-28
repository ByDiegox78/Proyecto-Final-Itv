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
    public IEnumerable<Vehiculo> GetAll(int page = 1, int pageSize = 5, bool includeDeleted = true) {
        _logger.Debug("GetAll: pag {Page}, size {Size}", page, pageSize);
        var entities = new List<VehiculoEntity>();
        using var connection = CreateConnection();
        connection.Open();
        
        string sql = "SELECT * FROM Vehiculos ";
        if (!includeDeleted) sql += "WHERE IsDeleted = 0 ";
        sql += "ORDER BY Id LIMIT @Limit OFFSET @Offset";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Limit", pageSize);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

        using var reader = command.ExecuteReader();
        while (reader.Read()) {
            entities.Add(ReadEntity(reader));
        }
        return entities.ToModel();
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
        if (ExisteMatricula(vehiculo.Matricula))
            return Result.Failure<Vehiculo, DomainError>(
                VehiculoErrors.MatriculaAlreadyExists($"La matrícula del vehículo ya se encuentra en uso {vehiculo.Matricula}"));
        if (!VerificarCochePropietario(vehiculo.DniPropietario))          
            return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError("El propietario no puede incluir este vehiculo porque ya tiene 3 a su disposicion"));

        var vehiculoEntity = vehiculo.ToEntity();
        vehiculoEntity.CreatedAt = DateTime.UtcNow;
        vehiculoEntity.UpdatedAt = DateTime.UtcNow;
        vehiculoEntity.IsDeleted = false;
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Vehiculos (
            Matricula, Marca, Cilindrada, Motor, Dni, IsDeleted, CreatedAt, UpdatedAt) 
            VALUES (
            @Matricula, @Marca, @Cilindrada, @Motor, @Dni, @IsDeleted, @CreatedAt, @UpdatedAt);
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
        
        if (vehiculo.Matricula != exists.Matricula) {
            var other = GetByMatricula(vehiculo.Matricula);
            if (other != null && other.Id != id) {
                _logger.Warning("No se puede actualizar vehículo con id {Id} porque la matrícula {Matricula} ya está en uso", id, vehiculo.Matricula);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaAlreadyExists(vehiculo.Matricula));
            }
        }
        if (vehiculo.DniPropietario != exists.DniPropietario) {
            if (!VerificarCochePropietario(vehiculo.DniPropietario)) {
                _logger.Warning("El propietario con DNI {Dni} no es válido o ya tiene 3 vehículos", vehiculo.DniPropietario);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));
            }
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
            SET Matricula = @Matricula, Marca = @Marca, Cilindrada = @Cilindrada, Motor = @Motor, Dni = @Dni, IsDeleted = @IsDeleted, CreatedAt = @CreatedAt, UpdatedAt = @UpdatedAt
            WHERE Id = @Id;";
        AddParameters(command, vEntity, vEntity.Id);
        command.ExecuteNonQuery();
        return Result.Success<Vehiculo, DomainError>(vEntity.ToModel());
    }
    public Vehiculo? Delete(int id) {
        var exists = GetById(id);
        if (exists == null) return null;
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Vehiculos SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        command.Parameters.Add(new SqliteParameter("@Id", id));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", DateTime.UtcNow.ToString("o")));
        command.ExecuteNonQuery();

        return GetById(id);    
    }
    public bool DeleteAll() {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Vehiculos;";
        command.ExecuteNonQuery();
        return true;
    }
    public Vehiculo? HardDelete(int id) {
        var exists = GetById(id);
        if (exists == null) return null;
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Vehiculos WHERE Id = @Id";
        command.Parameters.Add(new SqliteParameter("@Id", id));
        command.ExecuteNonQuery();
        return exists;
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
    public Vehiculo? GetByMatricula(string matricula) {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Vehiculos WHERE Matricula = @Matricula";
        command.Parameters.Add(new SqliteParameter("@Matricula", matricula));
        
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadEntity(reader).ToModel() : null;
    }
    private void EnsureTable() {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
        DROP TABLE IF EXISTS Vehiculos;
        CREATE TABLE Vehiculos(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Matricula TEXT NOT NULL UNIQUE,
            Marca TEXT NOT NULL,
            Cilindrada INTEGER NOT NULL,
            Motor INTEGER NOT NULL,
            Dni TEXT NOT NULL,
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
            IsDeleted = reader.GetInt32(reader.GetOrdinal("isdeleted")) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("createdat"))),
            UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("updatedat")))
        };
    }
    private bool VerificarCochePropietario(string dni) {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Vehiculos WHERE Dni = @dni AND IsDeleted = 0";
        command.Parameters.AddWithValue("@dni", dni);
        int cantidadCoches = Convert.ToInt32(command.ExecuteScalar());
        return cantidadCoches < 3;
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
        command.Parameters.Add(new SqliteParameter("@IsDeleted", entity.IsDeleted ? 1 : 0));
        command.Parameters.Add(new SqliteParameter("@CreatedAt", entity.CreatedAt.ToString("s")));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", entity.UpdatedAt.ToString("s")));
    }
    private bool ExisteMatricula(string matricula) {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT COUNT(1) FROM Vehiculos WHERE Matricula = @Matricula";
        command.Parameters.Add(new SqliteParameter("Matricula", matricula));
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}