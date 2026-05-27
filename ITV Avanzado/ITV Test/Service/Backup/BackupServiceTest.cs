    using System.IO.Compression;
    using FluentAssertions;
    using GestionItv.Models;
    using ITV_Avanzado.Error.Buckup;
    using ITV_Avanzado.Error.Common;
    using ITV_Avanzado.Service.Buckup;
    using ITV_Avanzado.Storage.Common;
    using Moq;
    using CSharpFunctionalExtensions;

    namespace ITV_Test.Service.Backup;

    [TestFixture]
    public class BackupServiceTest {
        [SetUp]
        public void SetUp() {
            _tempDir = Path.Combine(Path.GetTempPath(), $"BackupTest_{Guid.NewGuid()}");
            _backupDir = Path.Combine(_tempDir, "backups");
            _imagesDir = Path.Combine(_tempDir, "images");
            Directory.CreateDirectory(_backupDir);
            Directory.CreateDirectory(_imagesDir);

            _storageMock = new Mock<IStorage<Cita>>();

            _service = new BackupService(_storageMock.Object, _backupDir);
        }

        [TearDown]
        public void TearDown() {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        private string _tempDir = null!;
        private string _backupDir = null!;
        private string _imagesDir = null!;
        private BackupService _service = null!;
        private Mock<IStorage<Cita>> _storageMock = null!;

        [TestFixture]
        public class CasosPositivos : BackupServiceTest {
            [Test]
            public void RealizarBackup_ConListaVacia_DeberiaRetornarErrorCreationError() {
                var vehiculos = new List<Cita>();

                var resultado = _service.RealizarBackup(vehiculos);

                resultado.IsFailure.Should().BeTrue();
                resultado.Error.Should().BeOfType<BackupError.CreationError>();
                resultado.Error.Message.Should().Contain("No hay datos para respaldar");
            }
            [Test]
            public void RealizarBackup_ConDirectorioCustom_DeberiaCrearEnEseDirectorio() {
                var customDir = Path.Combine(_tempDir, "custom-backup");
                Directory.CreateDirectory(customDir);

                var vehiculo = new Cita {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };

                _storageMock.Setup(s => s.Salvar(It.IsAny<IEnumerable<Cita>>(), It.IsAny<string>()))
                    .Returns(Result.Success<bool, DomainError>(true));

                var resultado = _service.RealizarBackup(new List<Cita> {vehiculo}, customDir);
                
                resultado.IsSuccess.Should().BeTrue();
                resultado.Value.Should().StartWith(customDir);
                File.Exists(resultado.Value).Should().BeTrue();
            }
            [Test]
            public void RestaurarBackup_ConArchivoInexistente_DeberiaRetornarErrorFileNotFound() {
                var archivoInexistente = Path.Combine(_tempDir, "no_existe.zip");

                var resultado = _service.RestaurarBackup(archivoInexistente);

                resultado.IsFailure.Should().BeTrue();
                resultado.Error.Should().BeOfType<BackupError.FileNotFound>();
                resultado.Error.Message.Should().Contain("no_existe.zip");
            }
            [Test]
            public void RestaurarBackup_ConZipValido_DeberiaRetornarPersonas() {
                var vehiculo = new Cita {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };

                _storageMock.Setup(s => s.Cargar(It.IsAny<string>()))
                    .Returns(Result.Success<IEnumerable<Cita>, DomainError>(new List<Cita>{ vehiculo}));

                var zipPath = Path.Combine(_backupDir, "test-back.zip");
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                    zip.CreateEntry("data/citas.json");
                }
                
                var resultado = _service.RestaurarBackup(zipPath);
                
                resultado.IsSuccess.Should().BeTrue();
                resultado.Value.Should().HaveCount(1);
            }
            [Test]
            public void ListarBackups_SinBackups_DeberiaRetornarListaVacia() {
                var resultado = _service.ListarBackups();

                resultado.Should().BeEmpty();
            }
            [Test]
            public void RestaurarBackupSistema_ConCallbackExitoso_DeberiaRetornarContador() {
                var vehiculo = new Cita {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };              
                var vehiculos = new List<Cita> { vehiculo };
            
                _storageMock.Setup(s => s.Cargar(It.IsAny<string>()))
                    .Returns(Result.Success<IEnumerable<Cita>, DomainError>(vehiculos));
            
                var zipPath = Path.Combine(_backupDir, "test-back.zip");
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                    zip.CreateEntry("data/citas.json");
                }
            
                var deleteCallback = () => true;
                Func<Cita, Result<Cita, DomainError>> callback = p => Result.Success<Cita, DomainError>(p);
            
                var resultado = _service.RestaurarBackupSistema(zipPath, deleteCallback, callback);
            
                resultado.IsSuccess.Should().BeTrue();
                resultado.Value.Should().Be(1);
            }
        }

        [TestFixture]
        public class CasosNegativos : BackupServiceTest {
            [Test]
            public void RealizarBackup_SinDirectorio_DeberiaLanzarExcepcion() {
                var serviceSinDirectorio = new BackupService(_storageMock.Object);
                var vehiculos = new List<Cita> { new Cita { Id = 1, Matricula = "1234BCD"} };

                serviceSinDirectorio.Invoking(s => s.RealizarBackup(vehiculos))
                    .Should().Throw<InvalidOperationException>()
                    .WithMessage("*directorio*");
            }
            [Test]
            public void RestaurarBackup_ConArchivoCorrupto_DeberiaRetornarErrorInvalidBackupFile() {
                // Arrange
                var zipPath = Path.Combine(_backupDir, "corrupto.zip");
                File.WriteAllText(zipPath, "Esto no es un ZIP válido");

                // Act
                var resultado = _service.RestaurarBackup(zipPath);

                // Assert
                resultado.IsFailure.Should().BeTrue();
                resultado.Error.Should().BeOfType<BackupError.InvalidBackupFile>();
            }
            [Test]
            public void RestaurarBackup_ConZipSinDataJson_DeberiaRetornarErrorInvalidBackupFile() {
                var zipPath = Path.Combine(_backupDir, "sin-data.zip");
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                    zip.CreateEntry("otro-archivo.txt");
                }

                var resultado = _service.RestaurarBackup(zipPath);

                resultado.IsFailure.Should().BeTrue();
                resultado.Error.Should().BeOfType<BackupError.InvalidBackupFile>();
                resultado.Error.Message.Should().Contain("datos válidos");
            }
            [Test]
            public void RealizarBackup_ConErrorDeDirectorio_DeberiaRetornarErrorDirectoryError() {
                var service = new BackupService(_storageMock.Object, "C:\\:invalido\\ruta");
                var vehiculos = new List<Cita> { new Cita { Id = 1, Matricula = "1234RTY"} };

                // Act
                var resultado = service.RealizarBackup(vehiculos, "C:\\:invalido\\ruta");

                // Assert
                resultado.IsFailure.Should().BeTrue();
                resultado.Error.Should().BeOfType<BackupError.DirectoryError>();
                resultado.Error.Message.Should().Contain("directorio");
            }
            [Test]
            public void RealizarBackup_ConErrorDeEscritura_DeberiaRetornarError() {
                // Arrange
                var vehiculos = new List<Cita> { new Cita { Id = 1, Matricula = "1234RTY"} };
                var error = new BackupError.CreationError("Error de escritura");

                _storageMock.Setup(s => s.Salvar(It.IsAny<IEnumerable<Cita>>(), It.IsAny<string>()))
                    .Returns(Result.Failure<bool, DomainError>(error));

                // Act
                var resultado = _service.RealizarBackup(vehiculos, _backupDir);

                // Assert
                resultado.IsFailure.Should().BeTrue();
                resultado.Error.Should().BeOfType<BackupError.CreationError>();
            }
            [Test]
            public void RestaurarBackupSistema_ConCallbackFallido_DeberiaRetornarError() {
                // Arrange
                var vehiculo = new Cita { Id = 1, Matricula = "1234HJK"};
                var vehiculos = new List<Cita> { vehiculo };
            
                _storageMock.Setup(s => s.Cargar(It.IsAny<string>()))
                    .Returns(Result.Success<IEnumerable<Cita>, DomainError>(vehiculos));
            
                var zipPath = Path.Combine(_backupDir, "test.zip");
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                    zip.CreateEntry("data/personas.json");
                }
            
                var deleteCallback = () => true;
                Func<Cita, Result<Cita, DomainError>> callback = p =>
                    Result.Failure<Cita, DomainError>(BackupErrors.CreationError("Error al crear vehiculos"));
            
                // Act
                var resultado = _service.RestaurarBackupSistema(zipPath, deleteCallback, callback);
            
                // Assert
                resultado.IsFailure.Should().BeTrue();
                resultado.Error.Message.Should().Contain("El archivo de backup es inválido o está corrupto: El archivo de backup no contiene datos válidos.");
            }
            [Test]
            public void RestaurarBackup_ConZipVacio_DeberiaRetornarError() {
                // Arrange
                var zipPath = Path.Combine(_backupDir, "vacio.zip");
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                    // Zip vacío sin entradas
                }

                // Act
                var resultado = _service.RestaurarBackup(zipPath);

                // Assert
                resultado.IsFailure.Should().BeTrue();
                resultado.Error.Should().BeOfType<BackupError.InvalidBackupFile>();
            }
        }
    }
