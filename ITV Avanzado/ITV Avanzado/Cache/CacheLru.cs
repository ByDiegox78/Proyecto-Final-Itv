using Serilog;

namespace ITV_Avanzado.Cache;

public class CacheLru<TKey, TValue> : ICache<TKey, TValue>
    where TKey : notnull {
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue> _data = new();
    private readonly LinkedList<TKey> _usageOrder = new();
    private readonly ILogger _logger = Log.ForContext<CacheLru<TKey, TValue>>();

    public CacheLru(int capacity) {
        if (capacity <= 0)
            throw new ArgumentException("La capacidad no puede ser menor o igual que 0", nameof(capacity));
        _capacity = capacity;
    }
    /// <inheritdoc cref="ICache{TKey,TValue}.Add" />
    public void Add(TKey key, TValue value) {
        _logger.Debug("Añadiendo clave: {Key}", key);
        if (_data.TryGetValue(key, out var existingValue)) {
            _logger.Debug("La Clave {Key} ya existe. Actualizando el valor: {Old} a {New}", key, existingValue, value);
            _data[key] = value;
            RefreshUsage(key);
            return;
        }
        _logger.Debug("La Clave {Key} es nueva. La Capacidad Actual de la cache es de: {Used}/{Total}", 
            key, _data.Count, _capacity);
        if (_data.Count >= _capacity) {
            var oldestKey = _usageOrder.First!.Value;
            var oldestValue = _data[oldestKey];
            _logger.Debug("La cache esta llena. Sacando el elemento: {Key} = {Value}",
                oldestKey, oldestValue);
            _usageOrder.RemoveFirst();
            _data.Remove(oldestKey);
        }
        _data.Add(key, value);
        _usageOrder.AddLast(key);
        _logger.Debug("Se añadio el elemnto, La nueva cache es: {Order}",
            string.Join(" -> ", _usageOrder));    }

    /// <inheritdoc cref="ICache{TKey,TValue}.Get" />
    public TValue? Get(TKey key) {
        _logger.Debug("Buscando clave: {Key}", key);
        if (!_data.TryGetValue(key, out var value)) {
            _logger.Debug("Clave {Key} NO encontrada en cache", key);
            return default;
        }
        _logger.Debug("Clave {Key} encontrada: {Value}. Refrescando la Cache...",
            key, value);
        RefreshUsage(key);
        _logger.Debug("Nueva Cache despues de la actualizacion: {Order}",
            string.Join(" -> ", _usageOrder));
        return value;    
    }
    /// <inheritdoc cref="ICache{TKey,TValue}.Remove" />
    public bool Remove(TKey key) {
        _logger.Debug("Eliminando clave de la Cache: {Key}", key);
        if (!_data.Remove(key)) {
            _logger.Debug("Clave {Key} de la Cache no encontrada", key);
            return false;
        }
        _usageOrder.Remove(key);
        _logger.Debug("Clave {Key} de la Cache borrado correctamente", key);
        return true;
    }
    /// <summary>
    ///     Mueve una clave existente a la última posición de la lista de uso.
    ///     Este método es el corazón del algoritmo LRU.
    /// </summary>
    /// <param name="key">La clave del elemento que acaba de ser utilizado.</param>
    private void RefreshUsage(TKey key) {
        _logger.Verbose("[LRU-REFRESH] Moviendo clave {Key} al final de la lista", key);
        _usageOrder.Remove(key);
        _usageOrder.AddLast(key);
    }
}