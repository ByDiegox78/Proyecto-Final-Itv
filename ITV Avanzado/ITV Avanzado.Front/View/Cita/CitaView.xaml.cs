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
    
}