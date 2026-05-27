using System.Windows.Controls;
using ITV_Avanzado.Front.ViewModels.BackUp;
using Microsoft.Extensions.DependencyInjection;

namespace ITV_Avanzado.Front.View.Backup;

public partial class BackupView : Page {
    public BackupView() {
        InitializeComponent();
        var vm = App.Services.GetRequiredService<BackupViewModel>();
        DataContext = vm;
    }
}