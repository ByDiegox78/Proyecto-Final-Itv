using GestionItv.Models;
using ITV_Avanzado.Storage.Common;

namespace ITV_Avanzado.Storage.Csv;

/// <summary>
/// Hereda de <see cref="IStorage{T}" /> con T = <see cref="Cita" />.
/// </summary>
public interface ICitasCsvStorage : IStorage<Cita> {
    
}