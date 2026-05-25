using System.Windows;
using ITV_Avanzado.Front.ViewModels.Cita;

namespace ITV_Avanzado.Front.View.Cita;

public partial class CitaEdit : Window {
    public CitaEdit() {
        InitializeComponent();
    }

    protected override void OnContentRendered(EventArgs e) {
        base.OnContentRendered(e);
        if (DataContext is CitaEditVieModel vm) {
            vm.CloseAction = result => {
                DialogResult = result;
                Close();
            };
        }
    }
}