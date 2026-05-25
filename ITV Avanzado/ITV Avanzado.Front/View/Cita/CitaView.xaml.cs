using System.Windows.Controls;
using System.Windows.Input;
using ITV_Avanzado.Front.Cita;
using ITV_Avanzado.Front.ViewModels.Cita;
using Microsoft.Extensions.DependencyInjection;

namespace ITV_Avanzado.Front.View.Cita;

public partial class CitaView : Page {
    public CitaView() {
        InitializeComponent();
        var vm = App.Services.GetRequiredService<CitaViewModel>();
        DataContext = vm;
    }

    // private void OnCitaClick(object sender, MouseButtonEventArgs e) {
    //     if (DataContext is CitaViewModel vm && vm.ViewCommand.Execute(null)) vm.ViewCommand.CanExecute(null);
    // }
    
}