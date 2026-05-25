using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Config;
using ITV_Avanzado.Dto;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Storage;
using ITV_Avanzado.Mapper;
using Serilog;

namespace ITV_Avanzado.Storage.Json;

public class CitasJsonStorage : ICitasJsonStorage{
    private readonly ILogger _logger = Log.ForContext<CitasJsonStorage>();
    private readonly JsonSerializerOptions _options = new() {
        
        WriteIndented = true, //Hace el json mas visible
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Convierte las propiedades a camelCase en el JSON
        DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull, // Si un campo es nulo, no lo escribe en el json
        Converters = { new JsonStringEnumConverter() }, //Convierte cualquier enum que encuentre a string
        Encoder = JavaScriptEncoder //Permite caracteres especiales
            .UnsafeRelaxedJsonEscaping
    };
    
    public CitasJsonStorage() {
        InitStorage();
    }
    public Result<bool, DomainError> Salvar(IEnumerable<Cita> items, string path) {
        try {
            _logger.Debug("Guardando los items en el archivo '{path}'", path);
            var dto = items.Select(p => p.ToDto()).ToList(); //Convierte cada vehiculo a vehiculoDto
            // Convierte la lista de dto al formato json usando la configuracion que le proporcionamos
            var json = JsonSerializer.Serialize(dto, _options);
            File.WriteAllText(path, json, Encoding.UTF8); //Escribe el json en el archivo
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception e) {
            _logger.Error(e, "Error al guardar los items en el archivo '{path}'", path);
            return Result.Failure<bool, DomainError>(StorageErrors.WriteError(e.Message));

        }
    }

    public Result<IEnumerable<Cita>, DomainError> Cargar(string path) {
        _logger.Debug("Cargando los items del archivo '{path}'", path);
        if (!Path.Exists(path)) {
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.FileNotFound(path));
        }
        try {
            var json = File.ReadAllText(path, Encoding.UTF8); // Carga todo el archivo Json
            var dtos = JsonSerializer.Deserialize<List<CitaDto>>(json, _options); // Convierte el json a una List<VehiculoDto>
            if (dtos == null)
                return Result.Failure<IEnumerable<Cita>, DomainError>(
                    StorageErrors.InvalidFormat("No se pudieron deserializar los DTOs."));
            return Result.Success<IEnumerable<Cita>, DomainError>(dtos.Select(dto => dto.ToModel()));
        }
        catch (Exception e) {
            _logger.Error(e, "Error al cargar los items del archivo '{path}'", path);
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.ReadError(e.Message));
        }    
    }
    
    private void InitStorage() {
        if (Directory.Exists(AppConfig.DataFolder))
            return;
        Directory.CreateDirectory(AppConfig.DataFolder);
    }
}