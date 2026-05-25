using CommunityToolkit.Mvvm.ComponentModel;
using GestionItv.Models;
using ITV_Avanzado.Service.Citas;
using ITV_Avanzado.Service.Report;
using Serilog;

namespace ITV_Avanzado.Front.ViewModels.DashBoard;

public partial class DashBoardViewModel : ObservableObject {
    private readonly ILogger _logger = Log.ForContext<DashBoardViewModel>();
    private readonly ICitasService _citasService;
    private readonly IReportService _reportService;

    [ObservableProperty]
    private Informe? informe;
    
    public int TotalCitas => Informe?.TotalCitas ?? 0;
    public int Gasolina => Informe?.Gasolina ?? 0;
    public int Diesel => Informe?.Diesel ?? 0;
    public int Hibrido => Informe?.Hibrido ?? 0;
    public int Electrico => Informe?.Electrico ?? 0;
    public int CitasParaHoy => Informe?.CitasParaHoy ?? 0;

    public DashBoardViewModel(ICitasService citasService, IReportService reportService) {
        _citasService = citasService;
        _reportService = reportService;
        LoadStats();
    }

    private void LoadStats() {
        try {
            _logger.Information("Cargando estadísticas del dashboard");
            var totalCitas = _citasService.GetAll(1, 1000, false);
            foreach (var v in totalCitas) {
                _logger.Information("Vehículo: {Matricula}, FechaInspeccion: {Fecha}, EsHoy: {EsHoy}",
                    v.Matricula, v.FechaInspeccion, v.FechaInspeccion.Date == DateTime.Now.Date);
            }
            Informe = _reportService.GenerarInformeCita(totalCitas);
            _logger.Information("Informe generado: Total={Total}", Informe?.TotalCitas);

            OnPropertyChanged(nameof(TotalCitas));
            OnPropertyChanged(nameof(Gasolina));
            OnPropertyChanged(nameof(Diesel));
            OnPropertyChanged(nameof(Hibrido));
            OnPropertyChanged(nameof(Electrico));
            OnPropertyChanged(nameof(CitasParaHoy));
        }
        catch (Exception e) {
            _logger.Error(e, "Error al cargar estadísticas");
        }
    }
    partial void OnInformeChanged(Informe? value) {
        OnPropertyChanged(nameof(TotalCitas));
        OnPropertyChanged(nameof(Gasolina));
        OnPropertyChanged(nameof(Diesel));
        OnPropertyChanged(nameof(Hibrido));
        OnPropertyChanged(nameof(Electrico));
        OnPropertyChanged(nameof(CitasParaHoy));
    }
}