    using CSharpFunctionalExtensions;
    using GestionItv.Models;
    using ITV_Avanzado.Config;
    using ITV_Avanzado.Error.Common;
    using ITV_Avanzado.Error.Vehiculos;
    using ITV_Avanzado.Factory;
    using ITV_Avanzado.Repository.Common;
    using Serilog;

    namespace ITV_Avanzado.Repository.Memory;

    public class VehiculosRespositoryMemory : IVehiculosRepository {
        private readonly ILogger _logger = Log.ForContext<VehiculosRespositoryMemory>();
        private readonly Dictionary<string, int> _matricula = new();
        private readonly Dictionary<int, Vehiculo> _porId = new();
        private int _idCounter;

        
        public VehiculosRespositoryMemory( bool dropData, bool seedData) {
            if (dropData) {
                _logger.Warning("Borrando datos en memoria...");
                DeleteAll();
            }

            if (seedData) {
                _logger.Information("Cargando datos de semilla...");
                foreach (var persona in VehiculosFactory.Seed()) Create(persona);
                _logger.Information("SeedData completado.");
            }
        }
        public IEnumerable<Vehiculo> GetAll(int page, int pageSize, bool includeDeleted, string campoBusqueda) {
            _logger.Debug(
                "Obteniendo citas con paginación: página {Page}, tamaño {PageSize}, incluir borrados: {IncludeDeleted}",
                page, pageSize, includeDeleted);

            var consulta = _porId.Values.AsEnumerable();

            if (!includeDeleted) {
                consulta = _porId.Select(e => e.Value)
                    .Where(v => v.IsDeleted == false);
            }

            if (!string.IsNullOrWhiteSpace(campoBusqueda)) {
                consulta = consulta.Where(v => 
                    v.Matricula.Contains(campoBusqueda, StringComparison.OrdinalIgnoreCase) || 
                    v.Marca.Contains(campoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                    v.DniPropietario.Contains(campoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                    v.Cilindrada.ToString().Contains(campoBusqueda) ||
                    v.TipoMotor.ToString().Contains(campoBusqueda)
                );
            }
            return consulta
                .OrderBy(v => v.Id) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
        public Vehiculo? GetById(int id) {
            _logger.Debug("Buscando cita por id: {Id}", id);
            return _porId.GetValueOrDefault(id);

        }
        public Result<Vehiculo, DomainError> Create(Vehiculo vehiculo) {
            _logger.Debug("Creando un vehiculo {Entity}", vehiculo);
            if (!CupoVehiculosPorDia(vehiculo.DniPropietario, vehiculo.FechaInspeccion)) {
                _logger.Warning("El propietario con dni: {dni} tiene 3 vehiculos para inspeccion para el mismo dia", vehiculo.DniPropietario);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));
            }
            if (ExisteCitaDuplicada(vehiculo.Matricula,vehiculo.FechaInspeccion)) {
                _logger.Warning("La matricula {matriula} tiene una inspeccion resgistrada para hoy", vehiculo.Matricula);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaInspeccionDuplicada(vehiculo.Matricula));
            }
            var nuevo = vehiculo with {
                Id = ++_idCounter,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _porId[nuevo.Id] = nuevo;
            _matricula[nuevo.Matricula] = nuevo.Id;
            _logger.Information("Cita creada con ID {Id}", nuevo.Id);
            return Result.Success<Vehiculo, DomainError>(nuevo);
        }
        public Result<Vehiculo, DomainError> Update(int id, Vehiculo vehiculo) {
            _logger.Debug("Actualizando el vehiculo: {Entity}", vehiculo);
            if (!_porId.TryGetValue(id, out var actual)) {
                _logger.Warning("No se puede actualizar: vehiculo con id {Id} no encontrada", id);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
            }
            if (!CupoVehiculosPorDia(vehiculo.DniPropietario, vehiculo.FechaInspeccion, id)) {
                _logger.Warning("El propietario con dni: {dni} tiene 3 vehiculos para inspeccion para el mismo dia", vehiculo.DniPropietario);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MaxVehiculosUsageDniError(vehiculo.DniPropietario));
            }
            if (ExisteCitaDuplicada(vehiculo.Matricula,vehiculo.FechaInspeccion,id)) {
                _logger.Warning("La matricula {matriula} tiene una inspeccion resgistrada para hoy", vehiculo.Matricula);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.MatriculaInspeccionDuplicada(vehiculo.Matricula));
            }
            var actualizado = vehiculo with {
                Id = id,
                CreatedAt = actual.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _porId[id] = actualizado;
            if (actual.Matricula != actualizado.Matricula) {
                _matricula.Remove(actual.Matricula);
                _matricula[actualizado.Matricula] = id;
            }
            _logger.Information("Cita con ID {Id} actualizada correctamente", id);
            return Result.Success<Vehiculo, DomainError>(actualizado);
        }
        public Vehiculo? Delete(int id, bool isLogic = true) {
            _logger.Debug("Eliminando vehiculo con id {Id}", id);
            if (!_porId.TryGetValue(id, out var vehiculo)) {
                _logger.Warning("No se puede eliminar: vehiculo con id {Id} no encontrado", id);
                return null;
            }
            if (isLogic) {
                var eliminado =  vehiculo with {
                    IsDeleted = true,
                    UpdatedAt = DateTime.UtcNow
                };
                _porId[id] = eliminado;
                return eliminado;
            }
            _porId.Remove(id); 
            _matricula.Remove(vehiculo.Matricula);
            return vehiculo;
        }
        public IEnumerable<Vehiculo>? GetByMatricula(string matricula) {
            _logger.Debug("Buscando citas con matricula: {matricula}", matricula);
            return _porId.Values.Where(c => c.Matricula == matricula && !c.IsDeleted).ToList();
        } 
        public bool DeleteAll() {
            _logger.Warning("Eliminando permanentemente todos los vehiculos");
            _matricula.Clear();
            _porId.Clear();
            _idCounter = 0;
            return true;
        }
        public Result<Vehiculo, DomainError> Restore(int id) {
            if (!_porId.TryGetValue(id, out var actual)) {
                _logger.Warning("No se puede restaurar: vehiculo con id {Id} no encontrada", id);
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.NotFound(id.ToString()));
            }
            var restore = actual with {
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
            };
            _porId[id] = restore;
            _logger.Information("Cita con ID {Id} restaurada correctamente", id);
            return Result.Success<Vehiculo, DomainError>(restore);
        }
        private bool CupoVehiculosPorDia(string dni, DateTime fecha, int idActual = -1) {
            var count = _porId.Values.Count(v =>
                v.DniPropietario == dni &&
                v.FechaInspeccion.Date == fecha.Date &&
                v.Id != idActual &&
                !v.IsDeleted
            );
            return count < 3;
        }
       
        
        private bool ExisteCitaDuplicada(string matricula, DateTime fecha, int idActual = -1) {
            return _porId.Values.Any(v =>
                v.Matricula == matricula &&
                v.FechaInspeccion.Date == fecha.Date &&
                v.Id != idActual &&
                !v.IsDeleted
            );
        }
    }