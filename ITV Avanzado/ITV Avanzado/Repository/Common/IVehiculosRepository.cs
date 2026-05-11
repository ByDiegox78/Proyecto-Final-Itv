using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Repository.Common;

public interface IVehiculosRepository {
    /// <summary>
    ///     Obtiene todos los vehiculos con una paginacion de 5 personas.
    /// </summary>
    IEnumerable<Vehiculo> GetAll(int page, int pageSize, bool includeDeleted, string campoBusqueda);
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
    Vehiculo? Delete(int id, bool isLogic);
    /// <summary>
    ///     Busca el vehiculo por su matricula
    /// </summary>
    /// <param name="matricula">Parametro de busqueda</param>
    /// <returns></returns>
    IEnumerable<Vehiculo>? GetByMatricula(string matricula);
    /// <summary>
    ///     Elimina todos los vehiculos
    /// </summary>
    /// <returns></returns>
    bool DeleteAll();
    
    Result<Vehiculo, DomainError> Restore(int id);



}