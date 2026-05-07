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
using ITV_Avanzado.Storage.Binary;
using ITV_Avanzado.Storage.Common;
using ITV_Avanzado.Storage.Csv;
using ITV_Avanzado.Storage.Json;
using ITV_Avanzado.Storage.Xml;
using ITV_Avanzado.Validator.Common;
using ITV_Avanzado.Validator.Vehiculos;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace ITV_Avanzado.Infrastructure;

public class DependencesProvider {
    public static IServiceProvider BuildServiceProvider() {
        var service = new ServiceCollection();
        
        //CleanData();
        
        return service.BuildServiceProvider();

    }

    private void RegistreStorage(IServiceCollection service) {
        service.AddTransient<IStorage<Vehiculo>>(sp => {
            var type = AppConfig.StorageType.ToLower();
            return type switch {
                "json" => new VehiculoJsonStorage(),
                "csv" => new VehiculoCsvStorage(),
                "bin" or "binary" => new VehiculoBinaryStorage(),
                "xml" => new VehiculoXmlStorage(),
                _ => new VehiculoJsonStorage()
            };
        });
    }
    
    private static void RegisterRepositories(IServiceCollection services) {
        // Registrar repositorio de personas según configuración
        services.AddSingleton<IVehiculosRepository>(sp => {
            var repoType = AppConfig.RepositoryType.ToLower();
            return repoType switch {
                "memory" => new VehiculosRespositoryMemory(AppConfig.DropData, AppConfig.SeedData),
                "json" => new VehiculoJsonRepository(
                    Path.Combine(AppConfig.DataFolder, "itv.json"),
                    AppConfig.DropData,
                    AppConfig.SeedData),
                "binary" => new VehiculoBinaryRepository(Path.Combine(AppConfig.DataFolder, "itv.bin"), AppConfig.DropData, AppConfig.SeedData),
                "dapper" => CreateDapperRepository(AppConfig.DropData, AppConfig.SeedData),
                "efcore" => CreateEfRepository(AppConfig.DropData, AppConfig.SeedData),
                "ado" => new VehiculoAdoRepository(AppConfig.DropData, AppConfig.SeedData),
                _ => new VehiculosRespositoryMemory(AppConfig.DropData, AppConfig.SeedData)
            };
        });
    }
    private static VehiculoEfCoreRepository CreateEfRepository(bool dropData, bool seedData) {
        var dataFolder = AppConfig.DataFolder;
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);
        
        var dbPath = Path.Combine(dataFolder, "itv");
        var context = new AppDbContext($"Data Source={dbPath}");
        
        return new VehiculoEfCoreRepository(context, dropData, seedData);
    }

    private static VehiculoDapperRepository CreateDapperRepository(bool data, bool seed) {
        var dataFolder = AppConfig.DataFolder;
        if (!Directory.Exists(dataFolder)) {
            Directory.CreateDirectory(dataFolder);
        }

        var dbPath = Path.Combine(dataFolder, "itv-.db");
        var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        return new VehiculoDapperRepository(connection, () => connection.Close(), data, seed);
    }

    private static void RegistesValidators(IServiceCollection service) {
        service.AddTransient<IValidator<Vehiculo>, ValidadorVehiculo>();
    }
    private static void RegisterCaches(IServiceCollection services) {
        services.AddSingleton<ICache<int, Vehiculo>>(sp =>
            new CacheLru<int, Vehiculo>(AppConfig.CacheSize));
    }
    

    // private static void CleanData() {
    //     if (AppConfig.DropData || AppConfig.SeedData) {
    //         CleanDirectory(AppConfig.ReportDirectory);
    //         CleanDirectory(AppConfig.ImagesDirectory);
    //     }
    // }

    private static void CleanDirectory(string path) {
        try {
            if (Directory.Exists(path)) {
                foreach (var v in Directory.GetFiles(path)) {
                    try {
                        File.Delete(v);
                    } catch {
                     
                    }

                    foreach (var f in Directory.GetDirectories(path)) {
                        try {
                            Directory.Delete(f, true);
                        } catch (Exception e) {
                            
                        }
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