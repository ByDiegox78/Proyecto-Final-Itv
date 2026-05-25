using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionItv.Models;
using ITV_Avanzado.Front.Cita;
using ITV_Avanzado.Front.Mapper;
using ITV_Avanzado.Front.View.Cita;
using ITV_Avanzado.Service.Citas;
using ITV_Avanzado.Service.Dialogs;
using Serilog;

namespace ITV_Avanzado.Front.ViewModels.Cita;

public partial class CitaViewModel : ObservableObject {
    private readonly ICitasService _citasService;
    private readonly IDialogService _dialogService;
    private readonly ILogger _logger = Log.ForContext<CitaViewModel>();
    private List<Vehiculo> _todosLosVehiculos = [];

    [ObservableProperty] private string _motorSeleccionado = "TODOS";
    [ObservableProperty] private ObservableCollection<CitaItemViewModel> _citas = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _mostrarEliminados;
    [ObservableProperty] private TipoOrdenamiento _ordenActual = TipoOrdenamiento.Matricula;
    [ObservableProperty] private int _paginaActual = 1;
    [ObservableProperty] private string _textoBusqueda = "";
    [ObservableProperty] private CitaItemViewModel? _citaSeleccionada;
    [ObservableProperty] private string _estadoMensaje = "";
    [ObservableProperty] private int _tamanoPagina = 10;
    [ObservableProperty] private int _totalPaginas;
    [ObservableProperty] private int _totalRegistros;
    [ObservableProperty] private DateTime _fechaInicio = DateTime.Today.AddMonths(-1);
    [ObservableProperty] private DateTime? _fechaFin = DateTime.Today.AddMonths(1);

    public List<string> Motores => new List<string> { "TODOS" }.Concat(
        Enum.GetValues<Motor>().Select(m => m.ToString())).ToList();

    public int[] TamanosPagina => [5, 10, 25, 50];
    public bool PuedeIrPaginaAnterior => PaginaActual > 1;
    public bool PuedeIrPaginaSiguiente => PaginaActual < TotalPaginas;

    public CitaViewModel(ICitasService citasService, IDialogService dialogService) {
        _citasService = citasService;
        _dialogService = dialogService;
        LoadCitas();
    }

    partial void OnTextoBusquedaChanged(string value) { PaginaActual = 1; LoadCitas(); }
    partial void OnMotorSeleccionadoChanged(string value) { PaginaActual = 1; LoadCitas(); }
    partial void OnMostrarEliminadosChanged(bool value) { PaginaActual = 1; LoadCitas(); }
    partial void OnFechaInicioChanged(DateTime value) => LoadCitas();
    partial void OnFechaFinChanged(DateTime? value) => LoadCitas();

    partial void OnPaginaActualChanged(int value) {
        LoadCitas();
        PaginaSiguienteCommand.NotifyCanExecuteChanged();
        PaginaAnteriorCommand.NotifyCanExecuteChanged();
    }
    partial void OnTamanoPaginaChanged(int value) {
        PaginaActual = 1;
        LoadCitas();
        PaginaSiguienteCommand.NotifyCanExecuteChanged();
        PaginaAnteriorCommand.NotifyCanExecuteChanged();
    }

    partial void OnCitaSeleccionadaChanged(CitaItemViewModel? value) {
        EditarCitaCommand.NotifyCanExecuteChanged();
        EliminarCitaCommand.NotifyCanExecuteChanged();
        VerCitaCommand.NotifyCanExecuteChanged();
        RestaurarCitaCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void LoadCitas() {
        IsLoading = true;
        EstadoMensaje = "Cargando citas...";

        try {
            var result = _citasService.GetAll(1, int.MaxValue, MostrarEliminados);
            _todosLosVehiculos = result.ToList();
            FiltrarCitas();
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al cargar citas");
            EstadoMensaje = "Error al cargar";
        }
        finally {
            IsLoading = false;
        }
    }

    private void FiltrarCitas() {
        var filtered = _todosLosVehiculos.AsEnumerable();

        if (!MostrarEliminados)
            filtered = filtered.Where(v => !v.IsDeleted);

        if (MotorSeleccionado != "TODOS" && Enum.TryParse<Motor>(MotorSeleccionado, out var motor))
            filtered = filtered.Where(v => v.TipoMotor == motor);

        if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            filtered = filtered.Where(v =>
                v.Matricula.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                v.Marca.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                v.DniPropietario.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase));

        if (FechaInicio != default)
            filtered = filtered.Where(v => v.FechaInspeccion >= FechaInicio);

        if (FechaFin.HasValue)
            filtered = filtered.Where(v => v.FechaInspeccion <= FechaFin.Value);

        var listaFiltrada = AplicarOrdenamiento(filtered).ToList();

        TotalRegistros = listaFiltrada.Count;
        TotalPaginas = TotalRegistros == 0 ? 1 : (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);

        if (PaginaActual > TotalPaginas) PaginaActual = TotalPaginas;
        if (PaginaActual < 1) PaginaActual = 1;

        var pagina = listaFiltrada
            .Skip((PaginaActual - 1) * TamanoPagina)
            .Take(TamanoPagina)
            .Select(v => v.ToItemViewModel())
            .ToList();

        Citas = new ObservableCollection<CitaItemViewModel>(pagina);

        EstadoMensaje = (MotorSeleccionado != "TODOS", !string.IsNullOrWhiteSpace(TextoBusqueda)) switch {
            (true, true) => $"Página {PaginaActual}/{TotalPaginas} - Mostrando {Citas.Count} de {TotalRegistros} citas",
            (true, false) => $"Página {PaginaActual}/{TotalPaginas} - {Citas.Count} de {TotalRegistros} citas de {MotorSeleccionado}",
            (false, true) => $"Página {PaginaActual}/{TotalPaginas} - Mostrando {Citas.Count} de {TotalRegistros} citas",
            _ => $"Página {PaginaActual}/{TotalPaginas} - Total: {TotalRegistros} vehículos"
        };
    }

