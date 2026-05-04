using System.Data;
using Dapper;
using FluentAssertions;
using GestionItv.Models;
using ITV_Avanzado.Error.Vehiculos;
using ITV_Avanzado.Repository.Dapper;
using Microsoft.Data.Sqlite;

namespace ITV_Test.Repository.Dapper;

[TestFixture]
public class VehiculoDapperRepositoryTest {
    [TestFixture]
    public class CasosValidos {
        [SetUp]
        public void SetUp() {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _repository = new VehiculoDapperRepository(_connection);
        }

        [TearDown]
        public void TearDown() {
            _connection.Close();
            _connection.Dispose();
        }

        private IDbConnection _connection = null!;
        private VehiculoDapperRepository _repository = null!;
        
        [Test]
        public void Create_CrearVehiculo_CreaCorrectamente() {
            var vehiculo = new Vehiculo {
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
            var vehiculo = new Vehiculo {
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
            var p = _repository.Create(new Vehiculo {
                Matricula = "4567BCH",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "41234571X",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            }).Value;
            _repository.Create(new Vehiculo {
                Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });
            _repository.Create(new Vehiculo {
                Matricula = "2345BCF",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });
            _repository.Create(new Vehiculo {
                Matricula = "3456BCG",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });

            _repository.Delete(p.Id);
            var res = _repository.GetAll(includeDeleted: false);

            res.Should().HaveCount(3);
            res.First().Matricula.Should().Be("1234BCD");
        }
        [Test]
        public void GetAll_ConPaginarion_DebeDevolverPaginado() {
            _repository.Create(new Vehiculo {
                Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });
            _repository.Create(new Vehiculo {
                Matricula = "2345BCF",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });
            _repository.Create(new Vehiculo {
                Matricula = "3456BCG",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });
            _repository.Create(new Vehiculo {
                Matricula = "4567BCH",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "41234571X",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });

            var res = _repository.GetAll(1, 2);

            res.Should().HaveCount(2);
        }
        [Test]
        public void Update_ConDatosValidos_ActualizaCorrectamente() {
            _repository.Create(new Vehiculo {
                Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            });
            var act = new Vehiculo {
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

            _repository.Create(new Vehiculo {
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
        public void DeleteAll_EliminaTodosLosVehihulos_DebeVaciarRepositorio() {
            var vehiculo = new Vehiculo {
                Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            };
            var vehiculo2 = new Vehiculo {
                Matricula = "2345BCF",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            };
            var vehiculo3 = new Vehiculo {
                Matricula = "3456BCG",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            };
            var vehiculo4 = new Vehiculo {
                Matricula = "4567BCH",
                Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
            };

            _repository.Create(vehiculo);
            _repository.Create(vehiculo2);
            _repository.Create(vehiculo3);
            var res = _repository.DeleteAll();

            res.Should().BeTrue();
            _repository.GetAll().Should().BeEmpty();
        }

        [Test]
        public void Constructor_ConSeedData_CargaDatosIniciales() {

            var res = _repository = new VehiculoDapperRepository(_connection, null, false, true);

            res.GetAll().Should().NotBeEmpty();
        }
        [Test]
        public void Delete_ConBoradoFisico_EliminaCorrectamente() {
            _repository = new VehiculoDapperRepository(_connection,null,true, false);
            var vehiculo = new Vehiculo {
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
            var vehiculo = new Vehiculo {
                Matricula = "1234BCD", Marca = "Test", Cilindrada = 1000, 
                TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z"
            };
            _repository.Create(vehiculo);

            var resultado = _repository.DeleteAll();

            resultado.Should().BeTrue();
            _repository.GetAll().Should().BeEmpty();
        }
    }
    [TestFixture]
    public class CasosNegativos() {
        [SetUp]
        public void SetUp() {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _repository = new VehiculoDapperRepository(_connection);
        }

        [TearDown]
        public void TearDown() {
            _connection.Close();
            _connection.Dispose();
        }

    private IDbConnection _connection = null!;
    private VehiculoDapperRepository _repository = null!;

            [Test]
            public void Create_CrearVehiculoConMatriculaExistente_DebeDevolverError() {
                var vehiculo = new Vehiculo {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                    DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var vehiculo2 = new Vehiculo {
                    Matricula = "1234BCD",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };

                _repository.Create(vehiculo);
                var res = _repository.Create(vehiculo2);

                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<VehiculoError.MatriculaInspeccionDuplicada>();
                res.Error.Message.Should()
                    .Contain(
                        "1234BCD");
            }

            
            [Test]
            public void Create_CrearVehiculoConDniConMaximoVehiculo_DebeDevolverError() {
                var vehiculo = new Vehiculo {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina,
                    DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var vehiculo2 = new Vehiculo {
                    Matricula = "2345BCF",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var vehiculo3 = new Vehiculo {
                    Matricula = "3456BCG",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var vehiculo4 = new Vehiculo {
                    Matricula = "4567BCH",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };

                _repository.Create(vehiculo);
                _repository.Create(vehiculo2);
                _repository.Create(vehiculo3);
                var res = _repository.Create(vehiculo4);

                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<VehiculoError.MaxVehiculosUsageDniError>();
                res.Error.Message.Should()
                    .Contain(
                        "El propietario con dni: 01234567L tiene 3 vehiculos para inspeccion para el mismo dia");
            }
              [Test]
            public void Update_VehiculoNoExiste_DevuelveError() {
                var vehiculo4 = new Vehiculo {
                    Matricula = "4567BCH",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var res = _repository.Update(2, vehiculo4);

                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<VehiculoError.NotFound>();
                (res.Error as VehiculoError.NotFound)?.Id.Should().Be("2");

            }
            [Test]
            public void Update_VehiculoMatriculaExistente_DevuelveError() {
                var vehiculo = new Vehiculo {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var v2 = new Vehiculo {
                    Matricula = "6789BCW", Marca = "Mazda 3", Cilindrada = 1800, TipoMotor = Motor.Hibrido, DniPropietario = "61234573V",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                _repository.Create(vehiculo);
                _repository.Create(v2);
                
                var res = _repository.Update(2, vehiculo);

                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<VehiculoError.MatriculaInspeccionDuplicada>();
                (res.Error as VehiculoError.MatriculaInspeccionDuplicada)?.matricula.Should().Be("1234BCD");

            }
            [Test]
            public void  Update_PropietarioConLimiteDeVehiculoSuperado_DebeDevolverError() {
                var v1 = new Vehiculo {
                    Matricula = "4567BCH",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "41234571X",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var v2 = new Vehiculo {
                    Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "41234571X",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var v3 = new Vehiculo {
                    Matricula = "2345BCF", Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "41234571X",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var v4 = new Vehiculo {
                    Matricula = "3456BDG",
                    Marca = "Fiat", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L",
                    IsDeleted = false, CreatedAt = new DateTime(2024, 01, 17), UpdatedAt = new DateTime(2024, 01, 17)
                };
                var actu = new Vehiculo {
                    DniPropietario = "41234571X",
                };
                _repository.Create(v1);
                _repository.Create(v2);
                _repository.Create(v3);
                _repository.Create(v4);

                var res = _repository.Update(4, actu);
                
                res.IsFailure.Should().BeTrue();
                res.Error.Should().BeOfType<VehiculoError.MaxVehiculosUsageDniError>();
                (res.Error as VehiculoError.MaxVehiculosUsageDniError)?.Dni.Should().Be("41234571X");
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
                res.Error.Should().BeOfType<VehiculoError.NotFound>();
            }
            [Test]
            public void GetById_SinVehiculoExistente_DevuelveError() {
                var resultado = _repository.GetById(1);

                resultado.Should().BeNull();
            }
            [Test]
            public void GetByMatricula_VehiculoEliminado_DevuelveNull() {
                var vehiculo = new Vehiculo {
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

            [Test]
            public void GetById_DevuelveNull() {
                var res = _repository.GetById(1);

                res.Should().BeNull();
            }
    }
}