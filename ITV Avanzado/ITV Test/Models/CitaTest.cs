using FluentAssertions;
using GestionItv.Models;

namespace ITV_Test.Models;

[TestFixture]
public class CitaTest {
    [TestFixture]
    public class CasosPositivos {
        [Test]
        public void ToString_RetornaFormatoValido() {
            var vehiculo = new Cita {
                Id = 1, Matricula = "4196FMR", Marca = "Fiat", Cilindrada = 900, TipoMotor = Motor.Diesel,
                DniPropietario = "54407737H", FechaMatriculacion = DateTime.UtcNow, FechaInspeccion = DateTime.UtcNow
            };
            var result = vehiculo.ToString();
            result.Should().Contain("1");
            result.Should().Contain("4196FMR");
            result.Should().Contain("Fiat");
            result.Should().Contain("900");
            result.Should().Contain("Diesel");
            result.Should().Contain($"{DateTime.UtcNow}");
            result.Should().Contain($"{DateTime.UtcNow}");

        }
    }
}