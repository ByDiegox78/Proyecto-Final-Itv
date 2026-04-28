using System.Text;
using FluentAssertions;
using GestionItv.Models;
using ITV_Avanzado.Error.Storage;
using ITV_Avanzado.Storage.Json;

namespace ITV_Test.Storage.Json;
[TestFixture]
public class VehiculoJsonStorageTest {
    [SetUp]
    public void SetUp() {
        _storage = new VehiculoJsonStorage();
        _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
    }

    [TearDown]
    public void TearDown() {
        if (File.Exists(_path)) File.Delete(_path);
    }

    private VehiculoJsonStorage _storage = null!;
    private string _path = null!;

    [TestFixture]
    public class CasosPositivos {
        [SetUp]
        public void SetUp() {
            _storage = new VehiculoJsonStorage();
            _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        }

        [TearDown]
        public void TearDown() {
            if (File.Exists(_path)) File.Delete(_path);
        }

        private VehiculoJsonStorage _storage = null!;
        private string _path = null!;

        [Test]
        public void Salvar_ConDatosValidos_DevuelveCorrecto() {
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
        [Test]
        public void Salvar_ListaVacia_DeberiaCrearArchivoVacio() {
            // Arrange
            var vehiculos = new List<Vehiculo>();

            // Act
            var resultado = _storage.Salvar(vehiculos, _path);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            File.Exists(_path).Should().BeTrue();
        }
    }

    [TestFixture]
    public class CasosNegativos {
        [SetUp]
        public void SetUp() {
            _storage = new VehiculoJsonStorage();
            _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        }

        [TearDown]
        public void TearDown() {
            if (File.Exists(_path)) File.Delete(_path);
        }

        private VehiculoJsonStorage _storage = null!;
        private string _path = null!;

        [Test]
        public void Cargar_CuandoArchivoNoExists_DeberiaDevolverError() {
            var res = _storage.Cargar("json/inexistente.json");

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<StorageError.FileNotFound>();
            (res.Error as StorageError.FileNotFound)?.FilePath.Should().Be("json/inexistente.json");
            res.Error.Message.Should().Contain("json/inexistente.json");
        }
        [Test]
        public void Salvar_EnRutaInvalida_DeberiaRetornarError() {
            // Arrange
            var vehiculos = new List<Vehiculo>();

            // Act
            var res = _storage.Salvar(vehiculos, "invalida/archivo.json");

            // Assert
            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<StorageError.WriteError>();
            res.Error.Message.Should().Contain("Error al escribir");
        }
    }
}