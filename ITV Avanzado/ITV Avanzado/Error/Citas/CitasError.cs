using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Error.Citas;

public record CitaError(string Message) : DomainError(Message) {
    public sealed record NotFound(string Id)
        : CitaError($"No se ha encontrado ningun vehiculo con el identificador: {Id}");
    public sealed record Validation(IEnumerable<string> Errors)
        : CitaError($"Se han detectado errores de validación en la entidad:{Environment.NewLine}• {string.Join($"{Environment.NewLine}• ", Errors)}");
    public sealed record MaxCitasUsageDniError(string Dni)
        : CitaError($"El propietario con dni: {Dni} tiene 3 vehiculos para inspeccion para el mismo dia");
    public sealed record MatriculaInspeccionDuplicada(string matricula)
        : CitaError($"Conflicto de integridad: La matricula {matricula} tiene una inspeccion resgistrada para hoy");
    public sealed record DatabaseError(string Details)
        : CitaError($"Error de base de datos: {Details}");
    public sealed record StorageError(string Details)
        : CitaError($"Error de almacenamiento: {Details}");
    
}
public static class CitaErrors {
    public static DomainError NotFound(string id) {
        return new CitaError.NotFound(id);
    }
    public static DomainError Validation(IEnumerable<string> errors) {
        return new CitaError.Validation(errors);
    }
    public static DomainError MaxCitasUsageDniError(string dni) {
        return new CitaError.MaxCitasUsageDniError(dni);
    }
    public static DomainError MatriculaInspeccionDuplicada(string matricula) {
        return new CitaError.MatriculaInspeccionDuplicada(matricula);
    }
    public static DomainError DatabaseError(string details) {
        return new CitaError.DatabaseError(details);
    }
    public static DomainError StorageError(string details) {
        return new CitaError.StorageError(details);
    }
    
}