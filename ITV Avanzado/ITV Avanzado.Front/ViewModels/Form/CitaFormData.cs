using System.ComponentModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using GestionItv.Models;

namespace ITV_Avanzado.Front.ViewModels.Form;

public partial class CitaFormData : ObservableObject, IDataErrorInfo {
       [ObservableProperty] private string _matricula = string.Empty;

    [ObservableProperty] private string _marca = string.Empty;


    [ObservableProperty] private int _cilindrada;

    [ObservableProperty] private int id;

    [ObservableProperty] private Motor _motor;
    
    [ObservableProperty] private string _dniPropietario = string.Empty;

    [ObservableProperty] private DateTime _fechaMatriculacion;

    
    [ObservableProperty] private DateTime _fechaInspeccion;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; }

    
    public string Error { get; }
    
    public string this[string columnName] => columnName switch {
        nameof(Matricula) when !RegexMatricula.IsMatch(Matricula)
            => "La matrícula debe tener formato NNNNLLL (4 números + 3 consonantes)",
        nameof(Marca) when string.IsNullOrWhiteSpace(Marca) || Marca.Length < 2
            => "La marca debe tener al menos 2 caracteres",
        nameof(Cilindrada) when Cilindrada < 0 || Cilindrada > 3000
            => "La cilindrada debe estar entre 0 y 3000",
        nameof(FechaInspeccion) when FechaInspeccion < DateTime.Today || FechaInspeccion > DateTime.Today.AddDays(30)
            => "La inspección debe estar entre hoy y los próximos 30 días",
        nameof(FechaMatriculacion) when FechaMatriculacion > DateTime.Today
            => "La fecha de matriculación no puede ser futura",
        nameof(DniPropietario) when !ValidarDni(DniPropietario)
            => "El DNI no tiene un formato válido",
        _ => null!
    };
    
    public bool IsValid() {
        return string.IsNullOrEmpty(this[nameof(Matricula)]) &&
               string.IsNullOrEmpty(this[nameof(Marca)]) &&
               string.IsNullOrEmpty(this[nameof(Cilindrada)]) &&
               string.IsNullOrEmpty(this[nameof(FechaInspeccion)]) &&
               string.IsNullOrEmpty(this[nameof(FechaMatriculacion)]) &&
               string.IsNullOrEmpty(this[nameof(DniPropietario)]);
    }
    public string GetValidationErrors() {
        var campos = new[] {
            (nameof(Matricula), "Matricula"),
            (nameof(Marca), "Marca"),
            (nameof(Cilindrada), "Cilindrada"),
            (nameof(FechaInspeccion), "Fecha de Inspección"),
            (nameof(FechaMatriculacion), "Fecha de Matriculación"),
            (nameof(DniPropietario), "DNI del Propietario")

        };

        var errores = campos
            .Select(c => (Campo: c.Item2, Error: this[c.Item1]))
            .Where(c => !string.IsNullOrWhiteSpace(c.Error))
            .Select(c => $"• {c.Campo}: {c.Error}");

        return string.Join("\n", errores);
    }
    private static readonly Regex RegexMatricula =
        new(@"^[0-9]{4}[BCDFGHJKLMNPRSTVWXYZ]{3}$");
    private bool ValidarDni(string dni) {
        string letrasDniPermitidas = "TRWAGMYFPDXBNJZSQVHLCKE";
        if (string.IsNullOrWhiteSpace(dni) || !RegexDni.IsMatch(dni))
            return false;
        int numero = int.Parse(dni.Substring(0, 8));
        char letraCorrecta = letrasDniPermitidas[numero % 23];
        return dni[8] == letraCorrecta;
    }
    private static readonly Regex RegexDni =
        new(@"^[0-9]{8}[TRWAGMYFPDXBNJZSQVHLCKE]$");
}