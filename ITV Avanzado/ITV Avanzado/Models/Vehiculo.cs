namespace GestionItv.Models;

public record Vehiculo(
    int Id,
    string Matricula,
    string Marca,
    int Cilindrada,
    Motor TipoMotor,
    string DniPropietario,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
    );