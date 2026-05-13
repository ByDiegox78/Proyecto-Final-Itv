using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Service.Citas;

public interface ICitasService {

    IEnumerable<Vehiculo> GetAll(int page = 1, int pageSize = 10, bool includeDeleted = true, string? campoBusqueda = null);
    
    Result<Vehiculo, DomainError> GetById(int id);

    Result<IEnumerable<Vehiculo>, DomainError> GetByMatricula(string matricula);


    Result<Vehiculo, DomainError> Save(Vehiculo cita);

    Result<Vehiculo, DomainError> Update(int id, Vehiculo cita);

    Result<Vehiculo, DomainError> Delete(int id, bool isLogical = true);

    bool DeleteAll();

    Result<Vehiculo, DomainError> Restore(int id);
    
    IEnumerable<Vehiculo> GetCitasOrderBy(
        TipoOrdenamiento ordenamiento, 
        int page = 1, 
        int pageSize = 10, 
        bool includeDeleted = true);


}