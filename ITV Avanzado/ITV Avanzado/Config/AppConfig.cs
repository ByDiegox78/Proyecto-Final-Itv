using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace ITV_Avanzado.Config;

/// <summary>
///     Clase de configuración que lee desde appsettings.json.
/// </summary>
public class AppConfig {
    static AppConfig() {
        Config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
    public static IConfiguration Config { get; }
    
    public static CultureInfo Locale => CultureInfo.GetCultureInfo("es-ES");
    
    public static string DataFolder => Path.Combine(
        Environment.CurrentDirectory, 
        Config.GetValue<string>("Repository:Directory") ?? "data");
    
    public static string StorageType => Config.GetValue<string>("Storage:Type") ?? "json";
    
    public static string VehiculoFile {
        get {
            var extension = StorageType.ToLower() switch {
                "json" => "json",
                "xml" => "xml",
                "csv" => "csv",
                "bin" => "bin",
                _ => "json" 
            };
            return Path.Combine(DataFolder, $"itv.{extension}");
        }
    }
    
    public static string RepositoryType {
        get {
            var type = Config.GetValue<string>("Repository:Type") ?? "memory";
            return type.ToLower() switch {
                "memory" => "memory",
                "binary" => "binary",
                "json" => "json",
                "ado" => "ado",
                "dapper" => "dapper",
                "efcore" => "efcore",
                _ => "memory"
            };
        }
    }
    
    public static int CacheSize => Config.GetValue("Cache:Size", 5);
    
    public static string ConnectionString => Config.GetValue<string>("Repository:ConnectionString") ?? "Data Source=data/vehiculos.db";
    
    public static bool DropData => Config.GetValue<bool>("Repository:DropData", false);
    
    public static bool SeedData => Config.GetValue<bool>("Repository:SeedData", true);

    public static string BackupFormat {
        get {
            var format = Config.GetValue<string>("Backup:Format") ?? "json";
            return format.ToLower() switch
            {
                "json" => "json",
                "xml" => "xml",
                "csv" => "csv",
                "bin" => "bin",
                _ => "json"
            };
        }
    }
    public static string BackupDirectory => Path.Combine(AppContext.BaseDirectory, Config.GetValue<string>("Backup:Directory") ?? "back");
}