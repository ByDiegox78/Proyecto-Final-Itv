using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV_Avanzado.Front.View.AcercaDe;
using ITV_Avanzado.Front.View.Backup;
using ITV_Avanzado.Front.View.Cita;
using ITV_Avanzado.Front.View.DashBoard;
using ITV_Avanzado.Front.View.Informe;
using ITV_Avanzado.Service.Buckup;
using ITV_Avanzado.Service.Citas;
using ITV_Avanzado.Service.Dialogs;
using ITV_Avanzado.Service.ImportExport;
using ITV_Avanzado.Service.Report;
using Serilog;

namespace ITV_Avanzado.Front.ViewModels.Main;

public partial class MainViewModel(
    ICitasService citasService,
    IBuckupService backupService,
    IReportService reportService,
    IImportExportService importExportService,
    IDialogService dialogService
) : ObservableObject {
    public delegate void NavigateDelegate(Page page);

    private readonly IBuckupService _backupService = backupService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly IImportExportService _importExportService = importExportService;

    private readonly ILogger _logger = Log.ForContext<MainViewModel>();
    
    private readonly ICitasService _citasService = citasService;
    private readonly IReportService _reportService = reportService;
    
    [ObservableProperty] private bool _isDarkTheme = true;
    
    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _statusMessage = "Listo";
    
    private void OnInitialized() {
        _logger.Information("✅ MainViewModel inicializado");
    }

    public event NavigateDelegate? OnNavigateRequested;
    
    [RelayCommand]
     private void NavigateToDashboard() {
         OnNavigateRequested?.Invoke(new DashBoardView());
     }

    [RelayCommand]
    private void NavigateToCitas() {
        OnNavigateRequested?.Invoke(new CitaView());
    }
    
    [RelayCommand]
    private void NavigateToInformes() {
        OnNavigateRequested?.Invoke(new InformeView());
    }

    [RelayCommand]
    private void NavigateToBackup() {
        OnNavigateRequested?.Invoke(new BackupView());
    }

    [RelayCommand]
    private void MostrarAcercaDe() {
        var aboutWindow = new AcercaDe();
        aboutWindow.ShowDialog();
    }
    
}
