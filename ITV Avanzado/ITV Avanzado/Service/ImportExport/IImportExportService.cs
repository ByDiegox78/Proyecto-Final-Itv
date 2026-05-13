using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Service.ImportExport;

public interface IImportExportService {
    Result<int, DomainError> ExportarDatos(IEnumerable<Vehiculo> citas, string path);
    
    Result<IEnumerable<Vehiculo>, DomainError> ImportarDatos(string path);
    
    Result<int, DomainError> ExportarDatosSistema(IEnumerable<Vehiculo> citas);
    
    Result<IEnumerable<Vehiculo>, DomainError> ImportarDatosSistema(string path);
}