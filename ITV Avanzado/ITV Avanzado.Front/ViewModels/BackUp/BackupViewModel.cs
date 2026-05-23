using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV_Avanzado.Service.Buckup;
using ITV_Avanzado.Service.Citas;
using ITV_Avanzado.Service.Dialogs;
using Serilog;

namespace ITV_Avanzado.Front.ViewModels.BackUp;

public partial class BackupViewModel : ObservableObject {
    private readonly IBuckupService _backupService;
    private readonly IDialogService _dialogService;
    private readonly ILogger _logger = Log.ForContext<BackupViewModel>();
    private readonly ICitasService _citasService;

    [ObservableProperty] private ObservableCollection<string> _backups = new();
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _selectedBackup;
    [ObservableProperty] private string _statusMessage = "";

    public BackupViewModel(
        ICitasService citasService,
        IBuckupService backupService,
        IDialogService dialogService) {
        _backupService = backupService;
        _citasService = citasService;
        _dialogService = dialogService;
    }

    private void LoadBackup() {
        try {
            var backupList = _backupService.ListarBackups();
            Backups = new ObservableCollection<string>(backupList);
            StatusMessage = $"Encontrados {Backups.Count} backups";
        }
        catch (Exception e) {
           _logger.Error(e, "Error al cargas los backups");
           StatusMessage = "Error al cargar backups";
        }
    }

    [RelayCommand]
    private void RealizarBackup() {
        try {
            IsLoading = true;
            StatusMessage = "Realizando backups...";
            var citas = _citasService.GetAll();
            var result = _backupService.RealizarBackup(citas);

            if (result.IsSuccess) {
                LoadBackup();
                StatusMessage = $"Backup creado: {Path.GetFileName(result.Value)}";
                _dialogService.ShowSuccess($"Backup creado correctamente: \n{result.Value}");
            }
            else {
                _dialogService.ShowError(result.Error.Message);
                StatusMessage = "Error al crear el backup";
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al realizar backup");
            StatusMessage = "Error al crear backup";
        }
        finally {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private void RestaurarBackup() {
        if (string.IsNullOrEmpty(SelectedBackup)) {
            _dialogService.ShowWarning("Selecciona un bcakup para restaurar");
            return;
        }
        
        if (!_dialogService.ShowConfirmation(
                $"¿Restaurar el backup {Path.GetFileName(SelectedBackup)}?\n\n" +
                $"⚠️ Advertencia: Se borrarán todos los datos actuales (citas)\n" +
                $"y se reemplazará por el contenido de la copia de seguridad.\n\n" +
                $"Esta acción no se puede deshacer.",
                "Confimar restauración"))
            return;

        try {
            IsLoading = true;
            StatusMessage = "Restaurando backup...";

            var restoreResult = _backupService.RestaurarBackupSistema(
                SelectedBackup,
                () => _citasService.DeleteAll(),
                c => _citasService.Save(c));

            if (restoreResult.IsSuccess) {
                StatusMessage = $"Restaurados {restoreResult.Value} registros";
                _dialogService.ShowSuccess($"Backup restaurado correctamente\n{restoreResult.Value} registros");
            }
            else {
                _dialogService.ShowError(restoreResult.Error.Message);
                StatusMessage = "Error al restaurar";
            }
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al restaurar backup");
            StatusMessage = "Error al restaurar";
        }
        finally {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private void Refresh() {
        LoadBackup();
    }
    
    [RelayCommand]
    private void EliminarBackup() {
        if (string.IsNullOrEmpty(SelectedBackup)) return;

        if (!_dialogService.ShowConfirmation($"¿Eliminar el backup {Path.GetFileName(SelectedBackup)}"))
            return;

        try {
            File.Delete(SelectedBackup);
            LoadBackup();
            StatusMessage = "Backup eliminado";
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al eliminar backup");
            StatusMessage = "Error al eliminar";
        }
    }
    

}