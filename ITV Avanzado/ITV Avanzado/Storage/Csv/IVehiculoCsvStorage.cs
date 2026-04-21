using GestionItv.Models;
using ITV_Avanzado.Storage.Common;

namespace ITV_Avanzado.Storage.Csv;

/// <summary>
/// Hereda de <see cref="IStorage{T}" /> con T = <see cref="Vehiculo" />.
/// </summary>
public interface IVehiculoCsvStorage : IStorage<Vehiculo> {
    
}