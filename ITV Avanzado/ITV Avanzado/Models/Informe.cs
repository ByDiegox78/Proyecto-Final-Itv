namespace GestionItv.Models;

public record Informe() {
    public IEnumerable<Cita>? ListadoCitas { get; init; }
    
    public int TotalCitas { get; init; }

    public int Gasolina { get; init; }
    
    public int Diesel { get; init; }
    
    public int Hibrido { get; init; }
    
    public int Electrico { get; init; }
    
    public int CitasParaHoy { get; init; }
    
    
}