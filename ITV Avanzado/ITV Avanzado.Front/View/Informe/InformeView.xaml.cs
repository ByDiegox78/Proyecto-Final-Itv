using System.Windows.Controls;
using ITV_Avanzado.Front.ViewModels.Informes;
using Microsoft.Extensions.DependencyInjection;

namespace ITV_Avanzado.Front.View.Informe;

public partial class InformeView : Page {
    public InformeView() {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<InformesViewModel>();
    }
}