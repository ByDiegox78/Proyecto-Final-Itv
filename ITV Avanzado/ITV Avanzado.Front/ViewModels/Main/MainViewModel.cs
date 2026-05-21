// using System.Net.Mime;
// using System.Windows;
// using System.Windows.Controls;
// using CommunityToolkit.Mvvm.ComponentModel;
// using CommunityToolkit.Mvvm.Input;
// using ITV_Avanzado.Service.Buckup;
// using ITV_Avanzado.Service.Citas;
// using ITV_Avanzado.Service.Dialogs;
// using ITV_Avanzado.Service.ImportExport;
// using ITV_Avanzado.Service.Report;
// using Serilog;
//
// namespace ITV_Avanzado.Front.ViewModels.Main;
//
// public class MainViewModel(
//     ICitasService citasService,
//     IBuckupService backupService,
//     IReportService reportService,
//     IImportExportService importExportService,
//     IDialogService dialogService
// ) : ObservableObject {
//     public delegate void NavigateDelegate(Page page);
//
//     private readonly IBuckupService _backupService = backupService;
//     private readonly IDialogService _dialogService = dialogService;
//     private readonly IImportExportService _importExportService = importExportService;
//
//     private readonly ILogger _logger = Log.ForContext<MainViewModel>();
//     
//     private readonly ICitasService _citasService = citasService;
//     private readonly IReportService _reportService = reportService;
//     
//     [ObservableProperty] private bool _isDarkTheme = true;
//     
//     [ObservableProperty] private bool _isLoading;
//
//     [ObservableProperty] private string _statusMessage = "Listo";
//     
//     private void OnInitialized() {
//         _logger.Information("✅ MainViewModel inicializado");
//     }
//
//     public event NavigateDelegate? OnNavigateRequested;
//     
//     [RelayCommand]
//     private void NavigateToDashboard() {
//         OnNavigateRequested?.Invoke(new DashboardView());
//     }
//
//     [RelayCommand]
//     private void NavigateToCitas() {
//         OnNavigateRequested?.Invoke(new CitaView());
//     }
//     
//     [RelayCommand]
//     private void NavigateToInformes() {
//         OnNavigateRequested?.Invoke(new InformeView());
//     }
//
//     [RelayCommand]
//     private void NavigateToGraficos() {
//         OnNavigateRequested?.Invoke(new GraficoView());
//     }
//
//     [RelayCommand]
//     private void NavigateToBackup() {
//         OnNavigateRequested?.Invoke(new BackupView());
//     }
//
//     [RelayCommand]
//     private void NavigateToImportExport() {
//         OnNavigateRequested?.Invoke(new ImportExportView());
//     }
//     
//     [RelayCommand]
//     private void CambiarTema() {
//         IsDarkTheme = !IsDarkTheme;
//         ApplyTheme(IsDarkTheme ? "Dark" : "Light");
//     }
//
//     [RelayCommand]
//     private void Salir() {
//         if (_dialogService.ShowConfirmation("¿Estás seguro de que quieres salir?", "Confirmar salida")) {
//             _logger.Information("👋 Usuario cerró la aplicación");
//             MediaTypeNames.Application.Current.Shutdown();
//         }
//     }
//
//     [RelayCommand]
//     private void MostrarAcercaDe() {
//         var aboutWindow = new AcercaDe();
//         aboutWindow.ShowDialog();
//     }
//     
//     private void ApplyTheme(string themeName) {
//         try {
//             var themeUri = new Uri($"../Themes/{themeName}Theme.xaml", UriKind.Relative);
//             var themeDictionary = new ResourceDictionary { Source = themeUri };
//
//             var appResources = MediaTypeNames.Application.Current.Resources.MergedDictionaries;
//
//             for (var i = appResources.Count - 1; i >= 0; i--) {
//                 var dict = appResources[i];
//                 if (dict.Source != null && dict.Source.OriginalString.Contains("Theme")) appResources.RemoveAt(i);
//             }
//
//             appResources.Add(themeDictionary);
//
//             _logger.Information("✅ Tema cambiado a {Theme}", themeName);
//         }
//         catch (Exception ex) {
//             _logger.Error(ex, "❌ Error al aplicar el tema");
//         }
//     }
// }