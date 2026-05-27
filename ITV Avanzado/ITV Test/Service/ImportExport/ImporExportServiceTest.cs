using GestionItv.Models;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Service.ImportExport;
using ITV_Avanzado.Storage.Common;
using Moq;
using CSharpFunctionalExtensions;
using FluentAssertions;
using ITV_Avanzado.Error.Citas;
using NUnit.Framework.Internal;


namespace ITV_Test.Service;

[TestFixture]
public class ImporExportServiceTest {
    [SetUp]
    public void SetUp() {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ImportExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        _storageMock = new Mock<IStorage<Cita>>();
        _service = new ImportExportService(_storageMock.Object);
    }

    [TearDown]
    public void TearDown() {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string _tempDir = null!;
    private ImportExportService _service = null!;
    private Mock<IStorage<Cita>> _storageMock = null!;

    [TestFixture]
    public class CasosPositivos : ImporExportServiceTest {
        [Test]
        public void Exportar_ConCitas_DevuelveContador() {
            var citas = new List<Cita> {
                new Cita { Matricula = "1234VCF", Marca = "Alfa" },
                new Cita { Matricula = "1234VCG", Marca = "Renault" },
                new Cita { Matricula = "1234VCH", Marca = "Fiat" }
            };
            var path = Path.Combine(_tempDir, "export.json");
            _storageMock.Setup(s => s.Salvar(It.IsAny<IEnumerable<Cita>>(), It.IsAny<string>()))
                .Returns(Result.Success<bool, DomainError>(true));

            var res = _service.ExportarDatos(citas, path);
            
            res.IsSuccess.Should().BeTrue();
            res.Value.Should().Be(3);
        }

        [Test]
        public void Import_ConArchivo_DevuelveCitas() {
            var citas = new List<Cita> {
                new Cita { Matricula = "1234VCF", Marca = "Alfa" },
                new Cita { Matricula = "1234VCG", Marca = "Renault" },
                new Cita { Matricula = "1234VCH", Marca = "Fiat" }
            };
            var path = Path.Combine(_tempDir, "export.json");
            _storageMock.Setup(s => s.Cargar(path))
                .Returns(Result.Success<IEnumerable<Cita>, DomainError>(citas));
            
            var res = _service.ImportarDatos(path);
            
            res.IsSuccess.Should().BeTrue();
            res.Value.Should().HaveCount(3);
            res.Value.First().Matricula.Should().Be("1234VCF");
            res.Value.Last().Matricula.Should().Be("1234VCH");
        }

        [Test]
        public void Exportar_DeberiaLLamarExportarDatosConRutaVacia() {
            var citas = new List<Cita> {
                new Cita { Matricula = "1234VCF", Marca = "Alfa" },
                new Cita { Matricula = "1234VCG", Marca = "Renault" },
                new Cita { Matricula = "1234VCH", Marca = "Fiat" }
            };
            
            _storageMock.Setup(s => s.Salvar(It.IsAny<IEnumerable<Cita>>(), It.IsAny<string>()))
                .Returns(Result.Success<bool, DomainError>(true));
            
            var resultado = _service.ExportarDatosSistema(citas);
            
            resultado.IsSuccess.Should().BeTrue();
            _storageMock.Verify(s => s.Salvar(It.IsAny<IEnumerable<Cita>>(), string.Empty), Times.Once);

        }
        [Test]
        public void ImportarDatosSistema_ConRuta_DeberiaLLamarImportarDatos() {
            var path = Path.Combine(_tempDir, "test.json");
            var citas = new List<Cita> {
                new Cita { Matricula = "1234VCF", Marca = "Alfa" },
                new Cita { Matricula = "1234VCG", Marca = "Renault" },
                new Cita { Matricula = "1234VCH", Marca = "Fiat" }
            };
            
            _storageMock.Setup(s => s.Cargar(path))
                .Returns(Result.Success<IEnumerable<Cita>, DomainError>(citas));

            var resultado = _service.ImportarDatosSistema(path);

            resultado.IsSuccess.Should().BeTrue();
            _storageMock.Verify(s => s.Cargar(path), Times.Once);
        }
    }

    [TestFixture]
    public class CasosNegativos : ImporExportServiceTest {
        [Test]
        public void ImportarDatos_ConError_DeberiaRetornarError() {
            // Arrange
            var path = Path.Combine(_tempDir, "no-existe.json");

            _storageMock.Setup(s => s.Cargar(path))
                .Returns(Result.Failure<IEnumerable<Cita>, DomainError>(
                    CitaErrors.StorageError("File not found")));

            // Act
            var resultado = _service.ImportarDatos(path);

            // Assert
            resultado.IsFailure.Should().BeTrue();
        }

    }
}