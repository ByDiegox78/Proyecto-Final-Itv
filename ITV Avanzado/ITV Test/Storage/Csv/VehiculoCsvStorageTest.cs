using System.Text;
using FluentAssertions;
using GestionItv.Models;
using ITV_Avanzado.Error.Storage;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Storage.Csv;

namespace ITV_Test.Storage.Csv;

[TestFixture]
public class VehiculoCsvStorageTest {
    [SetUp]
    public void SetUp() {
        _storage = new VehiculoCsvStorage();
        _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
    }

    [TearDown]
    public void TearDown() {
        if (File.Exists(_path)) File.Delete(_path);
    }

    private VehiculoCsvStorage _storage = null!;
    private string _path = null!;

    [TestFixture]
    public class CasosPositivos {
        [SetUp]
        public void SetUp() {
            _storage = new VehiculoCsvStorage();
            _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
        }
        [TearDown]
        public void TearDown() {
            if (File.Exists(_path)) File.Delete(_path);
        }

        private VehiculoCsvStorage _storage = null!;
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
    }

    [TestFixture]
    public class CasosNegativos {
        [SetUp]
        public void SetUp() {
            _storage = new VehiculoCsvStorage();
            _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
        }
        [TearDown]
        public void TearDown() {
            if (File.Exists(_path)) File.Delete(_path);
        }

        private VehiculoCsvStorage _storage = null!;
        private string _path = null!;

        [Test]
        public void Cargar_SiArchivoNoExiste_RetornaError() {
            var res = _storage.Cargar("no/existe.csv");

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<StorageError.FileNotFound>();
            (res.Error as StorageError.FileNotFound)?.FilePath.Should().Be("no/existe.csv");
            res.Error.Message.Should().Contain("no/existe.csv");
        }

        [Test]
        public void Salvar_EnRutaInvalida_DevuelveError() {
            var vehiculos = new List<Vehiculo>();
            var res = _storage.Salvar(vehiculos, "/Invalida/arch.csv");

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<StorageError.WriteError>();
            res.Error.Message.Should().Contain("Error al escribir");
        }

        [Test]
        public void Cargar_CuandoElArchivoTieneFormatoInvalido_DevuelveError() {
            using var writer = new StreamWriter(_path, false, Encoding.UTF8);
            writer.WriteLine("Id;Matricula;Marca;Cilindrada;TipoMotor;DniPropietario;IsDelete;CreatedAt;UpdatedAt;");
            writer.WriteLine("1;1234BCD;Seat Ibiza;1200;Gasolina;01234567L;False;2024-01-17 00:00:00;2024-01-17 00:00:00;");

            var res = _storage.Cargar(_path);

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<StorageError.InvalidFormat>();
        }
    }
}