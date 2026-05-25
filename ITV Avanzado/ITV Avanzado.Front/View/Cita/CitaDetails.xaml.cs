using System.Windows;

namespace ITV_Avanzado.Front.View.Cita;

public partial class CitaDetails : Window {
    public CitaDetails() {
        InitializeComponent();
    }

    private void OnVolverPanelClick(object sender, RoutedEventArgs e) {
        Close();
    }
}