    using System.Text.RegularExpressions;
    using CSharpFunctionalExtensions;
    using GestionItv.Models;
    using ITV_Avanzado.Error.Common;
    using ITV_Avanzado.Error.Vehiculos;
    using ITV_Avanzado.Validator.Common;
    using static System.Console;

    namespace ITV_Avanzado.Validator.Vehiculos;

    public class ValidadorVehiculo : IValidator<Vehiculo> {
        public Result<Vehiculo, DomainError> Validar(Vehiculo entidad) {
            var errores = new List<string>();
            
            if (!RegexMatricula.IsMatch(entidad.Matricula)) {
                errores.Add("La matricula no cumple la regla de NNNNLLL");
            }
            if (string.IsNullOrWhiteSpace(entidad.Marca) || entidad.Marca.Length < 2) {
                errores.Add("La marca debe contener al manos 2 carazteres");
                
            }
            bool estaEnRangoValido = entidad.Cilindrada >= 800 && entidad.Cilindrada <= 3000;
            if (entidad.TipoMotor == Motor.Electrico) {
                if (entidad.Cilindrada != 0)
                    errores.Add("La cilindrada debe ser 0 en vehículos eléctricos");
            } else {
                if (entidad.Cilindrada < 800 || entidad.Cilindrada > 3000)
                    errores.Add("La cilindrada debe estar entre 800 y 3000 (excepto vehículos eléctricos)");
            }

            if (!Enum.IsDefined(entidad.TipoMotor)) {
                errores.Add("El tipo de motor no coincide con los disponibles");
            }

            if (!ValidarDni(entidad.DniPropietario)) {
                errores.Add("El dni del dueño no tiene el formato correcto");
            }

            if (entidad.FechaMatriculacion > DateTime.UtcNow.Date) {
                errores.Add("La fecha de matriculación no puede ser futura a la fecha actual");
            }

            if (entidad.FechaInspeccion < DateTime.UtcNow.Date || entidad.FechaInspeccion.Date > DateTime.UtcNow.AddDays(30)) {
                errores.Add("La fecha de inspección debe estar entre hoy y los próximos 30 días");
            }

            if (errores.Any())
                return Result.Failure<Vehiculo, DomainError>(VehiculoErrors.Validation(errores));
            return Result.Success<Vehiculo, DomainError>(entidad);
        }
        private bool ValidarDni(string dni) {
            string letrasDniPermitidas = "TRWAGMYFPDXBNJZSQVHLCKE";
            if (string.IsNullOrWhiteSpace(dni) || !RegexDni.IsMatch(dni))
                return false;
            int numero = int.Parse(dni.Substring(0, 8));
            char letraCorrecta = letrasDniPermitidas[numero % 23];
            return dni[8] == letraCorrecta;
        }
        private static readonly Regex RegexMatricula =
            new(@"^[0-9]{4}[BCDFGHJKLMNPRSTVWXYZ]{3}$");
        
        private static readonly Regex RegexDni =
            new(@"^[0-9]{8}[TRWAGMYFPDXBNJZSQVHLCKE]$");
    }