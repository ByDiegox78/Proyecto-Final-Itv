using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using ITV_Avanzado.Config;
using ITV_Avanzado.Front.Infraestructure;
using ITV_Avanzado.Front.View.Main;
using Serilog;
using Serilog.Debugging;

namespace ITV_Avanzado.Front;

public partial class App : Application {
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e) {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        
        ConfigureSerilog();
        
        Log.Information("Aplicacion WPF iniciada");
        
        Services = FrontDependenciesProvider.BuildServiceProvider(); 


        var mainWindow = new MainWindow();
        MainWindow = mainWindow;

        Log.Information("Llamando a mainWindow.Show()");
        mainWindow.Show();

        base.OnStartup(e);

        Log.Information("mainWindow.Show() completado");
        
        mainWindow.Show();
    }
    private void ConfigureSerilog() {
        // Habilitar SelfLog para depuración de Serilog
        SelfLog.Enable(msg => Debug.WriteLine($"SERILOG DIAG: {msg}"));

        // Configurar logger desde JSON
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(AppConfig.Config)
            .Enrich.FromLogContext()
            .CreateLogger();

        Log.Information("Serilog inicializado desde JSON");
    }
    private void ConfigureExceptionHandling() {
        // Excepciones en el hilo de UI
        DispatcherUnhandledException +=  OnDispatcherUnhandledException;
        // Excepciones en el hilo principal
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        // Excepciones en tareas asíncronas 
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }
    
    private void  OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
        Log.Fatal(e.Exception, "Exception no manejada");
        MessageBox.Show(
            $"Erro: {e.Exception.Message}",
            "Error",
            MessageBoxButton.OK, 
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e) {
        Log.Fatal(e.ExceptionObject as Exception, "Exposicion no manejada");
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) {
        Log.Error(e.Exception, "Exception en tarea");
        e.SetObserved();
    }
    
    
    protected override void OnExit(ExitEventArgs e) {
        Log.Information("Aplicacion cerrandose");
        Log.CloseAndFlush();
        
        if (Services is IDisposable disposable) disposable.Dispose();
        
        base.OnExit(e);
    }
}
