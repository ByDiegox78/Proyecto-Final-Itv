using CommunityToolkit.Mvvm.ComponentModel;
using GestionItv.Models;

namespace ITV_Avanzado.Front.Cita;

public partial class CitaItemViewModel : ObservableObject {
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _matricula = string.Empty;
    [ObservableProperty] private string _marca = string.Empty;
    [ObservableProperty] private int _cilindrada;
    [ObservableProperty] private Motor _tipoMotor;
    [ObservableProperty] private string _dniPropietario = string.Empty;
    [ObservableProperty] private DateTime _fechaMatriculacion;
    [ObservableProperty] private DateTime _fechaInspeccion;
    [ObservableProperty] private bool _isDeleted;

    public string Descripcion => $"{Matricula} {Marca} {TipoMotor} {DniPropietario}";
}