using FluentAssertions;
using GestionItv.Models;
using ITV_Avanzado.Error.Citas;
using ITV_Avanzado.Repository.Binary;

namespace ITV_Test.Repository.Binary;

[TestFixture]
public class CitaBinaryRepositoryTest {
    [TestFixture]
    public class CasosPositios {
        [SetUp]
        public void SetUp() {
            _temp = Path.GetTempFileName();
            if (File.Exists(_temp)) File.Delete(_temp); 
    
            _repository = new CitaBinaryRepository(_temp, true, false);
        }

        [TearDown]
        public void TearDown() {
            if (File.Exists(_temp))
                File.Delete(_temp);
        }

        private string _temp = null!;
        private CitaBinaryRepository _repository = null!;
        
        [Test]
        public void Load_DebeRecuperarDatos_CuandoSeInstanciaNuevoRepositorio() {
            // Arrange
            var vehiculo = new Cita { Matricula = "1234ABC", Marca = "Prueba", DniPropietario = "123" };
            _repository.Create(vehiculo);
            
            var nuevoRepo = new CitaBinaryRepository(_temp, false, false);
            var resultado = nuevoRepo.GetByMatricula("1234ABC");

            // Assert
            resultado.Should().NotBeEmpty();
            resultado.First().Marca.Should().Be("Prueba");
        }

        [Test]
        public void Constructor_ConSeedData_CargaDatosIniciales() {

            var repoConSemilla = new CitaBinaryRepository(_temp, true, true);

            repoConSemilla.GetAll(1, 10, false, null).Should().NotBeEmpty();
        }

        [Test]
        public void Constructor_DropDataYFileExiste_EliminaCorrectamente() {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "datos previos");
            var res = new CitaBinaryRepository(tempFile, dropData: true, seedData: false);
            
            res.GetAll(1, 10, false, null).Should().BeEmpty();
            

        }

        [Test]
        public void Create_CrearVehiculo_CreaCorrectamente() {
            var vehiculo = new Cita {
                Matricula = "1234BCD",
                Marca = "Seat Ibiza",
                Cilindrada = 1200,
                TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L",
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 01, 17),
                UpdatedAt = new DateTime(2024, 01, 17)
            };

            var res = _repository.Create(vehiculo);

