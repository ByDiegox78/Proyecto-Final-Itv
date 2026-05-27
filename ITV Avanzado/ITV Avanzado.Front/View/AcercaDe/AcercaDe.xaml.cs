using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ITV_Avanzado.Front.View.AcercaDe;

public partial class AcercaDe : Window {
    public AcercaDe() {
        InitializeComponent();
    }

    private void OnGithubClick(object sender, MouseButtonEventArgs e) {
        try {
            Process.Start(new ProcessStartInfo {
                FileName = "https://github.com/ByDiegox78",
                UseShellExecute = true
            });
        }
        catch {
            Clipboard.SetText("https://github.com/ByDiegox78");
            MessageBox.Show("El enlace se a copiado.","GitHub", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OnCerrar(object sender, RoutedEventArgs e) {
        Close();
    }
}