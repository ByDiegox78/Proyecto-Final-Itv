using ITV_Avanzado.Front.Cita;
using ITV_Avanzado.Front.Service.Dialogs;
using ITV_Avanzado.Front.ViewModels.BackUp;
using ITV_Avanzado.Front.ViewModels.Cita;
using ITV_Avanzado.Front.ViewModels.DashBoard;
using ITV_Avanzado.Front.ViewModels.ImportExport;
using ITV_Avanzado.Front.ViewModels.Informes;
using ITV_Avanzado.Front.ViewModels.Main;
using ITV_Avanzado.Infrastructure;
using ITV_Avanzado.Service.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ITV_Avanzado.Front.Infraestructure;

public static class FrontDependenciesProvider {
    public static IServiceProvider BuildServiceProvider() {
        Log.Information("Configurando servicios (Back + Front)...");

        var serviceProvider = DependencesProvider.BuildServiceProvider(services => {
            RegisterViewModels(services);
            Log.Information("ViewModels registradas desde Front");
        });

        Log.Information("Servicios configurados correctamente");
        return serviceProvider;
    }

    private static void RegisterViewModels(IServiceCollection services) {
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ImportExportViewModel>();
        services.AddSingleton<BackupViewModel>();
        services.AddSingleton<InformesViewModel>();
        services.AddSingleton<DashBoardViewModel>();
        services.AddSingleton<CitaViewModel>();
        services.AddSingleton<CitaItemViewModel>();
        services.AddSingleton<CitaEditVieModel>();
    }
}