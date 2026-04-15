using FluentAssertions;
using GestionItv.Models;

namespace ITV_Test.Models;
[TestFixture]
public class InformeTest {
    [TestFixture]
    public class CasosPositivos {
        [Test]
        public void ToString_RetornaFormatoValido() {
            var fecha = DateTime.UtcNow;

            var informe = new Informe {
                Id = 1,
                Matricula = "4196FMR",
                Marca = "Fiat",
                Cilindrada = 900,
                DatosMotor = "Diesel",
                PropietarioDni = "54407737H"
            };
            
            var result = informe.ToString();
            
            result.Should().Contain("1");
            result.Should().Contain("4196FMR");
            result.Should().Contain("Fiat");
            result.Should().Contain("Diesel");
            result.Should().Contain("900");
        }
    }
}
