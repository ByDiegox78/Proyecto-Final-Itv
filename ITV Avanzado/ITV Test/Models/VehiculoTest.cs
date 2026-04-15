using FluentAssertions;
using GestionItv.Models;

namespace ITV_Test.Models;

[TestFixture]
public class VehiculoTest {
    [TestFixture]
    public class CasosPositivos {
        [Test]
        public void ToString_RetornaFormatoValido() {
            var fecha = DateTime.UtcNow;

            var vehiculo = new Vehiculo(
                Id: 1,
                Matricula: "4196FMR",
                Marca: "Fiat",
                Cilindrada: 900,
                TipoMotor: Motor.Diesel,
                DniPropietario: "54407737H",
                IsDeleted: false,
                CreatedAt: fecha,
                UpdatedAt: fecha
            );
            var result = vehiculo.ToString();
            
            result.Should().Contain("1");
            result.Should().Contain("4196FMR");
            result.Should().Contain("Fiat");
            result.Should().Contain("900");
            
        }
    }
}