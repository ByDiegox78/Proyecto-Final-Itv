using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionItv.Models;
using ITV_Avanzado.Config;
using ITV_Avanzado.Service.Citas;
using ITV_Avanzado.Service.Dialogs;
using ITV_Avanzado.Service.ImportExport;
using Microsoft.Win32;
using Serilog;

namespace ITV_Avanzado.Front.ViewModels.ImportExport;

public partial class ImportExportViewModel(
    ICitasService citasService,
    IImportExportService importExportService,
    IDialogService dialogService
) : ObservableObject {
    private readonly IDialogService _dialogService = dialogService;
    private readonly IImportExportService _importExportService = importExportService;
    private readonly ILogger _logger = Log.ForContext<ImportExportViewModel>();
    private readonly ICitasService _citasService = citasService;
    
    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _statusMessage = "";

    [ObservableProperty] private bool _sustituirDatos;
    
    [RelayCommand]
    private void ExportarCsv() {
        try {
            IsLoading = true;
            StatusMessage = "Exportando datos...";
            
            var dialog = new SaveFileDialog {
                Filter = "CSV|*.csv",
                FileName = $"Exportacion_{DateTime.Now:yyyyMMdd}"
            };

            if (dialog.ShowDialog() == true) {
                var citas = _citasService.GetAll(page: 1, pageSize: 1000, includeDeleted: false);
                var csvPath = Path.Combine(AppConfig.DataFolder, "citas.csv");
                var result = _importExportService.ExportarDatos(citas, csvPath);

                if (result.IsSuccess) {
                    File.Copy(Path.Combine(AppConfig.DataFolder, "citas.csv"), dialog.FileName, true);
                    StatusMessage = $"Exportados {result.Value} registros";
                    _dialogService.ShowSuccess($"Exportación completada\n{result.Value} registros");
                }
                else {
                    _dialogService.ShowError(result.Error.Message);
                    StatusMessage = "Error al exportar";
                }
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al exportar");
            StatusMessage = "Error al exportar";
        }
        finally {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private void ImportarCsv() {
        try {
            var dialog = new OpenFileDialog {
                Filter = "CSV|*.csv",
                Title = "Seleccionar archivo CSV"
            };

            if (dialog.ShowDialog() != true) return;

            IsLoading = true;
            StatusMessage = "Importando datos...";

            if (SustituirDatos) _citasService.DeleteAll();

            var result = _importExportService.ImportarDatosSistema(dialog.FileName);

            if (result.IsSuccess) {
                var count = result.Value.Count();
                StatusMessage = $"Importados {count} registros";
                _dialogService.ShowSuccess($"Importación completada\n{count} registros");
            }
            else {
                _dialogService.ShowError(result.Error.Message);
                StatusMessage = "Error al importar";
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al importar");
            _dialogService.ShowError($"Error al importar: {ex.Message}");
            StatusMessage = "Error al importar";
        }
        finally {
            IsLoading = false;
        }
    }
     [RelayCommand]
    private void ExportarJson() {
        try {
            IsLoading = true;
            StatusMessage = "Exportando JSON...";

            var dialog = new SaveFileDialog {
                Filter = "JSON|*.json",
                FileName = $"Exportacion_{DateTime.Now:yyyyMMdd}"
            };

            if (dialog.ShowDialog() == true) {
                var personas = _citasService.GetAll(1, 1000, false);
                var options = new JsonSerializerOptions {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var json = JsonSerializer.Serialize(personas, options);
                File.WriteAllText(dialog.FileName, json);

                StatusMessage = "Exportación JSON completada";
                _dialogService.ShowSuccess("Exportación JSON completada");
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al exportar JSON");
            StatusMessage = "Error al exportar";
        }
        finally {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ImportarJson() {
        try {
            var dialog = new OpenFileDialog {
                Filter = "JSON|*.json",
                Title = "Seleccionar archivo JSON"
            };

            if (dialog.ShowDialog() != true) return;

            IsLoading = true;
            StatusMessage = "Importando JSON...";

            if (SustituirDatos) _citasService.DeleteAll();

            var json = File.ReadAllText(dialog.FileName);
            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var personas = JsonSerializer.Deserialize<IEnumerable<GestionItv.Models.Cita>>(json, options);

            if (personas != null) {
                var count = 0;
                foreach (var persona in personas) {
                    var result = _citasService.Save(persona);
                    if (result.IsSuccess) count++;
                }

                StatusMessage = $"Importados {count} registros";
                _dialogService.ShowSuccess($"Importación completada\n{count} registros");
            }
            else {
                _dialogService.ShowError("El archivo JSON no tiene un formato válido");
                StatusMessage = "Error al importar";
            }
        }
        catch (JsonException ex) {
            _logger.Error(ex, "Error al importar JSON - formato inválido");
            _dialogService.ShowError($"Error al importar JSON: El formato del archivo no es válido.\n\nDetalles: {ex.Message}");
            StatusMessage = "Error al importar";
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al importar JSON");
            _dialogService.ShowError($"Error al importar: {ex.Message}");
            StatusMessage = "Error al importar";
        }
        finally {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private void ExportarXml() {
        try {
            IsLoading = true;
            StatusMessage = "Exportando XML...";

            var dialog = new SaveFileDialog {
                Filter = "XML|*.xml",
                FileName = $"Exportacion_{DateTime.Now:yyyyMMdd}"
            };

            if (dialog.ShowDialog() == true) {
                var personas = _citasService.GetAll(1, 1000, false);
                var xmlSerializer = new XmlSerializer(typeof(List<GestionItv.Models.Cita>));
                using var writer = new StreamWriter(dialog.FileName);
                xmlSerializer.Serialize(writer, personas.ToList());

                StatusMessage = "Exportación XML completada";
                _dialogService.ShowSuccess("Exportación XML completada");
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al exportar XML");
            StatusMessage = "Error al exportar";
        }
        finally {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ImportarXml() {
        try {
            var dialog = new OpenFileDialog {
                Filter = "XML|*.xml",
                Title = "Seleccionar archivo XML"
            };

            if (dialog.ShowDialog() != true) return;

            IsLoading = true;
            StatusMessage = "Importando XML...";

            if (SustituirDatos) _citasService.DeleteAll();

            var xmlSerializer = new XmlSerializer(typeof(List<GestionItv.Models.Cita>));
            using var reader = new StreamReader(dialog.FileName);
            var personas = (List<GestionItv.Models.Cita>?)xmlSerializer.Deserialize(reader);

            if (personas != null) {
                var count = 0;
                foreach (var persona in personas) {
                    var result = _citasService.Save(persona);
                    if (result.IsSuccess) count++;
                }

                StatusMessage = $"Importados {count} registros";
                _dialogService.ShowSuccess($"Importación completada\n{count} registros");
            }
            else {
                _dialogService.ShowError("El archivo XML no tiene un formato válido");
                StatusMessage = "Error al importar";
            }
        }
        catch (InvalidOperationException ex) {
            _logger.Error(ex, "Error al importar XML - formato inválido");
            _dialogService.ShowError(
                $"Error al importar XML: El formato del archivo no es válido.\n\nDetalles: {ex.Message}");
            StatusMessage = "Error al importar";
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al importar XML");
            _dialogService.ShowError($"Error al importar: {ex.Message}");
            StatusMessage = "Error al importar";
        }
        finally {
            IsLoading = false;
        }
    }
    [RelayCommand]
private void ExportarBinario() {
    try {
        IsLoading = true;
        StatusMessage = "Exportando binario...";

        var dialog = new SaveFileDialog {
            Filter = "Binario|*.bin",
            FileName = $"Exportacion_{DateTime.Now:yyyyMMdd}"
        };

        if (dialog.ShowDialog() == true) {
            var citas = _citasService.GetAll(page: 1, pageSize: 1000, includeDeleted: false);
            var binPath = Path.Combine(AppConfig.DataFolder, "citas.bin");
            var result = _importExportService.ExportarDatos(citas, binPath);

            if (result.IsSuccess) {
                File.Copy(binPath, dialog.FileName, true);
                StatusMessage = $"Exportados {result.Value} registros";
                _dialogService.ShowSuccess($"Exportación completada\n{result.Value} registros");
            }
            else {
                _dialogService.ShowError(result.Error.Message);
                StatusMessage = "Error al exportar";
            }
        }
    }
    catch (Exception ex) {
        _logger.Error(ex, "Error al exportar binario");
        StatusMessage = "Error al exportar";
    }
    finally {
        IsLoading = false;
    }
}

[RelayCommand]
private void ImportarBinario() {
    try {
        var dialog = new OpenFileDialog {
            Filter = "Binario|*.bin",
            Title = "Seleccionar archivo binario"
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        StatusMessage = "Importando binario...";

        if (SustituirDatos) _citasService.DeleteAll();

        var result = _importExportService.ImportarDatosSistema(dialog.FileName);

        if (result.IsSuccess) {
            var count = result.Value.Count();
            StatusMessage = $"Importados {count} registros";
            _dialogService.ShowSuccess($"Importación completada\n{count} registros");
        }
        else {
            _dialogService.ShowError(result.Error.Message);
            StatusMessage = "Error al importar";
        }
    }
    catch (Exception ex) {
        _logger.Error(ex, "Error al importar binario");
        _dialogService.ShowError($"Error al importar: {ex.Message}");
        StatusMessage = "Error al importar";
    }
    finally {
        IsLoading = false;
    }
}

}