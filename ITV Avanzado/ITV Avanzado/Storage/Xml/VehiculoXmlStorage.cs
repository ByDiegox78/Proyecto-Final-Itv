using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Config;
using ITV_Avanzado.Dto;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Storage;
using ITV_Avanzado.Mapper;
using Serilog;

namespace ITV_Avanzado.Storage.Xml;

public class VehiculoXmlStorage : IVehiculoXmlStorage {
    
    private readonly XmlSerializerNamespaces XmlSerializerNamespaces = new(); //Gestiona el namespace del xml
    
    private readonly XmlWriterSettings XmlWriterSettings = new() { // Configura como se escribe el XML
        Indent = true, // Mete tabulazos
        Encoding = Encoding.UTF8 
    };
    public VehiculoXmlStorage() {
        InitStorage();
    }
    private readonly ILogger _logger = Log.ForContext<VehiculoXmlStorage>();
    
    
    public Result<bool, DomainError> Salvar(IEnumerable<Vehiculo> items, string path) {
        try {
            _logger.Debug("Guardando los items en el archivo '{path}'", path);
            
            var dtos = items.Select(p => p.ToDto()).ToList(); // Convertir a Dto
            var serializer = new XmlSerializer(typeof(List<VehiculoDto>)); // Convertir la lista a Xml 
            
            using var streamWriter = new StreamWriter(path, false, Encoding.UTF8); // Abrir el archivo para escribir
            var xmlWriter = XmlWriter.Create(streamWriter, XmlWriterSettings);  //Aplica la configuracion para el archivo
            serializer.Serialize(xmlWriter, dtos, XmlSerializerNamespaces); // Convierte la lista en xml y escribe el archivo
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
            var serializer = new XmlSerializer(typeof(List<VehiculoDto>));
            using var streamReader = new StreamReader(path);
            var dtos = serializer.Deserialize(streamReader) as List<VehiculoDto>;
            if (dtos == null) 
                return Result.Failure<IEnumerable<Vehiculo>, DomainError>(StorageErrors.InvalidFormat("El archivo XML no tiene un formato válido o está vacío"));
            var vehiculos = dtos?.Select(dto => dto.ToModel()).ToList();
            return Result.Success<IEnumerable<Vehiculo>, DomainError>(vehiculos);
            
        } catch (Exception e) {
            _logger.Error(e, "Error inesperado al cargar el archivo XML en {Path}", path);
            return Result.Failure<IEnumerable<Vehiculo>, DomainError>(
                StorageErrors.ReadError($"Error al leer el Xml: {e.Message}")
            );
        }
    }
    private static void InitStorage() {
        if (Directory.Exists(AppConfig.DataFolder))
            return;
        Directory.CreateDirectory(AppConfig.DataFolder);
    }
}