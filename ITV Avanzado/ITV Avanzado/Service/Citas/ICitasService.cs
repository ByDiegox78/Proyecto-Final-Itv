using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Service.Citas;

public interface ICitasService {

    IEnumerable<Cita> GetAll(int page = 1, int pageSize = 10, bool includeDeleted = true, string? campoBusqueda = null);
    
    Result<Cita, DomainError> GetById(int id);

    Result<IEnumerable<Cita>, DomainError> GetByMatricula(string matricula);


    Result<Cita, DomainError> Save(Cita cita);

    Result<Cita, DomainError> Update(int id, Cita cita);

    Result<Cita, DomainError> Delete(int id, bool isLogical = true);

    bool DeleteAll();

    Result<Cita, DomainError> Restore(int id);
    
    IEnumerable<Cita> GetCitasOrderBy(
        TipoOrdenamiento ordenamiento, 
        int page = 1, 
        int pageSize = 10, 
        bool includeDeleted = true);


}