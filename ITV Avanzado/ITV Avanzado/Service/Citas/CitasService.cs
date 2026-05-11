// using CSharpFunctionalExtensions;
// using GestionItv.Models;
// using ITV_Avanzado.Cache;
// using ITV_Avanzado.Error.Common;
// using ITV_Avanzado.Repository.Common;
// using ITV_Avanzado.Validator.Common;
//
// namespace ITV_Avanzado.Service.Citas;
//
// public class CitasService(
//     IVehiculosRepository repository,
//     IValidator<Vehiculo> validator,
//     ICache<int, Vehiculo> cache
//     ) : ICitasService {
//     
//     
//     public IEnumerable<Vehiculo> GetAll(int page = 1, int pageSize = 10, bool includeDeleted = true) {
//         return repository.GetAll(page, pageSize, includeDeleted);
//     }
//
//     public Result<Vehiculo, DomainError> GetById(int id) {
//         
//     }
//
//     public Result<Vehiculo, DomainError> GetByMatricula(string matricula) {
//         throw new NotImplementedException();
//     }
//
//     public Result<Vehiculo, DomainError> GetByDniPropietario(string dni) {
//         throw new NotImplementedException();
//     }
//
//     public Result<Vehiculo, DomainError> Save(Vehiculo cita) {
//         throw new NotImplementedException();
//     }
//
//     public Result<Vehiculo, DomainError> Update(int id, Vehiculo cita) {
//         throw new NotImplementedException();
//     }
//
//     public Result<Vehiculo, DomainError> Delete(int id, bool isLogical = true) {
//         throw new NotImplementedException();
//     }
//
//     public bool DeleteAll() {
//         throw new NotImplementedException();
//     }
//
//     public Result<Vehiculo, DomainError> Restore(int id) {
//         throw new NotImplementedException();
//     }
// }