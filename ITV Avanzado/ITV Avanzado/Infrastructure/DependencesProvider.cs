using GestionItv.Models;
using ITV_Avanzado.Cache;
using ITV_Avanzado.Config;
using ITV_Avanzado.Entity;
using ITV_Avanzado.Repository.Ado;
using ITV_Avanzado.Repository.Binary;
using ITV_Avanzado.Repository.Common;
using ITV_Avanzado.Repository.Dapper;
using ITV_Avanzado.Repository.EfCore;
using ITV_Avanzado.Repository.Json;
using ITV_Avanzado.Repository.Memory;
using ITV_Avanzado.Service.Buckup;
using ITV_Avanzado.Service.Citas;
using ITV_Avanzado.Service.ImportExport;
using ITV_Avanzado.Service.Report;
using ITV_Avanzado.Storage.Binary;
using ITV_Avanzado.Storage.Common;
using ITV_Avanzado.Storage.Csv;
using ITV_Avanzado.Storage.Json;
using ITV_Avanzado.Storage.Xml;
using ITV_Avanzado.Validator.Citas;
using ITV_Avanzado.Validator.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace ITV_Avanzado.Infrastructure;
/// <summary>
///     Proveedor de dependencias centralizado para toda la aplicación.
///     Configura la inyección de dependencias registrando repositorios, servicios, validadores y caches.
/// </summary>
public class DependencesProvider {
    /// <summary>
    ///     Construye y configura el contenedor de inyección de dependencias.
    /// </summary>
    /// <param name="configureAdditional">Callback opcional para registrar servicios adicionales.</param>
    /// <returns>Proveedor de servicios configurado.</returns>
    public static IServiceProvider BuildServiceProvider(Action<IServiceCollection>? configureAdditional = null) {
        var services = new ServiceCollection();
        CleanData();

        RegisterCaches(services);
        RegisterValidators(services);
        RegisterStorages(services);
        RegisterRepositories(services);
        RegisterServices(services);
        
        configureAdditional?.Invoke(services);
        
        return services.BuildServiceProvider();

    }
    /// <summary>
    ///     Registra el almacenamiento según la configuración del appsettings.json.
    /// </summary>
    private static void RegisterStorages(IServiceCollection service) {
        service.AddTransient<IStorage<Cita>>(sp => {
            var type = AppConfig.StorageType.ToLower();
            return type switch {
                "json" => new CitasJsonStorage(),
                "csv" => new CitasCsvStorage(),
                "bin" or "binary" => new CitasBinaryStorage(),
                "xml" => new CitasXmlStorage(),
                _ => new CitasJsonStorage()
            };
        });
    }
    /// <summary>
    ///     Registra el repositorio de citas según la configuración del appsettings.json.
    ///     Permite intercambiar entre Memory, JSON, Binary, Dapper, EFCore, Ado.
    /// </summary>
    private static void RegisterRepositories(IServiceCollection services) {
        // Registrar repositorio de citas según configuración
        services.AddSingleton<ICitaRepository>(sp => {
            var repoType = AppConfig.RepositoryType.ToLower();
            return repoType switch {
                "memory" => new CitaRespositoryMemory(AppConfig.DropData, AppConfig.SeedData),
                "json" => new CitaJsonRepository(
                    Path.Combine(AppConfig.DataFolder, "itv.json"),
                    AppConfig.DropData,
                    AppConfig.SeedData),
                "binary" => new CitaBinaryRepository(Path.Combine(AppConfig.DataFolder, "itv.bin"), AppConfig.DropData, AppConfig.SeedData),
                "dapper" => CreateDapperRepository(AppConfig.DropData, AppConfig.SeedData),
                "efcore" => CreateEfRepository(AppConfig.DropData, AppConfig.SeedData),
                "ado" => new CitaAdoRepository(AppConfig.DropData, AppConfig.SeedData),
                _ => new CitaRespositoryMemory(AppConfig.DropData, AppConfig.SeedData)
            };
        });
    }
    /// <summary>
    ///     Crea el repositorio EfCore con conexión SQLite.
    /// </summary>
    private static CitaEfCoreRepository CreateEfRepository(bool dropData, bool seedData) {
        var dataFolder = AppConfig.DataFolder;
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);
        
        var dbPath = Path.Combine(dataFolder, "itv");
        var context = new AppDbContext($"Data Source={dbPath}");
        
        return new CitaEfCoreRepository(context, dropData, seedData);
    }
    /// <summary>
    ///     Crea el repositorio Dapper con conexión SQLite.
    /// </summary>
    private static CitaDapperRepository CreateDapperRepository(bool data, bool seed) {
        var dataFolder = AppConfig.DataFolder;
        if (!Directory.Exists(dataFolder)) {
            Directory.CreateDirectory(dataFolder);
        }

        var dbPath = Path.Combine(dataFolder, "itv-.db");
        var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        return new CitaDapperRepository(connection, () => connection.Close(), data, seed);
    }

    /// <summary>
    ///     Registra el validador de citas.
    /// </summary>
    private static void RegisterValidators(IServiceCollection service) {
        service.AddTransient<IValidator<Cita>, ValidadorCitas>();
    }
    /// <summary>
    ///     Registra la caché LRU para citas con el tamaño configurado.
    /// </summary>
    private static void RegisterCaches(IServiceCollection services) {
        services.AddSingleton<ICache<int, Cita>>(sp =>
            new CacheLru<int, Cita>(AppConfig.CacheSize));
    }
    
    /// <summary>
    ///     Limpia los directorios de salida si se requiere regenerar datos.
    /// </summary>
    private static void CleanData() {
        if (AppConfig.DropData || AppConfig.SeedData) {
            CleanDirectory(AppConfig.ReportDirectory);
        }
    }

    /// <summary>
    ///     Registra los servicios de negocio: Backup, Reportes, Import/Export y Citas.
    /// </summary>
    private static void RegisterServices(IServiceCollection services) {
        services.AddTransient<IBuckupService, BackupService>(sp =>
            new BackupService(sp.GetRequiredService<IStorage<Cita>>(), AppConfig.BackupDirectory));

        services.AddTransient<IReportService, ReportService>(sp =>
            new ReportService(AppConfig.ReportDirectory));

        services.AddTransient<IImportExportService, ImportExportService>();

        services.AddScoped<ICitasService, CitasService>(sp =>
            new CitasService(
                sp.GetRequiredService<ICitaRepository>(),
                sp.GetRequiredService<IValidator<Cita>>(),
                sp.GetRequiredService<ICache<int, Cita>>()
            )
        );
    }
    /// <summary>
    ///     Vacía un directorio eliminando todos los archivos y subdirectorios.
    /// </summary>
    /// <param name="path">Ruta del directorio a limpiar.</param>
    private static void CleanDirectory(string path) {
        try {
            if (Directory.Exists(path)) {
                foreach (var v in Directory.GetFiles(path)) {
                    try { File.Delete(v); } catch { }
                    foreach (var f in Directory.GetDirectories(path)) {
                        try { Directory.Delete(f, true); } catch { }
                    }
                }
            }
            Directory.CreateDirectory(path);
        }
        catch (Exception e) {
            Console.WriteLine($"Warning: No se pudo limpiar directorio {path}: {e.Message}");
        }
    }
}