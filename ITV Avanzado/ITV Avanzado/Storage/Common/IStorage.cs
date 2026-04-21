using CSharpFunctionalExtensions;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Storage.Common;

public interface IStorage<T> {
    /// <summary>
    ///     Salva una colección de elementos en un archivo.
    /// </summary>
    /// <param name="items">Colección de los ciudadanos que guardemos.</param>
    /// <param name="path">Ruta del archivo donde se guardarán los datos.</param>
    /// <returns>
    ///     Result con true si se guardó correctamente o error:
    ///     <see cref="Errors.Storage.StorageErrors.FileNotFound(string)" />,
    ///     <see cref="Errors.Storage.StorageErrors.InvalidFormat(string)" /> o
    ///     <see cref="Errors.Storage.StorageErrors.WriteError(string)" />.
    /// </returns>
    Result<bool, DomainError> Salvar(IEnumerable<T> items, string path);
    /// <summary>
    ///     Carga una colección de elementos desde un archivo.
    /// </summary>
    /// <param name="path">Ruta del archivo desde donde se cargarán los datos.</param>
    /// <returns>Colección de elementos cargados desde el archivo.</returns>
    Result<IEnumerable<T>, DomainError> Cargar(string path);
}