    private IEnumerable<Vehiculo> AplicarOrdenamiento(IEnumerable<Vehiculo> lista) {
        return OrdenActual switch {
            TipoOrdenamiento.Matricula => lista.OrderBy(v => v.Matricula),
            TipoOrdenamiento.Dni => lista.OrderBy(v => v.DniPropietario),
            TipoOrdenamiento.Marca => lista.OrderBy(v => v.Marca),
            TipoOrdenamiento.Cilindrada => lista.OrderBy(v => v.Cilindrada),
            TipoOrdenamiento.FechaItv => lista.OrderBy(v => v.FechaInspeccion),
            _ => lista.OrderBy(v => v.Id)
        };
    }

    [RelayCommand(CanExecute = nameof(CanView))]
    private void VerCita() {
        if (CitaSeleccionada == null) return;
        var vehiculo = _todosLosVehiculos.FirstOrDefault(v => v.Id == CitaSeleccionada.Id);
        if (vehiculo == null) return;

        var detailsWindow = new CitaDetails {
            DataContext = vehiculo,
            Owner = Application.Current.MainWindow
        };
        detailsWindow.ShowDialog();
    }

    private bool CanView() => CitaSeleccionada != null;

    [RelayCommand]
    private void NuevaCita() {
        var nuevaCita = new Vehiculo {
            Matricula = "",
            Marca = "",
            Cilindrada = 0,
            TipoMotor = Motor.Diesel,
            DniPropietario = "",
            FechaMatriculacion = DateTime.Today,
            FechaInspeccion = DateTime.Today.AddDays(1)
        };

        var editVm = new CitaEditVieModel(nuevaCita, _citasService, _dialogService, true);
        var editWindow = new CitaEdit {
            DataContext = editVm,
            Owner = Application.Current.MainWindow
        };

        if (editWindow.ShowDialog() == true) {
            LoadCitas();
            EstadoMensaje = "Cita creada";
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void EditarCita() {
        if (CitaSeleccionada == null) return;

        var vehiculo = _todosLosVehiculos.FirstOrDefault(v => v.Id == CitaSeleccionada.Id);
        if (vehiculo == null) return;

        var editVm = new CitaEditVieModel(vehiculo, _citasService, _dialogService, false);
        var editWindow = new CitaEdit {
            DataContext = editVm,
            Owner = Application.Current.MainWindow
        };

        if (editWindow.ShowDialog() == true) {
            LoadCitas();
            EstadoMensaje = "Cita actualizada";
        }
    }

    private bool CanEdit() => CitaSeleccionada != null;

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void EliminarCita() {
        if (CitaSeleccionada == null) return;

        if (!_dialogService.ShowConfirmation("¿Está seguro de que desea eliminar este registro?", "Confirmar Acción"))
            return;

        var result = _citasService.Delete(CitaSeleccionada.Id, true);
        if (result.IsSuccess) {
            LoadCitas();
            EstadoMensaje = "Cita eliminada";
        }
        else {
            _dialogService.ShowError(result.Error.Message);
        }
    }

    private bool CanDelete() => CitaSeleccionada != null;

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void RestaurarCita() {
        if (CitaSeleccionada == null) return;

        if (!_dialogService.ShowConfirmation(
                $"¿Restaurar {CitaSeleccionada.Matricula}?", "Confirmar Restauración"))
            return;

        var result = _citasService.Restore(CitaSeleccionada.Id);
        if (result.IsSuccess) {
            LoadCitas();
            EstadoMensaje = "Cita restaurada";
        }
        else {
            _dialogService.ShowError($"Error al restaurar: {result.Error.Message}");
        }
    }

    [RelayCommand]
    private void Refrescar() {
        TextoBusqueda = "";
        MotorSeleccionado = "TODOS";
        FechaInicio = DateTime.Today.AddMonths(-1);
        FechaFin = DateTime.Today.AddMonths(1);
        MostrarEliminados = false;
        TamanoPagina = 10;
        OrdenActual = TipoOrdenamiento.Matricula;
        PaginaActual = 1;
    }

    [RelayCommand]
    private void OrdenarPor(TipoOrdenamiento orden) {
        OrdenActual = orden;
        LoadCitas();
    }

    [RelayCommand(CanExecute = nameof(PuedeIrPaginaAnterior))]
    private void PaginaAnterior() {
        if (PaginaActual > 1) PaginaActual--;
    }

    [RelayCommand(CanExecute = nameof(PuedeIrPaginaSiguiente))]
    private void PaginaSiguiente() {
        if (PaginaActual < TotalPaginas) PaginaActual++;
    }

    [RelayCommand]
    private void PrimeraPagina() => PaginaActual = 1;

    [RelayCommand]
    private void UltimaPagina() => PaginaActual = TotalPaginas;

    [RelayCommand]
    private void CambiarTamanoPagina(int tamano) => TamanoPagina = tamano;
}