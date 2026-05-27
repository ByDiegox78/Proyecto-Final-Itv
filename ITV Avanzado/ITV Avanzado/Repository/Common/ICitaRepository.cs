using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Repository.Common;

public interface ICitaRepository {
    /// <summary>
    ///     Obtiene todas las citas con paginación y filtros opcionales.
    /// </summary>
    IEnumerable<Cita> GetAll(int page, int pageSize, bool includeDeleted, string? campoBusqueda);
    /// <summary>
    ///     Obtiene una cita por su ID.
    /// </summary>
    Cita? GetById(int id);
    /// <summary>
    ///     Crea una nueva cita en el sistema.
    /// </summary>
    /// <returns>Result con la cita creada o error de dominio.</returns>
    Result<Cita, DomainError> Create(Cita cita);
    /// <summary>
    ///     Actualiza una cita existente.
    /// </summary>
    /// <returns>Result con la cita actualizada o error de dominio.</returns>
    Result<Cita, DomainError> Update(int id, Cita cita);

    /// <summary>
    ///     Elimina una cita (física o lógicamente según <paramref name="isLogic"/>).
    /// </summary>
    Cita? Delete(int id, bool isLogic);
    /// <summary>
    ///     Busca citas por matrícula con paginación.
    /// </summary>
    /// <param name="matricula">Matrícula a buscar.</param>
    /// <param name="page">Número de página (1-based).</param>
    /// <param name="pageSize">Tamaño de página.</param>
    IEnumerable<Cita>? GetByMatricula(string matricula, int page = 1, int pageSize = 10);
    /// <summary>
    ///     Elimina todas las citas del sistema.
    /// </summary>
    bool DeleteAll();
    /// <summary>
    ///     Restaura una cita eliminada lógicamente.
    /// </summary>
    /// <returns>Result con la cita restaurada o error de dominio.</returns>
    Result<Cita, DomainError> Restore(int id);



}