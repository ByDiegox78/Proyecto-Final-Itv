using FluentAssertions;
using GestionItv.Models;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Validator.Vehiculos;

namespace ITV_Test.Validator;

[TestFixture]
public class ValidadorVehiculoTest {
    [TestFixture]
    public class CasosPositivos {
        [SetUp]
        public void SetUp() {
            _validador = new ValidadorVehiculo();
        }
        private ValidadorVehiculo _validador = null!;
        [Test]
        public void Validar_VehiculoValido_Succes() {
            var vehiculo = new Vehiculo {
                Id = 1, Matricula = "4196FMR", Marca = "Fiat", Cilindrada = 900, TipoMotor = Motor.Diesel,
                DniPropietario = "54407737H", FechaMatriculacion = DateTime.UtcNow.AddDays(-100),
                FechaInspeccion = DateTime.UtcNow.AddDays(10)
            };
            var res = _validador.Validar(vehiculo);

            res.IsSuccess.Should().BeTrue();
        }
        [TestCase(Motor.Diesel)]
        [TestCase(Motor.Hibrido)]
        [TestCase(Motor.Gasolina)]
        public void Validate_MotoresCombustionValidos_Succes(Motor motor) {
            var vehiculo = new Vehiculo {
                Id = 1, Matricula = "4196FMR", Marca = "Fiat", Cilindrada = 900, TipoMotor = motor,
                DniPropietario = "54407737H", FechaMatriculacion = DateTime.UtcNow.AddDays(-100),
                FechaInspeccion = DateTime.UtcNow.AddDays(10)
            };
            var res = _validador.Validar(vehiculo);

                res.IsSuccess.Should().BeTrue();
                res.Value.TipoMotor.Should().Be(motor);
        }
        [TestCase(Motor.Electrico)]
        public void Validate_MotoreElectricoValido_Succes(Motor motor) {
            var vehiculo = new Vehiculo {
                Id = 1, Matricula = "4196FMR", Marca = "Fiat", Cilindrada = 0, TipoMotor = motor,
                DniPropietario = "54407737H", FechaMatriculacion = DateTime.UtcNow.AddDays(-10),
                FechaInspeccion = DateTime.UtcNow.AddDays(10)
            };
            var res = _validador.Validar(vehiculo);

            res.IsSuccess.Should().BeTrue();
            res.Value.TipoMotor.Should().Be(motor);
        }
    }

    [TestFixture]
    public class CasosInvalidos {
        [SetUp]
        public void SetUp() {
            _validador = new ValidadorVehiculo();
        }
        private ValidadorVehiculo _validador = null!;

        [TestCase("AAAAAAA")]
        [TestCase("123RTG")]
        [TestCase("FMR4196")]
        [TestCase("FM4196R")]
        public void Validar_MatriculaInvalida_Invalid(string matricula) {
            var vehiculo = new Vehiculo {
                Id = 1, Matricula = matricula, Marca = "Fiat", Cilindrada = 900, TipoMotor = Motor.Diesel,
                DniPropietario = "54407737H", FechaMatriculacion = DateTime.UtcNow.AddDays(-100),
                FechaInspeccion = DateTime.UtcNow.AddDays(10)
            };

            var res = _validador.Validar(vehiculo);

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<VehiculoError.Validation>();
            (res.Error as VehiculoError.Validation)?.Errors.Should()
                .Contain("La matricula no cumple la regla de NNNNLLL");
        }

