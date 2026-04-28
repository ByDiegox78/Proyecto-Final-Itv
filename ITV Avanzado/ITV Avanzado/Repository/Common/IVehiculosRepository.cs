using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Repository.Common;

public interface IVehiculosRepository {
    /// <summary>
    ///     Obtiene todos los vehiculos con una paginacion de 5 personas.
    /// </summary>
    IEnumerable<Vehiculo> GetAll(int page = 1, int pageSize = 5, bool includeDeleted = true);
    /// <summary>
    ///     Obtiene un vehiculo por su ID.
    /// </summary>
    Vehiculo? GetById(int id);
    /// <summary>
    ///     Crea una nuevo vehiculo en el sistema.
    /// </summary>
    /// <returns>Result con el vehiculo creado o error de dominio.</returns>
    Result<Vehiculo, DomainError> Create(Vehiculo vehiculo);
    /// <summary>
    ///     Actualiza un vehiculo existente.
    /// </summary>
    /// <returns>Result con el vehiculo actualizado o error de dominio.</returns>
    Result<Vehiculo, DomainError> Update(int id, Vehiculo vehiculo);

    /// <summary>
    ///     Elimina un vehiculo.
    /// </summary>
    Vehiculo? Delete(int id);
    /// <summary>
    ///     Busca el vehiculo por su matricula
    /// </summary>
    /// <param name="matricula">Parametro de busqueda</param>
    /// <returns></returns>
    Vehiculo? GetByMatricula(string matricula);
    /// <summary>
    ///     Elimina todos los vehiculos
    /// </summary>
    /// <returns></returns>
    bool DeleteAll();
    /// <summary>
    /// Elimina el vehilo con borrado fisico
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Vehiculo? HardDelete(int id);

    Result<Vehiculo, DomainError> Restore(int id);



}