            res.IsSuccess.Should().BeTrue();
            res.Value.Id.Should().Be(1);
            res.Value.DniPropietario.Should().Be("01234567L");
            res.Value.Matricula.Should().Be("1234BCD");
        }

        [Test]
        public void GetById_ConVehiculoExistente_DevuelveVehiculoCorrecto() {
            var vehiculo = new Cita {
                Matricula = "1234BCD",
                Marca = "Seat Ibiza",
                Cilindrada = 1200,
                TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L",
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 01, 17),
                UpdatedAt = new DateTime(2024, 01, 17)
            };
            _repository.Create(vehiculo);
            var res = _repository.GetById(1);

            res.Should().NotBeNull();
            res!.Id.Should().Be(1);
            res.DniPropietario.Should().Be("01234567L");
            res.Matricula.Should().Be("1234BCD");
        }
        [Test]
        public void GetAll_SinBorrados_DebeDevolverSoloActivos() {
            var p = _repository.Create(new Cita {
                Matricula = "4567BCH",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "41234571X",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            }).Value;
            _repository.Create(new Cita {
                Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });
            _repository.Create(new Cita {
                Matricula = "2345BCF",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });
            _repository.Create(new Cita {
                Matricula = "3456BCG",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });

            _repository.Delete(p.Id);
            var res = _repository.GetAll(1, 10, false, null);

            res.Should().HaveCount(3);
            res.First().Matricula.Should().Be("1234BCD");
        }

        [Test]
        public void Update_ConDatosValidos_ActualizaCorrectamente() {
            _repository.Create(new Cita {
                Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });
            var act = new Cita {
                Matricula = "3456BCS", Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Diesel,
                DniPropietario = "51234572W"
            };
            var res = _repository.Update(1, act);

            res.IsSuccess.Should().BeTrue();
            res.Value.Marca.Should().Be("Fiat");
            res.Value.TipoMotor.Should().Be(Motor.Diesel);
        }
        [Test]
        public void Restore_DevuelveVehiculo_DevuelveCorrectamente() {

            _repository.Create(new Cita {
                Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L", IsDeleted = false,
                CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });
            var vehiculo = _repository.GetById(1);
            _repository.Delete(vehiculo.Id);
            var res = _repository.Restore(vehiculo.Id);

            res.IsSuccess.Should().BeTrue();
            res.Value.IsDeleted.Should().Be(false);
            res.Value.UpdatedAt.Should().BeAfter(new DateTime(2024, 01, 17));
            res.Value.TipoMotor.Should().Be(vehiculo.TipoMotor);
        }
        [Test]
        public void Delete_ConBoradoFisico_EliminaCorrectamente() {
            _repository = new CitaBinaryRepository(_temp,true, false);
            var vehiculo = new Cita {
                Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L", FechaInspeccion = new DateTime(2024, 01, 17),
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            };
            _repository.Create(vehiculo);
            var res = _repository.Delete(1, false);
            
            res.Should().NotBeNull();
            _repository.GetById(1).Should().BeNull();
        }
        [Test]
        public void DeleteAll_EliminaTodo_DevuelveTrue() {
            var vehiculo = new Cita {
                Matricula = "1234BCD", Marca = "Test", Cilindrada = 1000, 
                TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z"
            };
            _repository.Create(vehiculo);

            var resultado = _repository.DeleteAll();

            resultado.Should().BeTrue();
            _repository.GetAll(1, 10, false, null).Should().BeEmpty();
        }
         [TestCase("1234BCD")]
        [TestCase("Seat Ibiza")]
        [TestCase("01234567L")]
        public void GetAll_ConBusquedaPersonalizada_DevuelveCorrecto(string campo) {
            var vehiculo = new Cita {
                Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Diesel,
                DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            };
            var vehiculo2 = new Cita {
                Matricula = "2345BCF",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            };
            var vehiculo3 = new Cita {
                Matricula = "3456BCG",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            };
            var vehiculo4 = new Cita {
                Matricula = "4567BCH",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            };
            _repository.Create(vehiculo);
            _repository.Create(vehiculo2);
            _repository.Create(vehiculo3);
            _repository.Create(vehiculo4);

            var res = _repository.GetAll(1, 2, false, campo);

            var primero = res.First();
            res.Should().NotBeNull();
            (primero.Matricula == campo || 
             primero.Marca == campo || 
             primero.DniPropietario == campo)
                .Should().BeTrue($"porque el resultado debe coincidir con el término de búsqueda '{campo}'");
        }
        
    }

    [TestFixture]
        public class CasosNegativos {
            [SetUp]
            public void SetUp() {
                _temp = Path.GetTempFileName();
    
                if (File.Exists(_temp)) File.Delete(_temp); 
    
                _repository = new CitaBinaryRepository( _temp, dropData: true, seedData: false);
            }

            [TearDown]
            public void TearDown() {
                if (File.Exists(_temp))
                    File.Delete(_temp);
            }

            private string _temp = null!;
            private CitaBinaryRepository _repository = null!;

            [Test]
            public void Create_CrearVehiculoConMatriculaExistente_DebeDevolverError() {
                var vehiculo = new Cita {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                    DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var vehiculo2 = new Cita {
                    Matricula = "1234BCD",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };

                _repository.Create(vehiculo);
                var res = _repository.Create(vehiculo2);

                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<CitaError.MatriculaInspeccionDuplicada>();
                res.Error.Message.Should()
                    .Contain(
                        "1234BCD");
            }
             [Test]
            public void Create_CrearVehiculoConDniConMaximoVehiculo_DebeDevolverError() {
                var vehiculo = new Cita {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                    DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var vehiculo2 = new Cita {
                    Matricula = "2345BCF",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var vehiculo3 = new Cita {
                    Matricula = "3456BCG",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var vehiculo4 = new Cita {
                    Matricula = "4567BCH",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };

                _repository.Create(vehiculo);
                _repository.Create(vehiculo2);
                _repository.Create(vehiculo3);
                var res = _repository.Create(vehiculo4);

                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<CitaError.MaxCitasUsageDniError>();
                res.Error.Message.Should()
                    .Contain(
                        "El propietario con dni: 01234567L tiene 3 vehiculos para inspeccion para el mismo dia");
            }
            [Test]
            public void Update_VehiculoNoExiste_DevuelveError() {
                var vehiculo4 = new Cita {
                    Matricula = "4567BCH",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var res = _repository.Update(2, vehiculo4);

                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<CitaError.NotFound>();
                (res.Error as CitaError.NotFound)?.Id.Should().Be("2");

            }
            [Test]
            public void Update_VehiculoMatriculaExistente_DevuelveError() {
                var vehiculo = new Cita {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var v2 = new Cita {
                    Matricula = "6789BCW", Marca = "Mazda 3", Cilindrada = 1800, TipoMotor = Motor.Hibrido, DniPropietario = "61234573V",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                _repository.Create(vehiculo);
                _repository.Create(v2);
                
                var res = _repository.Update(2, vehiculo);

                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<CitaError.MatriculaInspeccionDuplicada>();
                (res.Error as CitaError.MatriculaInspeccionDuplicada)?.matricula.Should().Be("1234BCD");

            }
            [Test]
            public void  Update_PropietarioConLimiteDeVehiculoSuperado_DebeDevolverError() {
                var v1 = new Cita {
                    Matricula = "4567BCH",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "41234571X",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var v2 = new Cita {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "41234571X",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var v3 = new Cita {
                    Matricula = "2345BCF", Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "41234571X",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var v4 = new Cita {
                    Matricula = "3456BDG",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var actu = new Cita {
                    DniPropietario = "41234571X",
                };
                _repository.Create(v1);
                _repository.Create(v2);
                _repository.Create(v3);
                _repository.Create(v4);

                var res = _repository.Update(4, actu);
                
                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<CitaError.MaxCitasUsageDniError>();
                (res.Error as CitaError.MaxCitasUsageDniError)?.Dni.Should().Be("41234571X");
            }
            [Test]
            public void Delete_CuandoNoExiste_DeberiaRetornarNull() {
                var res = _repository.Delete(1);
                
                res.Should().BeNull();
            }
        
            [Test]
            public void Restore_VehiculoNoExiste_DevuelveError() {
                var res = _repository.Restore(999);

                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<CitaError.NotFound>();
            }
            [Test]
            public void GetByMatricula_VehiculoEliminado_DevuelveNull() {
                var vehiculo = new Cita {
                    Matricula = "1234BCD",
                    Marca = "Seat Ibiza",
                    Cilindrada = 1200,
                    TipoMotor = Motor.Gasolina,
                    DniPropietario = "01234567L",
                    IsDeleted = false
                };
                var creado = _repository.Create(vehiculo).Value;
                _repository.Delete(creado.Id); 
                var res = _repository.GetByMatricula("1234BCD");

                res.Should().NotBeNull();
                res.Should().BeEmpty();
            }
            [Test]
            public void GetByMatricula_NoExisteDevuelveNull() {
                var res = _repository.GetByMatricula("4196FMR");

                res.Should().NotBeNull();
                res.Should().BeEmpty();
            }
        }
}