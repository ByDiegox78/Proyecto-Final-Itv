using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Error.Vehiculos;

public record VehiculoError(string Message) : DomainError(Message) {
    public sealed record NotFound(string Id)
        : VehiculoError($"No se ha encontrado ningun vehiculo con el identificador: {Id}");
    public sealed record Validation(IEnumerable<string> Errors)
        : VehiculoError($"Se han detectado errores de validación en la entidad:{Environment.NewLine}• {string.Join($"{Environment.NewLine}• ", Errors)}");
    public sealed record DniAlreadyExists(string Dni)
        : VehiculoError($"Conflicto de integridad: El DNI {Dni} ya está registrado en el sistema.");
    public sealed record MatriculaAlreadyExists(string matricula)
        : VehiculoError($"Conflicto de integridad: La matricula {matricula} ya está registrado en el sistema.");
    public sealed record DatabaseError(string Details)
        : VehiculoError($"Error de base de datos: {Details}");
    public sealed record StorageError(string Details)
        : VehiculoError($"Error de almacenamiento: {Details}");

    public sealed record MaxVehiculosUsageDniError(string Details)
        : VehiculoError($"Error, cantidad de vehiculos superada: {Details}");
}
public static class VehiculoErrors {
    public static DomainError NotFound(string id) {
        return new VehiculoError.NotFound(id);
    }
    public static DomainError Validation(IEnumerable<string> errors) {
        return new VehiculoError.Validation(errors);
    }
    public static DomainError DniAlreadyExists(string dni) {
        return new VehiculoError.DniAlreadyExists(dni);
    }
    public static DomainError MatriculaAlreadyExists(string matricula) {
        return new VehiculoError.MatriculaAlreadyExists(matricula);
    }
    public static DomainError DatabaseError(string details) {
        return new VehiculoError.DatabaseError(details);
    }
    public static DomainError StorageError(string details) {
        return new VehiculoError.StorageError(details);
    }
    public static DomainError MaxVehiculosUsageDniError(string details) {
        return new VehiculoError.MaxVehiculosUsageDniError(details);
    }
    
}