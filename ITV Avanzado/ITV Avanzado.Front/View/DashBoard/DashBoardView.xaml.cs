using System.Windows.Controls;
using ITV_Avanzado.Front.ViewModels.DashBoard;
using Microsoft.Extensions.DependencyInjection;

namespace ITV_Avanzado.Front.View.DashBoard;

public partial class DashBoardView : Page {
    public DashBoardView() {
        InitializeComponent();
        var vm = App.Services.GetRequiredService<DashBoardViewModel>();
        DataContext = vm;
    }
}