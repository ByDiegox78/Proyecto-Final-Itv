using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Error.Buckup;
/// <summary>
///     Contenedor de errores específicos del buckup
/// </summary>
public abstract record BuckupError(string Message) : DomainError(Message) {
    public abstract record BackupError(string Message) : DomainError(Message) {
        public sealed record FileNotFound(string FilePath)
            : BackupError($"No se encontro el archivo de backup: {FilePath}");

        public sealed record InvalidBackupFile(string Details)
            : BackupError($"El archivo de backup es inválido o está corrupto: {Details}");

        public sealed record CreationError(string Details)
            : BackupError($"Error al crear el backup: {Details}");

        public sealed record RestorationError(string Details)
            : BackupError($"Error al restaurar el backup: {Details}");

        public sealed record DirectoryError(string Details)
            : BackupError($"Error con el directorio de backup: {Details}");
    }
}
/// <summary>
///     Factory de errores
/// </summary>
public static class BuckupErrors {
        public static DomainError FileNotFound(string filePath) {
            return new BuckupError.BackupError.FileNotFound(filePath);
        }

        public static DomainError InvalidBackupFile(string details) {
            return new BuckupError.BackupError.InvalidBackupFile(details);
        }

        public static DomainError CreationError(string details) {
            return new BuckupError.BackupError.CreationError(details);
        }

        public static DomainError RestorationError(string details) {
            return new BuckupError.BackupError.RestorationError(details);
        }

        public static DomainError DirectoryError(string details) {
            return new BuckupError.BackupError.DirectoryError(details);
        }
    }
