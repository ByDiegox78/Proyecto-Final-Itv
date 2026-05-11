using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Storage.Common;
using Serilog;

namespace ITV_Avanzado.Service.ImportExport;

public class ImportExportService(
    IStorage<Vehiculo> storage
    ) : IImportExportService{
    
    private readonly ILogger _logger = Log.ForContext<ImportExportService>();

    public Result<int, DomainError> ExportarDatos(IEnumerable<Vehiculo> citas, string path) {
        _logger.Information("Exportando datps a {Path}", path);
        var list = citas.ToList();
        return storage.Salvar(list, path)
            .Map(_ => list.Count);
    }

    public Result<IEnumerable<Vehiculo>, DomainError> ImportarDatos(string path) {
        return storage.Cargar(path);
    }

    public Result<int, DomainError> ExportarDatosSistema(IEnumerable<Vehiculo> citas) {
        return ExportarDatos(citas, string.Empty);
    }

    public Result<IEnumerable<Vehiculo>, DomainError> ImportarDatosSistema(string path) {
        return ImportarDatos(path);
    }
}