        [TestCase("A")]
        [TestCase("")]
        public void Validar_MarcaInvalida_NoSucces(string marca) {
            var vehiculo = new Vehiculo {
                Id = 1, Matricula = "4196FMR", Marca = marca, Cilindrada = 900, TipoMotor = Motor.Diesel,
                DniPropietario = "54407737H", FechaMatriculacion = DateTime.UtcNow.AddDays(-100),
                FechaInspeccion = DateTime.UtcNow.AddDays(10)
            };
            var res = _validador.Validar(vehiculo);
            
            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<VehiculoError.Validation>();
            (res.Error as VehiculoError.Validation)?.Errors.Should()
                .Contain("La marca debe contener al manos 2 carazteres");
        }
        [TestCase(200)]
        [TestCase(3500)]
        [TestCase(1)]
        public void Validar_CilindradaElectricoInvalida_NoSucces(int cilindrada) {
            var vehiculo = new Vehiculo {
                Id = 1, Matricula = "4196FMR", Marca = "Fiat", Cilindrada = 900, TipoMotor = Motor.Electrico,
                DniPropietario = "54407737H", FechaMatriculacion = DateTime.UtcNow.AddDays(-100),
                FechaInspeccion = DateTime.UtcNow.AddDays(10)
            };
            var res = _validador.Validar(vehiculo);
            
            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<VehiculoError.Validation>();
            (res.Error as VehiculoError.Validation)?.Errors.Should()
                .Contain("La cilindrada debe ser 0 en vehículos eléctricos");
        }
        [TestCase(200)]
        [TestCase(3500)]
        [TestCase(0)]
        public void Validar_CilindradaCombustionInvalida_NoSucces(int cilindrada) {
            
            var vehiculo = new Vehiculo {
                Id = 1, Matricula = "4196FMR", Marca = "Fiat", Cilindrada = cilindrada, TipoMotor = Motor.Diesel,
                DniPropietario = "54407737H", FechaMatriculacion = DateTime.UtcNow.AddDays(-100),
                FechaInspeccion = DateTime.UtcNow.AddDays(10)
            };
            var res = _validador.Validar(vehiculo);
            
            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<VehiculoError.Validation>();
            (res.Error as VehiculoError.Validation)?.Errors.Should()
                .Contain("La cilindrada debe estar entre 800 y 3000 (excepto vehículos eléctricos)");
        }

        [Test]
        public void Validar_MotorInvalido_NoSucces() {
            var vehiculo = new Vehiculo {
                Id = 1, Matricula = "4196FMR", Marca = "Fiat", Cilindrada = 900, TipoMotor = (Motor)1000,
                DniPropietario = "54407737H", FechaMatriculacion = DateTime.UtcNow.AddDays(-100),
                FechaInspeccion = DateTime.UtcNow.AddDays(10)
            };
            var res = _validador.Validar(vehiculo);
            
            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<VehiculoError.Validation>();
            (res.Error as VehiculoError.Validation)?.Errors.Should()
                .Contain("El tipo de motor no coincide con los disponibles");
        }
        [TestCase("RRRRRRR")]
        [TestCase("R5673456")]
        [TestCase("234R789")]
        [TestCase("54407737z")]
        public void Validar_DniInvalida_NoSucces(string dni) {
            var vehiculo = new Vehiculo {
                Id = 1, Matricula = "4196FMR", Marca = "Fiat", Cilindrada = 900, TipoMotor = Motor.Diesel,
                DniPropietario = dni,
            };
            var res = _validador.Validar(vehiculo);
            
            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<VehiculoError.Validation>();
            (res.Error as VehiculoError.Validation)?.Errors.Should()
                .Contain("El dni del dueño no tiene el formato correcto");
        }
        [Test]
        public void Create_FechaMatriculacionFutura_Error() {
            var vehiculo = new Vehiculo {
                Matricula = "1234ABC",
                Marca = "Seat",
                Cilindrada = 1200,
                TipoMotor = Motor.Gasolina,
                DniPropietario = "12345678A",
                FechaMatriculacion = DateTime.UtcNow.AddDays(1),
                FechaInspeccion = DateTime.UtcNow.AddDays(10)
            };

            var res = _validador.Validar(vehiculo);

            res.IsSuccess.Should().BeFalse();
        }
        [TestCase(31)]
        [TestCase(-1)]
        public void Create_FechaInspeccionMayor30DiasOAnteriorActual_Error(int dias)
        {
            var vehiculo = new Vehiculo {
                Matricula = "1234JKL",
                Marca = "Seat",
                Cilindrada = 1200,
                TipoMotor = Motor.Gasolina,
                DniPropietario = "12345678A",
                FechaMatriculacion = DateTime.UtcNow.AddDays(-10),
                FechaInspeccion = DateTime.UtcNow.AddDays(dias)
            };

            var res = _validador.Validar(vehiculo);

            res.IsSuccess.Should().BeFalse();
        }
        
    }
    
}