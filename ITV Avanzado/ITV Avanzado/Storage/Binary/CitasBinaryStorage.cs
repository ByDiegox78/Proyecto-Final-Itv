using System.Text;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Config;
using ITV_Avanzado.Dto;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Storage;
using ITV_Avanzado.Mapper;
using Serilog;

namespace ITV_Avanzado.Storage.Binary;

public class CitasBinaryStorage : ICitasBinaryStorage {
    
    private readonly ILogger _logger = Log.ForContext<CitasBinaryStorage>();

    public CitasBinaryStorage() {
        _logger.Debug("Inicializando la clase AcademiaBinStorage");
        InitStorage();
    }
    
    public Result<bool, DomainError> Salvar(IEnumerable<Cita> items, string path) {
        try
        {
            _logger.Debug("Guardando datos en el archivo binario '{path}'", path);
            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            var dtos = items.Select(p => p.ToDto()).ToList();
            writer.Write(dtos.Count);
            foreach (var d in dtos) {
                writer.Write(d.Id);
                writer.Write(d.Matricula);
                writer.Write(d.Marca);
                writer.Write(d.Cilindrada);
                writer.Write(d.TipoMotor);
                writer.Write(d.DniPropietario);
                writer.Write(d.FechaMatriculacion);
                writer.Write(d.FechaInspeccion);
                writer.Write(d.IsDelete);
                writer.Write(d.CreatedAt);
                writer.Write(d.UpdatedAt);
            }
            return Result.Success<bool, DomainError>(true);
        } catch (Exception e) {
            _logger.Error(e, "Error al gurdar los datos en el archivo binario '{path}'", path);
            return Result.Failure<bool, DomainError>(StorageErrors.WriteError(e.Message));
        }
    }

    public Result<IEnumerable<Cita>, DomainError> Cargar(string path) {
        _logger.Debug("Cargando los datos del archivo binario '{path}'", path);

        if (!File.Exists(path)) {
            _logger.Warning("El archivo '{path}' no existe.", path);
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.FileNotFound(path));
        }

        try {
            var list = new List<Cita>();
            var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++) {
                var dto = new CitaDto(
                    reader.ReadInt32(),
                    reader.ReadString(),
                    reader.ReadString(),
                    reader.ReadInt32(),
                    reader.ReadString(),
                    reader.ReadString(),
                    reader.ReadString(),
                    reader.ReadString(),
                    reader.ReadBoolean(),
                    reader.ReadString(),
                    reader.ReadString()
                );
                list.Add(dto.ToModel());
            }
            return Result.Success<IEnumerable<Cita>, DomainError>(list);
        } catch (Exception e) {
            _logger.Error(e, "Error al cargar los items del archivo binario '{path}'", path);
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.InvalidFormat(e.Message));
        }
    }
    private void InitStorage() {
        if (Directory.Exists(AppConfig.DataFolder))
            return;
        _logger.Debug("El directorio 'data' no existe. Creándolo...");
        Directory.CreateDirectory(AppConfig.DataFolder);
    }
}