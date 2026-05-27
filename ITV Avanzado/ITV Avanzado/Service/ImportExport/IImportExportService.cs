using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Service.ImportExport;

public interface IImportExportService {
    Result<int, DomainError> ExportarDatos(IEnumerable<Cita> citas, string path);
    
    Result<IEnumerable<Cita>, DomainError> ImportarDatos(string path);
    
    Result<int, DomainError> ExportarDatosSistema(IEnumerable<Cita> citas);
    
    Result<IEnumerable<Cita>, DomainError> ImportarDatosSistema(string path);
}