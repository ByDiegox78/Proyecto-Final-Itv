using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Repository.Common;

public interface ICitaRepository {
    /// <summary>
    ///     Obtiene todos los vehiculos con una paginacion de 5 personas.
    /// </summary>
    IEnumerable<Cita> GetAll(int page, int pageSize, bool includeDeleted, string? campoBusqueda);
    /// <summary>
    ///     Obtiene un vehiculo por su ID.
    /// </summary>
    Cita? GetById(int id);
    /// <summary>
    ///     Crea una nuevo vehiculo en el sistema.
    /// </summary>
    /// <returns>Result con el vehiculo creado o error de dominio.</returns>
    Result<Cita, DomainError> Create(Cita cita);
    /// <summary>
    ///     Actualiza un vehiculo existente.
    /// </summary>
    /// <returns>Result con el vehiculo actualizado o error de dominio.</returns>
    Result<Cita, DomainError> Update(int id, Cita cita);

    /// <summary>
    ///     Elimina un vehiculo.
    /// </summary>
    Cita? Delete(int id, bool isLogic);
    /// <summary>
    ///     Busca el vehiculo por su matricula
    /// </summary>
    /// <param name="matricula">Parametro de busqueda</param>
    /// <returns></returns>
    IEnumerable<Cita>? GetByMatricula(string matricula, int page = 1, int pageSize = 10);
    /// <summary>
    ///     Elimina todos los vehiculos
    /// </summary>
    /// <returns></returns>
    bool DeleteAll();
    
    Result<Cita, DomainError> Restore(int id);



}