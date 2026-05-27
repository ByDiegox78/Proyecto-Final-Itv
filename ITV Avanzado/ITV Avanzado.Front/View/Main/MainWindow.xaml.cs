using System.Windows;
using System.Windows.Controls;
using ITV_Avanzado.Config;
using ITV_Avanzado.Front.View.Backup;
using ITV_Avanzado.Front.View.Cita;
using ITV_Avanzado.Front.View.DashBoard;
using ITV_Avanzado.Front.View.EmportExport;
using ITV_Avanzado.Front.View.Informe;
using ITV_Avanzado.Front.ViewModels.ImportExport;
using ITV_Avanzado.Front.ViewModels.Main;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ITV_Avanzado.Front.View.Main;

public partial class MainWindow : Window {
    private bool _exitConfirmedViaMenu;
    public MainWindow() {
        InitializeComponent();
        
        var viewModel = App.Services.GetRequiredService<MainViewModel>();
        DataContext = viewModel;
        Log.Information("Main Inicializado");
        MainFrame.Navigate(new DashBoardView());
        DeleteTypeText.Text = $"Borrado: {(AppConfig.IsLogic ? "Logico" : "Fisico")}";
        Closing += (s, e) => {
            if (_exitConfirmedViaMenu) return;

            var result = MessageBox.Show(
                "¿Está seguro de que desea salir de la aplicación?",
                "Confirmar salida",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                e.Cancel = true;
            else
                _exitConfirmedViaMenu = true;
        };
    }
    private void OnNavigateRequested(Page page) {
        MainFrame.Navigate(page); //?
    }
    protected override void OnClosed(EventArgs e) {
        Log.Information("🏁 MainWindow cerrada");
        base.OnClosed(e);
        Application.Current.Shutdown();
    }
    private void OnSalirClick(object sender, RoutedEventArgs e) {
        var result = MessageBox.Show(
            "¿Está seguro de que desea salir de la aplicación?",
            "Confirmar salida",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes) {
            Log.Information("👋 Usuario cerró la aplicación desde el menú");
            _exitConfirmedViaMenu = true;
            Application.Current.Shutdown();
        }
    }
    private void OnExportarClick(object sender, RoutedEventArgs e) {
      MainFrame.Navigate(new ImportExportView());
    }
  

    private void OnCrearBackupClick(object sender, RoutedEventArgs e) {
      MainFrame.Navigate(new BackupView());
    }

    private void OnRestaurarBackupClick(object sender, RoutedEventArgs e) { 
        MainFrame.Navigate(new BackupView());
    }
    private void OnCitasClick(object sender, RoutedEventArgs e) {
           MainFrame.Navigate(new CitaView());
    }
    private void OnInformesClick(object sender, RoutedEventArgs e) {
        MainFrame.Navigate(new InformeView());
    }
    private void OnConfiguracionClick(object sender, RoutedEventArgs e) {
        var tipoBorrado = AppConfig.IsLogic ? "Lógico" : "Físico";
        MessageBox.Show(
            "Configuración de la aplicación\n\n" +
            $"Repositorio: {AppConfig.RepositoryType.ToUpper()}\n" +
            $"Storage: {AppConfig.StorageType.ToUpper()}\n" +
            $"Directorio: {AppConfig.DataFolder}\n" +
            $"Tipo de borrado: {tipoBorrado}",
            "Configuración",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    private void OnAcercaDeClick(object sender, RoutedEventArgs e) {
        var aboutWindow = new AcercaDe.AcercaDe();
        aboutWindow.Owner = this;
        aboutWindow.ShowDialog();
    }

    private void OnImportExportClick(object sender, RoutedEventArgs e) {
        MainFrame.Navigate(new ImportExportView());
    }

    private void OnDashboardClick(object sender, RoutedEventArgs e) { MainFrame.Navigate(new DashBoardView());
    }
    private void OnBackupClick(object sender, RoutedEventArgs e) {
        MainFrame.Navigate(new BackupView());
    }

}