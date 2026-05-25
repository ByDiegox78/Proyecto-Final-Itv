using System.IO.Compression;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Buckup;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Storage.Common;
using Serilog;

namespace ITV_Avanzado.Service.Buckup;

public class BackupService(
    IStorage<Cita> storage, 
    string? defaultBackupDirectory = null
    ) 
    : IBuckupService {
    private readonly ILogger _logger = Log.ForContext<BackupService>();
    
    public Result<string, DomainError> RealizarBackup(IEnumerable<Cita> vehiculos, string? customBackupDirectory = null) {
        var dir = customBackupDirectory ?? defaultBackupDirectory
            ?? throw new InvalidOperationException("No se a dicho un directorio");
        _logger.Information("Iniciando proceso de backup.");
        var list = vehiculos.ToList();
        if (list.Count == 0) {
            _logger.Warning("No hay datos para respaldar.");
            return Result.Failure<string, DomainError>(BackupErrors.CreationError("No hay datos para respaldar.")) ;
        }
        try {
            Directory.CreateDirectory(dir);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al crear el directorio de backup: {dir}", dir);
            return Result.Failure<string, DomainError>(
                BackupErrors.DirectoryError($"No se pudo crear el directorio: {dir}"));
        }
        var tempDir = Path.Combine(Path.GetTempPath(), $"backup-{Guid.NewGuid()}");
        var dataDir = Path.Combine(tempDir, "data");
        var imgDir = Path.Combine(tempDir, "img");
        Directory.CreateDirectory(dataDir);
        Directory.CreateDirectory(imgDir);
        try {
            var json = Path.Combine(dataDir, "citas.json");
            var salvarRes = storage.Salvar(list, json);
            if (salvarRes.IsFailure) {
                BackupErrors.CreationError("Error en la serializacion");
                return Result.Failure<string, DomainError>(
                    BackupErrors.CreationError("Error al serializar los datos."));
            }

            var fecha = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var zipPath = Path.Combine(dir, $"{fecha}-back.zip");

            try {
                ZipFile.CreateFromDirectory(tempDir, zipPath);
            }
            catch (Exception e) {
                _logger.Error(e, "Error al crear el archivo ZIP.");
                return Result.Failure<string, DomainError>(BackupErrors.CreationError("Error al comprimir el backup."));
            }

            _logger.Information("Backup creado correctamente: {zipPath}", zipPath);
            return Result.Success<string, DomainError>(zipPath);
        }
        finally {
            if (Directory.Exists(tempDir)) {
                Directory.Delete(tempDir, true);
                _logger.Debug("Directorio temporal limpiado.");
            }
        }
    }

    public Result<IEnumerable<Cita>, DomainError> RestaurarBackup(string archivoBackup, string? customImagesDirectory = null) {
        _logger.Information("Iniciando restauración desde: {archivo}", archivoBackup);
        if (!File.Exists(archivoBackup)) {
            _logger.Warning("Archivo de backup no encontrado: {path}", archivoBackup);
            return Result.Failure<IEnumerable<Cita>, DomainError>(BackupErrors.FileNotFound(archivoBackup));
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"restore-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try {
            try {
                ZipFile.ExtractToDirectory(archivoBackup, tempDir);
            }
            catch (Exception e) {
                _logger.Error(e, "Error al extraer el archivo ZIP.");
                return Result.Failure<IEnumerable<Cita>, DomainError>(
                    BackupErrors.InvalidBackupFile("No se pudo extraer el archivo ZIP."));
            }

            var dataDir = Path.Combine(tempDir, "data");

            var jsonPath = Path.Combine(dataDir, "citas.json");
            if (!File.Exists(jsonPath)) {
                _logger.Warning("El archivo de backup no contiene datos válidos (vehiculos.json no encontrado).");
                return Result.Failure<IEnumerable<Cita>, DomainError>(
                    BackupErrors.InvalidBackupFile("El archivo de backup no contiene datos válidos."));
            }

            var cargarResult = storage.Cargar(jsonPath);
            if (cargarResult.IsFailure) {
                _logger.Error("Error al deserializar los datos del backup.");
                return Result.Failure<IEnumerable<Cita>, DomainError>(
                    BackupErrors.InvalidBackupFile("El archivo de backup contiene datos corruptos."));
            }

            var vehiculos = cargarResult.Value.ToList();
            _logger.Information("Datos extraídos del backup correctamente.");
            return Result.Success<IEnumerable<Cita>, DomainError>(vehiculos);
        }
        finally {
            if (Directory.Exists(tempDir)) {
                Directory.Delete(tempDir, true);
                _logger.Debug("Directorio temporal limpiado.");
            }
        }
    }

    public IEnumerable<string> ListarBackups(string? customBackupDirectory = null) {
        var backDirectory = customBackupDirectory ?? defaultBackupDirectory;
        if (backDirectory == null || !Directory.Exists(backDirectory))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(backDirectory, "*.zip")
            .OrderByDescending(f => File.GetCreationTime(f));
    }

    public Result<string, DomainError> RealizarBackupSistema(IEnumerable<Cita> vehiculos) {
        return RealizarBackup(vehiculos);
    }

    public Result<int, DomainError> RestaurarBackupSistema(string archivoBackup, Func<bool> deleteAllCallback, Func<Cita, Result<Cita, DomainError>> createCallback) {
        _logger.Information("Iniciando restauración completa del sistema desde: {archivo}", archivoBackup);
        var deleteResult = deleteAllCallback();
        if (!deleteResult) {
            _logger.Warning("No se pudieron borrar los datos existentes.");
            return Result.Failure<int, DomainError>(
                BackupErrors.RestorationError("No se pudieron borrar los datos existentes."));
        }
        return RestaurarBackup(archivoBackup)
            .Bind(vehiculos => {
                var contador = 0;
                DomainError? primerError = null;

                foreach (var p in vehiculos) {
                    var result = createCallback(p);
                    if (result.IsSuccess)
                        contador++;
                    else if (primerError == null)
                        primerError = result.Error;
                }
                if (primerError != null && contador == 0)
                    return Result.Failure<int, DomainError>(primerError);

                _logger.Information("Restauración completada. Total registros: {count}", contador);
                return Result.Success<int, DomainError>(contador);
            });
    }
}