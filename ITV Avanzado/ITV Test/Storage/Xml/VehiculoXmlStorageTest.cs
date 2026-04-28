using System.Text;
using System.Xml.Serialization;
using FluentAssertions;
using GestionItv.Models;
using ITV_Avanzado.Dto;
using ITV_Avanzado.Error.Storage;
using ITV_Avanzado.Storage.Xml;

namespace ITV_Test.Storage.Xml;

[TestFixture]
public class VehiculoXmlStorageTest {
    [SetUp]
    public void Setup() {
        _storage = new VehiculoXmlStorage();
        _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xml");
    }
    [TearDown]
    public void TearDown() {
        if (File.Exists(_path)) File.Delete(_path);
    }
    private VehiculoXmlStorage _storage = null!;
    private string _path = null!;

    [TestFixture]
    public class CasosPositivos {
        [SetUp]
        public void Setup() {
            _storage = new VehiculoXmlStorage();
            _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xml");
        }
        [TearDown]
        public void TearDown() {
            if (File.Exists(_path)) File.Delete(_path);
        }
        private VehiculoXmlStorage _storage = null!;
        private string _path = null!;

        [Test]
        public void Salvar_GuardarVehiculoValido_GuardaCorrectamente() {
            var vehiculos = new List<Vehiculo> {
                new Vehiculo {
                    Id = 1,
                    Matricula = "1234BCD",
                    Marca = "Seat Ibiza",
                    Cilindrada = 1200,
                    TipoMotor = Motor.Gasolina,
                    DniPropietario = "01234567L",
                    FechaMatriculacion = new DateTime(2024, 01, 17),
                    FechaInspeccion = new DateTime(2024, 01, 17),
                    IsDeleted = false,
                    CreatedAt = new DateTime(2024, 01, 17),
                    UpdatedAt = new DateTime(2024, 01, 17)
                }
            };
            var res = _storage.Salvar(vehiculos, _path);

            res.IsSuccess.Should().BeTrue();
            File.Exists(_path).Should().BeTrue();
        }
    }
    [Test]
    public void Cargar_ConArchivos_DebeDevolverDatos() {
        var vehiculos = new List<Vehiculo> {
            new Vehiculo {
                Id = 1,
                Matricula = "1234BCD",
                Marca = "Seat Ibiza",
                Cilindrada = 1200,
                TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L",
                FechaMatriculacion = new DateTime(2024, 01, 17),
                FechaInspeccion = new DateTime(2024, 01, 17),
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 01, 17),
                UpdatedAt = new DateTime(2024, 01, 17)
            }
        };
        _storage.Salvar(vehiculos, _path);

        var res = _storage.Cargar(_path);

        res.IsSuccess.Should().BeTrue();
        res.Value.Should().HaveCount(1);
        res.Value.First().Matricula.Should().Be("1234BCD");
    }
    

    [TestFixture]
    public class CasosNegativos {
        [SetUp]
        public void Setup() {
            _storage = new VehiculoXmlStorage();
            _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xml");
        }
        [TearDown]
        public void TearDown() {
            if (File.Exists(_path)) File.Delete(_path);
        }
        private VehiculoXmlStorage _storage = null!;
        private string _path = null!;
        
        [Test]
        public void Cargar_SiArchivoNoExiste_RetornaError() {
            var res = _storage.Cargar("no/existe.xml");

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<StorageError.FileNotFound>();
            (res.Error as StorageError.FileNotFound)?.FilePath.Should().Be("no/existe.xml");
            res.Error.Message.Should().Contain("no/existe.xml");
        }
        [Test]
        public void Salvar_EnRutaInvalida_DevuelveError() {
            var vehiculos = new List<Vehiculo>();
            var res = _storage.Salvar(vehiculos, "/Invalida/arch.xml");

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<StorageError.WriteError>();
            res.Error.Message.Should().Contain("Error al escribir");
        }
        
    }
}