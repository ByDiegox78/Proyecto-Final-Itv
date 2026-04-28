namespace GestionItv.Models;

public record Vehiculo {
    public int Id { get; init; }
    public string Matricula { get; init; } = string.Empty;
    public string Marca { get; init; } = string.Empty;
    public int Cilindrada { get; init; }
    public Motor TipoMotor { get; init; }
    public string DniPropietario { get; init; } = string.Empty;
    
    public DateTime FechaMatriculacion { get; init; }
    
    public DateTime FechaInspeccion { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}