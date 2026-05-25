using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Service.Citas;

public interface ICitasService {
    /// <summary>
    ///     Obtiene todas las citas con paginación y filtro opcional.
    /// </summary>
    IEnumerable<Cita> GetAll(int page = 1, int pageSize = 10, bool includeDeleted = true, string? campoBusqueda = null);
    /// <summary>
    ///     Obtiene una cita por su ID.
    /// </summary>
    Result<Cita, DomainError> GetById(int id);
    /// <summary>
    ///     Obtiene citas por matrícula.
    /// </summary>
    Result<IEnumerable<Cita>, DomainError> GetByMatricula(string matricula);
    /// <summary>
    ///     Guarda una nueva cita.
    /// </summary>
    Result<Cita, DomainError> Save(Cita cita);
    /// <summary>
    ///     Actualiza una cita existente identificada por su ID.
    /// </summary>
    Result<Cita, DomainError> Update(int id, Cita cita);
    /// <summary>
    ///     Elimina una cita (física o lógicamente).
    /// </summary>
    /// <param name="isLogical">True para borrado lógico, False para borrado físico.</param>
    Result<Cita, DomainError> Delete(int id, bool isLogical = true);
    /// <summary>
    ///     Elimina todas las citas del sistema.
    /// </summary>
    bool DeleteAll();
    /// <summary>
    ///     Restaura una cita eliminada lógicamente.
    /// </summary>
    Result<Cita, DomainError> Restore(int id);
    /// <summary>
    ///     Obtiene citas ordenadas por el campo especificado.
    /// </summary>
    /// <param name="ordenamiento">Campo por el que ordenar.</param>
    IEnumerable<Cita> GetCitasOrderBy(
        TipoOrdenamiento ordenamiento, 
        int page = 1, 
        int pageSize = 10, 
        bool includeDeleted = true);
}