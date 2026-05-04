using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Error.Vehiculos;

public record VehiculoError(string Message) : DomainError(Message) {
    public sealed record NotFound(string Id)
        : VehiculoError($"No se ha encontrado ningun vehiculo con el identificador: {Id}");
    public sealed record Validation(IEnumerable<string> Errors)
        : VehiculoError($"Se han detectado errores de validación en la entidad:{Environment.NewLine}• {string.Join($"{Environment.NewLine}• ", Errors)}");
    public sealed record MaxVehiculosUsageDniError(string Dni)
        : VehiculoError($"El propietario con dni: {Dni} tiene 3 vehiculos para inspeccion para el mismo dia");
    public sealed record MatriculaInspeccionDuplicada(string matricula)
        : VehiculoError($"Conflicto de integridad: La matricula {matricula} tiene una inspeccion resgistrada para hoy");
    public sealed record DatabaseError(string Details)
        : VehiculoError($"Error de base de datos: {Details}");
    public sealed record StorageError(string Details)
        : VehiculoError($"Error de almacenamiento: {Details}");
    
}
public static class VehiculoErrors {
    public static DomainError NotFound(string id) {
        return new VehiculoError.NotFound(id);
    }
    public static DomainError Validation(IEnumerable<string> errors) {
        return new VehiculoError.Validation(errors);
    }
    public static DomainError MaxVehiculosUsageDniError(string dni) {
        return new VehiculoError.MaxVehiculosUsageDniError(dni);
    }
    public static DomainError MatriculaInspeccionDuplicada(string matricula) {
        return new VehiculoError.MatriculaInspeccionDuplicada(matricula);
    }
    public static DomainError DatabaseError(string details) {
        return new VehiculoError.DatabaseError(details);
    }
    public static DomainError StorageError(string details) {
        return new VehiculoError.StorageError(details);
    }
    
}