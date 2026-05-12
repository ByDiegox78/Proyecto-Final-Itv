using System.Globalization;
using FluentAssertions;
using GestionItv.Models;
using ITV_Avanzado.Error.Report;
using ITV_Avanzado.Service.Report;

namespace ITV_Test.Service.Report;

[TestFixture]
public class ReportServiceTest {
    [SetUp]
    public void SetUp() {
        // Crear un subdirectorio temporal único para esta ejecución de tests
        _tempDirPath = Path.Combine(Path.GetTempPath(), $"ReportTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirPath);

        _service = new ReportService(_tempDirPath);
        _tempFiles = new List<string>();

        _originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("es-ES");
        CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
    }

    [TearDown]
    public void TearDown() {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalCulture;

        if (Directory.Exists(_tempDirPath))
            try {
                Directory.Delete(_tempDirPath, true);
            }
            catch {
            }
    }

    private ReportService _service = null!;
    private List<string> _tempFiles = null!;
    private CultureInfo _originalCulture = null!;
    private string _tempDirPath = null!;

    [TestFixture]
    public class CasosPositivos : ReportServiceTest {
        [Test]
        public void GenerarInformeCitaHtml_DeberiaIncluirDatosYDiferenciarMotores() {
            var hoy = DateTime.Today;
            var citas = new List<Vehiculo> {
                new() { Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
                    TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
                    FechaInspeccion = hoy.AddDays(5)
                },
                new() { Matricula = "2345BCF", Marca = "Toyota", Cilindrada = 1800,
                    TipoMotor = Motor.Hibrido, DniPropietario = "87654321X",
                    FechaInspeccion = hoy.AddDays(10)
                }
            };

            var resultado = _service.GenerarInformeCitaHtml(citas);

            var html = resultado.Value;
            html.Should().Contain("1234BCD");
            html.Should().Contain("2345BCF");
            html.Should().Contain("ECO");
            html.Should().Contain("Resumen de Citas ITV");
        }
        [Test]
        public void GenerarInformeCitaHtml_ConBorrados_DeberiaIncluirEliminados() {
            var citas = new List<Vehiculo> {
                new() { Matricula = "1234BCD", IsDeleted = true },
                new() { Matricula = "2345BCF", IsDeleted = false }
            };

            var resultado = _service.GenerarInformeCitaHtml(citas, true);

            var html = resultado.Value;
            html.Should().Contain("1234BCD");
            html.Should().Contain("2345BCF");
        }
        [Test]
        public void GuardarInforme_EnDirectorioTemporal_DeberiaPersistirCorrectamente() {
            var html = "<html><body>Test ITV</body></html>";
            var fileName = "informe_itv.html";
            var expectedPath = Path.Combine(_tempDirPath, fileName);

            var resultado = _service.GuardarInforme(html, fileName);

            resultado.IsSuccess.Should().BeTrue();
            File.Exists(expectedPath).Should().BeTrue();
            File.ReadAllText(expectedPath).Should().Be(html);
        }
        [Test]
        public void GuardarInformeHtml_ConHtmlValido_DeberiaGuardarArchivo() {
            var html = "<html><body><h1>Test ITV</h1></body></html>";
            var fileName = Path.Combine(_tempDirPath, "test.html");
    
            var resultado = _service.GuardarInformeHtml(html, fileName);
    
            resultado.IsSuccess.Should().BeTrue();
            File.Exists(fileName).Should().BeTrue();
            File.ReadAllText(fileName).Should().Be(html);
        }
    }
    [TestFixture]
    public class CasosNegativos : ReportServiceTest {
        [Test]
        public void GuardarInforme_ConNombreInvalido_DeberiaRetornarError() {
            var resultado = _service.GuardarInforme("html", "archivo\0invalido.html");

            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<ReportError.SaveError>();
            resultado.Error.Message.Should().Contain("Error al guardar el informe");
        }
    }
    [TestFixture]
    public class GenerarInformeCitaModelTests : ReportServiceTest {
        [Test]
        public void GenerarInformeCita_ConCitas_DeberiaCalcularEstadisticas() {
            var citas = new List<Vehiculo> {
                new() { Id = 1, TipoMotor = Motor.Gasolina, FechaInspeccion = DateTime.Today },
                new() { Id = 2, TipoMotor = Motor.Diesel, FechaInspeccion = DateTime.Today.AddDays(5) },
                new() { Id = 3, TipoMotor = Motor.Gasolina, FechaInspeccion = DateTime.Today }
            };

            var resultado = _service.GenerarInformeCita(citas);

            resultado.Should().NotBeNull();
            resultado.TotalCitas.Should().Be(3);
            resultado.Gasolina.Should().Be(2);
            resultado.Diesel.Should().Be(1);
            resultado.CitasParaHoy.Should().Be(2);
        }

        [Test]
        public void GenerarInformeCita_SinCitas_DeberiaRetornarVacio() {
            var citas = new List<Vehiculo>();

            var resultado = _service.GenerarInformeCita(citas);

            resultado.Should().NotBeNull();
            resultado.TotalCitas.Should().Be(0);
            resultado.Gasolina.Should().Be(0);
            resultado.CitasParaHoy.Should().Be(0);
        }

        [Test]
        public void GenerarInformeCita_ConTodosLosMotores_DeberiaContarCorrectamente() {
            var citas = new List<Vehiculo> {
                new() { TipoMotor = Motor.Gasolina },
                new() { TipoMotor = Motor.Diesel },
                new() { TipoMotor = Motor.Hibrido },
                new() { TipoMotor = Motor.Electrico }
            };

            var resultado = _service.GenerarInformeCita(citas);

            resultado.Gasolina.Should().Be(1);
            resultado.Diesel.Should().Be(1);
            resultado.Hibrido.Should().Be(1);
            resultado.Electrico.Should().Be(1);
        }
    }
    [TestFixture]
    public class GenerarInformeCitaHtmlCompletoTests : ReportServiceTest {
        [Test]
        public void GenerarInformeCitaHtml_ConCitasVariadas_DeberiaGenerarHtmlCompleto() {
            var citas = new List<Vehiculo> { new() {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", TipoMotor = Motor.Gasolina,
                    DniPropietario = "12345678Z", FechaInspeccion = new DateTime(2026, 5, 20)
                }, 
                new() {
                    Matricula = "2345BCF", Marca = "Toyota Prius", TipoMotor = Motor.Hibrido,
                    DniPropietario = "87654321X", FechaInspeccion = new DateTime(2026, 5, 25)
                }
            };

            var resultado = _service.GenerarInformeCitaHtml(citas);

            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().Contain("Resumen de Citas ITV");
            resultado.Value.Should().Contain("1234BCD");
            resultado.Value.Should().Contain("2345BCF");
            resultado.Value.Should().Contain("Seat Ibiza");
            resultado.Value.Should().Contain("Toyota Prius");
        }

        [Test]
        public void GenerarInformeCitaHtml_SinCitas_DeberiaGenerarHtmlVacio() {
            var citas = new List<Vehiculo>();

            var resultado = _service.GenerarInformeCitaHtml(citas);

            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().Contain("Resumen de Citas ITV");
        }
    }

    [TestFixture]
    public class GenerarInformePdfCompletoTest : ReportServiceTest {
        [Test]
        public void GuardarInformePdf_ConHtmlValido_DeberiaGenerarArchivo() {
            var html = "<html><body><h1>Test ITV</h1></body></html>";
            var fileName = Path.Combine(_tempDirPath, "test.pdf");
    
            var resultado = _service.GuardarInformePdf(html, fileName);
    
            resultado.IsSuccess.Should().BeTrue();
            File.Exists(fileName).Should().BeTrue();
            new FileInfo(fileName).Length.Should().BeGreaterThan(0);
        }
        [Test]
        public void GuardarInformePdf_ConNombreInvalido_DeberiaRetornarError() {
            var resultado = _service.GuardarInformePdf("<html></html>", "archivo\0invalido.pdf");
            resultado.IsFailure.Should().BeTrue();
        }
    }
}