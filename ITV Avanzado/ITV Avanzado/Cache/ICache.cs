namespace ITV_Avanzado.Cache;


/// <summary>
/// Contrato para la cache LRU
/// </summary>
/// <typeparam name="TKey"> Tipo de clave </typeparam>
/// <typeparam name="TValue"> Tipo de valor </typeparam>
public interface ICache<in TKey, TValue> where TKey : notnull  {
    /// <summary>
    /// Agrega un elemento a la cache eliminando el mas usado si esta llena
    /// </summary>
    /// <param name="key">Clave del elemento</param>
    /// <param name="value">Valor del elemento</param>
    void Add(TKey key, TValue value);
    /// <summary>
    /// Obtiene un elemento de la cache
    /// </summary>
    /// <param name="key">Clave del elemento</param>
    /// <returns>El valor o null</returns>
    TValue? Get(TKey key);
    /// <summary>
    /// Elimina el elemento de la cache
    /// </summary>
    /// <param name="key">Clave del elemento</param>
    /// <returns>Devuelve true si se elimino y falso si no existia</returns>
    bool Remove(TKey key);
}