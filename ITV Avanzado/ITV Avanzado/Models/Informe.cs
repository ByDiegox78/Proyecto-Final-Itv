namespace GestionItv.Models;

public record Informe() {
    public int Id { get; init; }
    public string Matricula { get; init; } = string.Empty;
    public string Marca { get; init; } = string.Empty;
    
    // Datos Técnicos
    public string DatosMotor { get; init; } = string.Empty; 
    public string PropietarioDni { get; init; } = string.Empty;
}