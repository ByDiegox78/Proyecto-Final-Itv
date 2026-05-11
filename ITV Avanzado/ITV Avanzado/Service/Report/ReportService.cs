using System.Text;
using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Error.Report;
using ITV_Avanzado.Error.Vehiculos;
using Serilog;

namespace ITV_Avanzado.Service.Report;

public class ReportService : IReportService{
    private const string DateFormat = "dd/MM/yyyy";
    private readonly ILogger _logger = Log.ForContext<ReportService>();
    private readonly string _reportDirectory;
    
    public ReportService(string reportDirectory) {
        _reportDirectory = reportDirectory;
        _logger.Debug("Inicializando la clase ReportService con directorio {Directory}", _reportDirectory);
    }
    
    public Informe GenerarInformeCita(IEnumerable<Vehiculo> citas) {
        _logger.Information("Generando modelo informe de citas.");

        var list = citas.ToList();

        return new Informe {
            ListadoCitas = list.OrderBy(c => c.FechaInspeccion),

            TotalCitas = list.Count,

            Gasolina = list.Count(c => c.TipoMotor == Motor.Gasolina),
            Diesel = list.Count(c => c.TipoMotor == Motor.Diesel),
            Hibrido = list.Count(c => c.TipoMotor == Motor.Hibrido),
            Electrico = list.Count(c => c.TipoMotor == Motor.Electrico),

            CitasParaHoy = list.Count(c => c.FechaInspeccion == DateTime.UtcNow.Date)
        };
    }

    public Result<string, DomainError> GenerarInformeCitaHtml(IEnumerable<Vehiculo> citas, bool mostrarEliminados = false) {
        try {
            var list = mostrarEliminados ? citas : citas.Where(c => !c.IsDeleted);
            var stats = GenerarInformeCita(citas);
            
            var html = $@"
        <html>
        <head>
            <style>
                body {{ font-family: sans-serif; margin: 40px; color: #333; }}
                h1 {{ color: #2c3e50; border-bottom: 2px solid #3498db; }}
                .summary {{ background: #f8f9fa; padding: 15px; border-radius: 8px; margin-bottom: 20px; }}
                table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                th {{ background: #3498db; color: white; padding: 12px; text-align: left; }}
                td {{ padding: 10px; border-bottom: 1px solid #ddd; }}
                tr:nth-child(even) {{ background: #f2f2f2; }}
                .badge-eco {{ background: #2ecc71; color: white; padding: 4px 8px; border-radius: 4px; font-size: 0.8em; }}
            </style>
        </head>
        <body>
            <h1>Resumen de Citas ITV</h1>
            <div class='summary'>
                <p><strong>Total de Citas:</strong> {stats.TotalCitas}</p>
                <p><strong>Citas para Hoy:</strong> {stats.CitasParaHoy}</p>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>Fecha</th>
                        <th>Matrícula</th>
                        <th>Vehículo</th>
                        <th>Motor</th>
                        <th>DNI Propietario</th>
                    </tr>
                </thead>
                <tbody>
                    {string.Join("", list.Select(c => $@"
                    <tr>
                        <td>{c.FechaInspeccion:dd/MM/yyyy HH:mm}</td>
                        <td><strong>{c.Matricula}</strong></td>
                        <td>{c.Marca}</td>
                        <td>{c.TipoMotor} {(c.TipoMotor == Motor.Electrico || c.TipoMotor == Motor.Hibrido ? "<span class='badge-eco'>ECO</span>" : "")}</td>
                        <td>{c.DniPropietario}</td>
                    </tr>"))}
                </tbody>
            </table>
        </body>
        </html>";
            return Result.Success<string, DomainError>(html);
        }
        catch (Exception e) {
            return Result.Failure<string, DomainError>(VehiculoErrors.DatabaseError($"Error al generar HTML: {e.Message}"));
        }
    }

    public Result<bool, DomainError> GuardarInformeHtml(string html, string fileName) {
        try {
            File.WriteAllText(fileName, html);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            return Result.Failure<bool, DomainError>(VehiculoErrors.DatabaseError($"No se pudo guardar el archivo: {ex.Message}"));
        }
    }

    public Result<bool, DomainError> GuardarInforme(string html, string fileName) {
        var directory = _reportDirectory;
        _logger.Information("Guardando informe en directorio {Directory}", directory);

        try {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, fileName);
            File.WriteAllText(filePath, html, Encoding.UTF8);

            _logger.Information("Informe guardado correctamente en {FilePath}", filePath);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al guardar informe");
            return Result.Failure<bool, DomainError>(
                ReportErrors.SaveError(ex.Message));
        }
    }

    public Result<bool, DomainError> GuardarInformePdf(string html, string fileName) {
        try {
            _logger.Information("Iniciando conversión de HTML a PDF para {FileName}", fileName);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            return Result.Failure<bool, DomainError>(VehiculoErrors.DatabaseError($"Error en conversión PDF: {ex.Message}"));
        }
    }
}