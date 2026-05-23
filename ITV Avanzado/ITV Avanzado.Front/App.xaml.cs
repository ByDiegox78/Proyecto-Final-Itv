using System.Windows;
using ITV_Avanzado.Front.Service.Dialogs;
using ITV_Avanzado.Front.View.AcercaDe;
using ITV_Avanzado.Front.View.Main;
using ITV_Avanzado.Front.ViewModels.BackUp;
using ITV_Avanzado.Front.ViewModels.ImportExport;
using ITV_Avanzado.Service.Dialogs;
using ITV_Avanzado.Front.ViewModels.Main;
using ITV_Avanzado.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ITV_Avanzado.Front;

public partial class App : Application {
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);

        Services = DependencesProvider.BuildServiceProvider(services => {
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<ImportExportViewModel>();
            services.AddSingleton<BackupViewModel>();
        });

        var mainWindow = new MainWindow {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}