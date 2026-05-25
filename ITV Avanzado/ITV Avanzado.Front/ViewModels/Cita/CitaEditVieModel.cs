using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionItv.Models;
using ITV_Avanzado.Front.Mapper;
using ITV_Avanzado.Front.ViewModels.Form;
using ITV_Avanzado.Service.Citas;
using ITV_Avanzado.Service.Dialogs;
using Serilog;

namespace ITV_Avanzado.Front.ViewModels.Cita;

public partial class CitaEditVieModel(Vehiculo cita,
    ICitasService citasService,
    IDialogService dialogService,
    bool isNew) : ObservableObject {
    private readonly IDialogService _dialogService = dialogService;
    private readonly bool _isNew = isNew;
    private readonly ILogger _logger = Log.ForContext<CitaEditVieModel>();
    private readonly ICitasService _citasService = citasService;

    [ObservableProperty] private CitaFormData _formData = cita.ToFromData();
    [ObservableProperty] private string _windowTitle = isNew ? "Nueva Cita" : "Editar Cita";

    public IEnumerable<Motor> Motors => Enum.GetValues<Motor>();
    public Action<bool>? CloseAction { get; set; }

    [RelayCommand]
    private void Save() {
        if (!FormData.IsValid()) {
            _dialogService.ShowWarning(
                $"Se han detectado los siguientes errores de validación:\n\n{FormData.GetValidationErrors()}",
                "Errores de validación");
            return;
        }

        var hoy = DateTime.Now.Date;
        var fecha = FormData.FechaInspeccion.Date;
        if (fecha < hoy || fecha > hoy.AddDays(30)) {
            _dialogService.ShowWarning(
                "La fecha de inspección debe estar entre la fecha actual y 30 días como máximo.",
                "Errores de validación");
            return;
        }

        if (FormData.FechaMatriculacion.Date > DateTime.Now.Date) {
            _dialogService.ShowWarning(
                "La fecha de matriculación no puede ser futura.",
                "Errores de validación");
            return;
        }

        try {
            var modelo = FormData.ToModel();

            if (!_isNew) {
                modelo = modelo with {
                    Id = cita.Id,
                    CreatedAt = cita.CreatedAt,
                    IsDeleted = cita.IsDeleted,
                };
            }

            var result = _isNew
                ? _citasService.Save(modelo)
                : _citasService.Update(modelo.Id, modelo);

            if (result.IsSuccess) {
                _logger.Information("Cita para matrícula {Matricula} guardada correctamente", modelo.Matricula);
                CloseAction?.Invoke(true);
            }
            else {
                _logger.Warning("Error de negocio al guardar: {Error}", result.Error.Message);
                _dialogService.ShowError(result.Error.Message);
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error crítico al guardar la cita");
            _dialogService.ShowError("Se ha producido un error inesperado al guardar la cita.");
        }
    }

    [RelayCommand]
    private void Cancel() {
        _logger.Debug("Operación de edición cancelada por el usuario");
        CloseAction?.Invoke(false);
    }
}