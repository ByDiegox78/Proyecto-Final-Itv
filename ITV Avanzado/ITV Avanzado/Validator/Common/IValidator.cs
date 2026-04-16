using CSharpFunctionalExtensions;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Validator.Common;

public interface IValidator<T> {
    Result<T, DomainError> Validar(T entity);
}