using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV_Avanzado.Service.Citas;
using ITV_Avanzado.Service.Dialogs;
using ITV_Avanzado.Service.Report;
using Microsoft.Win32;
using Serilog;

namespace ITV_Avanzado.Front.ViewModels.Informes;

public partial class InformesViewModel(
    ICitasService citasService,
    IReportService reportService,
    IDialogService dialogService
) : ObservableObject {
    private readonly IDialogService _dialogService = dialogService;
    private readonly ILogger _logger = Log.ForContext<InformesViewModel>();
    private readonly ICitasService _citasService = citasService;
    private readonly IReportService _reportService = reportService;

     [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _mostrarEliminados;
    [ObservableProperty] private string _statusMessage = "";

    [RelayCommand]
    private void GenerarInformeGeneralPdf() {
        try {
            IsGenerating = true;
            StatusMessage = "Generando PDF...";

            var vehiculos = _citasService.GetAll(1, 1000, MostrarEliminados);
            var htmlResult = _reportService.GenerarInformeCitaHtml(vehiculos, MostrarEliminados);
            if (htmlResult.IsFailure) {
                _dialogService.ShowError(htmlResult.Error.Message);
                return;
            }

            var dialog = new SaveFileDialog {
                Filter = "PDF|*.pdf",
                FileName = $"Informe_ITV_{DateTime.Now:yyyyMMdd}"
            };

            if (dialog.ShowDialog() == true) {
                var result = _reportService.GuardarInformePdf(htmlResult.Value, dialog.FileName);
                if (result.IsSuccess) {
                    StatusMessage = "PDF generado";
                    _dialogService.ShowSuccess("Informe PDF generado correctamente");
                }
                else {
                    _dialogService.ShowError(result.Error.Message);
                }
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al generar PDF");
            StatusMessage = "Error al generar";
        }
        finally {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private void GenerarInformeGeneralHtml() {
        try {
            IsGenerating = true;
            StatusMessage = "Generando HTML...";

            var vehiculos = _citasService.GetAll(1, 1000, MostrarEliminados);
            var htmlResult = _reportService.GenerarInformeCitaHtml(vehiculos, MostrarEliminados);
            if (htmlResult.IsFailure) {
                _dialogService.ShowError(htmlResult.Error.Message);
                return;
            }

            var dialog = new SaveFileDialog {
                Filter = "HTML|*.html",
                FileName = $"Informe_ITV_{DateTime.Now:yyyyMMdd}"
            };

            if (dialog.ShowDialog() == true) {
                var result = _reportService.GuardarInformeHtml(htmlResult.Value, dialog.FileName);
                if (result.IsSuccess) {
                    StatusMessage = "HTML guardado";
                    _dialogService.ShowSuccess("Informe HTML generado correctamente");
                }
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al generar HTML");
            StatusMessage = "Error al generar";
        }
        finally {
            IsGenerating = false;
        }
    }
}