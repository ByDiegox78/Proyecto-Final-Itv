using System.Text;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Config;
using ITV_Avanzado.Dto;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Storage;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Mapper;
using Serilog;

namespace ITV_Avanzado.Storage.Csv;

public class VehiculoCsvStorage : IVehiculoCsvStorage {
    private readonly ILogger _logger = Log.ForContext<VehiculoCsvStorage>();

    public VehiculoCsvStorage() : this(AppConfig.DataFolder) { }
    public VehiculoCsvStorage(string dataFolder) {
        InitStorage(dataFolder);
    }
    
    public Result<bool, DomainError> Salvar(IEnumerable<Vehiculo> items, string path) {
        try {
            _logger.Debug("Guardando los items en el archivo {Path}", path);
            using var write = new StreamWriter(path, false, Encoding.UTF8);
            write.WriteLine("Id;Matricula;Marca;Cilindrada;TipoMotor;DniPropietario;FechaMatriculacion;FechaInspeccion;IsDelete;CreatedAt;UpdatedAt;");
            items.Select(p => p.ToDto())
                .ToList()
                .ForEach(p => {
                    write.WriteLine(
                        $"{p.Id};{p.Matricula};{p.Marca};{p.Cilindrada};{p.TipoMotor};{p.FechaMatriculacion};{p.FechaInspeccion};{p.DniPropietario};{p.IsDelete};{p.CreatedAt};{p.UpdatedAt};");
                });
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception e) {
            _logger.Error(e,"Error al guardar en el archivo {Path}", path);
            return Result.Failure<bool, DomainError>(StorageErrors.WriteError(e.Message));
        }
    }
    public Result<IEnumerable<Vehiculo>, DomainError> Cargar(string path) {
        if (!Path.Exists(path)) {
            return Result.Failure<IEnumerable<Vehiculo>, DomainError>(StorageErrors.FileNotFound(path));
        }
        try {
            var vehiculos = File.ReadLines(path, Encoding.UTF8)
                .Skip(1)
                .Select(linea => linea.Split(';'))
                .Select(campos => new VehiculoDto(
                    int.Parse(campos[0]),
                    campos[1],
                    campos[2],
                    int.Parse(campos[3]),
                    campos[4],
                    campos[5],
                    campos[6],
                    campos[7],
                    bool.Parse(campos[8]),
                    campos[9],
                    campos[10]
                ).ToModel()).ToList();
            return Result.Success<IEnumerable<Vehiculo>, DomainError>(vehiculos);
        }
        catch (Exception e) {
            _logger.Error("Error al cargar el archivo {Path}", path);
            return Result.Failure<IEnumerable<Vehiculo>, DomainError>(StorageErrors.InvalidFormat(e.Message)); 
        }
    }

    private void InitStorage(string folder) {
        if (Directory.Exists(AppConfig.DataFolder)) {
            return;
        }
        _logger.Debug("El directorio '{Folder}' no existe. Creándolo...", folder);
        Directory.CreateDirectory("data");
    }
}