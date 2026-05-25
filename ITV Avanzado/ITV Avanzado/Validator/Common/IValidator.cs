using CSharpFunctionalExtensions;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Validator.Common;
/// <summary>
///     Contrato para validar entidades del dominio.
/// </summary>
/// <typeparam name="T">Tipo de entidad a validar.</typeparam>
public interface IValidator<T> {
    /// <summary>
    ///     Valida una entidad según las reglas de dominio.
    /// </summary>
    /// <param name="entidad">Entidad a validar.</param>
    /// <returns>
    ///     Result con la entidad validada o error <see cref="Errors.Citas.CitaError.Validation(string)" /> si no es
    ///     válida.
    /// </returns>
    Result<T, DomainError> Validar(T entity);
}