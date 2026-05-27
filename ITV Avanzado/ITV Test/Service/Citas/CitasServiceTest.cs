using GestionItv.Models;
using ITV_Avanzado.Cache;
using ITV_Avanzado.Error.Common;
using ITV_Avanzado.Repository.Common;
using ITV_Avanzado.Service.Citas;
using ITV_Avanzado.Validator.Common;
using Moq;
using CSharpFunctionalExtensions;
using FluentAssertions;
using ITV_Avanzado.Error.Citas;


namespace ITV_Test.Service.Citas;

[TestFixture]
public class CitasServiceTest {
    [SetUp]
   
    public void SetUp() {
        _repositoryMock = new Mock<ICitaRepository>();
        _valMock = new Mock<IValidator<Cita>>();
        _cacheMock = new Mock<ICache<int, Cita>>();
        
        _valMock.Setup(v => v.Validar(It.IsAny<Cita>()))
            .Returns((Cita p) => Result.Success<Cita, DomainError>(p));

        _service = new CitasService(_repositoryMock.Object, _valMock.Object, _cacheMock.Object);
    }
    
    private CitasService _service = null!;
    private Mock<ICitaRepository> _repositoryMock = null!;
    private Mock<IValidator<Cita>> _valMock = null!;
    private Mock<ICache<int, Cita>> _cacheMock = null!;

    [TestFixture]
    public class CasosPositivos : CitasServiceTest {
        [Test]
        public void GetAll_SinParametro_DevuelveTodo() {
            var hoy = DateTime.Today;
            var citas = new List<Cita> {
                new() {
                    Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
                    TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
                    FechaInspeccion = hoy.AddDays(5)
                },
                new() {
                    Matricula = "2345BCF", Marca = "Toyota", Cilindrada = 1800,
                    TipoMotor = Motor.Hibrido, DniPropietario = "87654321X",
                    FechaInspeccion = hoy.AddDays(10)
                }
            };
            _repositoryMock.Setup(r => r.GetAll(1, 10, true, null)).Returns(citas);
            
            var resultado = _service.GetAll().ToList();
            
            resultado.Should().HaveCount(2);
            _repositoryMock.Verify(r => r.GetAll(1, 10, true, null), Times.Once);
        }

        [Test]
        public void GetById_ConCache_RetornaDeLaCahche() {
            var hoy = DateTime.Today;
            var citas = new Cita {
                    Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
                    TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
                    FechaInspeccion = hoy.AddDays(5)
            };
            _cacheMock.Setup(c => c.Get(1)).Returns(citas);
            
            var res = _service.GetById(1);
            
            res.IsSuccess.Should().BeTrue();
            res.Value.Matricula.Should().Be("1234BCD");
            _cacheMock.Verify(c => c.Get(1), Times.Once);
            _repositoryMock.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
        }
        [Test]
        public void GetById_SinCache_RetornaDesdeElRepositorio() {
            var hoy = DateTime.Today;
            var citas = new Cita {
                Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
                TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
                FechaInspeccion = hoy.AddDays(5)
            };
            _cacheMock.Setup(c => c.Get(1)).Returns((Cita?)null);
            _repositoryMock.Setup(r => r.GetById(1)).Returns(citas);
            
            var res = _service.GetById(1);
            
            res.IsSuccess.Should().BeTrue();
            res.Value.Matricula.Should().Be("1234BCD");
            _cacheMock.Verify(c => c.Get(1), Times.Once);
            _cacheMock.Verify(c => c.Add(1, citas), Times.Once);
            _repositoryMock.Verify(r => r.GetById(1), Times.Once);
        }

        [Test]
        public void GetByMatricula_CinUnaOVariasCitas_DevuelveLasCitas() {
            var hoy = DateTime.Today;
            var citas = new List<Cita>() {
               new Cita {
                   Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
                   TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
                   FechaInspeccion = hoy.AddDays(5)
               },
               new Cita {
               Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
               TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
               FechaInspeccion = hoy.AddDays(6)
            },
            new Cita {
                Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
                TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
                FechaInspeccion = hoy.AddDays(7)
            } 
            };
            _repositoryMock.Setup(r => r.GetByMatricula("1234BCD")).Returns(citas);
            
            var res = _service.GetByMatricula("1234BCD");
            
            res.IsSuccess.Should().BeTrue();
            res.Value.Should().HaveCount(3);
            res.Value.All(v => v.Matricula == "1234BCD").Should().BeTrue();
            _repositoryMock.Verify(r => r.GetByMatricula("1234BCD"), Times.Once);
        }

        [Test]
        public void Save_ConCitaValida_DeberiaGuardar() {
            var cita = new Cita {
                Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
                TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
                FechaMatriculacion = DateTime.UtcNow.AddDays(-100),
                FechaInspeccion = DateTime.UtcNow.AddDays(5)
            };

            _valMock.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<Cita, DomainError>(cita));
            _repositoryMock.Setup(r => r.Create(It.IsAny<Cita>()))
                .Returns(Result.Success<Cita, DomainError>(cita));

            var res = _service.Save(cita);

            res.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(r => r.Create(It.IsAny<Cita>()), Times.Once);
            _cacheMock.Verify(c => c.Add(It.IsAny<int>(), It.IsAny<Cita>()), Times.Once);
        }

        [Test]
        public void Update_ConCitaExistentes_DebeActualizarYLimpiarCache() {
            var cita = new Cita {
                Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
                TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
                FechaMatriculacion = DateTime.UtcNow.AddDays(-100),
                FechaInspeccion = DateTime.UtcNow.AddDays(5)
            };
            var citaActualizada = new Cita {
                Matricula = "1234BCD", Marca = "Fiat", Cilindrada = 1200,
                TipoMotor = Motor.Diesel, DniPropietario = "12345678Z",
                FechaMatriculacion = DateTime.UtcNow.AddDays(-100),
                FechaInspeccion = DateTime.UtcNow.AddDays(5)
            };
            _repositoryMock.Setup(v => v.GetById(1)).Returns(cita);
            _repositoryMock.Setup(v => v.GetByMatricula(It.IsAny<string>())).Returns(Array.Empty<Cita>());
            _repositoryMock.Setup(v => v.GetAll(1, int.MaxValue, false, null)).Returns(Array.Empty<Cita>());
            _repositoryMock.Setup(v => v.Update(1, It.IsAny<Cita>()))
                .Returns(Result.Success<Cita, DomainError>(citaActualizada));
            _valMock.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<Cita, DomainError>(citaActualizada));

            var res = _service.Update(1, citaActualizada);

            res.IsSuccess.Should().BeTrue();
            _cacheMock.Verify(c => c.Remove(1), Times.Once);
            _repositoryMock.Verify(r => r.Update(1, It.IsAny<Cita>()), Times.Once);
        }

        [Test]
        public void Delete_ConCitaExixtente_DebeEliminarYLimpiarCache() {
            var hoy = DateTime.Today;
            var citas = new Cita {
                Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
                TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
                FechaInspeccion = hoy.AddDays(5)
            };
            _repositoryMock.Setup(r => r.GetById(1)).Returns(citas);
            _repositoryMock.Setup(r => r.Delete(1, true)).Returns(citas);
            
            var resultado = _service.Delete(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _cacheMock.Verify(c => c.Remove(1), Times.Once);
            _repositoryMock.Verify(r => r.Delete(1, true), Times.Once);
            _repositoryMock.Verify(r => r.GetById(1), Times.Once);
        }
        
        
        [Test]
        public void DeleteAll_DeberiaLlamarRepository() {
            _repositoryMock.Setup(r => r.DeleteAll()).Returns(true);

            var res = _service.DeleteAll();
            
            res.Should().BeTrue();
            _repositoryMock.Verify(r => r.DeleteAll(), Times.Once);
        }
        [Test]
        public void GetCitasOrderBy_RetornarCitasOrdenadas() {
            // Arrange
            var listaDesordenada = new List<Cita> {
                new() { Id = 1, Matricula = "1234FGH" },
                new() { Id = 2, Matricula = "1234RTY" }
            };
            _repositoryMock.Setup(repo => repo.GetAll(1, int.MaxValue, true, null))
                .Returns(listaDesordenada);
            
            var resultado = _service.GetCitasOrderBy(TipoOrdenamiento.Matricula, 1, 10, true).ToList();

            resultado.Should().HaveCount(2);
            resultado.First().Matricula.Should().Be("1234FGH");
            resultado.Last().Matricula.Should().Be("1234RTY");

            _repositoryMock.Verify(repo => repo.GetAll(1, int.MaxValue, true, null), Times.Once);
        }
        [Test]
        public void GetCitasOrderBy_DeberiaCubrirTodosLosCasosDelSwitch() {
            var citas = new List<Cita> {
                new() {
                    Id = 10, Matricula = "B", DniPropietario = "2", Marca = "Z", Cilindrada = 2000,
                    FechaInspeccion = DateTime.Now.AddDays(1)
                },
                new() {
                    Id = 1, Matricula = "A", DniPropietario = "1", Marca = "A", Cilindrada = 1000,
                    FechaInspeccion = DateTime.Now
                }
            };

            _repositoryMock.Setup(r => r.GetAll(1, int.MaxValue, true, null)).Returns(citas);

            _service.GetCitasOrderBy(TipoOrdenamiento.Matricula).First().Id.Should().Be(1);
            _service.GetCitasOrderBy(TipoOrdenamiento.Dni).First().Id.Should().Be(1);
            _service.GetCitasOrderBy(TipoOrdenamiento.Marca).First().Marca.Should().Be("A");
            _service.GetCitasOrderBy(TipoOrdenamiento.FechaItv).First().Id.Should().Be(1);
            _service.GetCitasOrderBy(TipoOrdenamiento.Cilindrada).First().Cilindrada.Should().Be(1000);
            _service.GetCitasOrderBy((TipoOrdenamiento)999).First().Id.Should().Be(1);

            _repositoryMock.Verify(r => r.GetAll(1, int.MaxValue, true, null), Times.Exactly(6));
        }
        
    }

    [TestFixture]
    public class CasosNegativos : CitasServiceTest {
        [Test]
        public void GetById_ConCitaInexistente_DeberiaDevolverError() {
            _cacheMock.Setup(c => c.Get(1)).Returns((Cita?)null);
            _repositoryMock.Setup(r => r.GetById(1)).Returns((Cita?)null);

            var res = _service.GetById(1);

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<CitaError.NotFound>();
            res.Error.Message.Should().Contain("1");
            _cacheMock.Verify(c => c.Get(1), Times.Once);
            _repositoryMock.Verify(r => r.GetById(1), Times.Once);
        }

        [Test]
        public void GetByMatricula_ConCitasInexistentes_DevuelveError() {
            _repositoryMock.Setup(r => r.GetByMatricula("1234BVC")).Returns((IEnumerable<Cita>?)null);
            var res = _service.GetByMatricula("1234BVC");
            
            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<CitaError.NotFound>();
            res.Error.Message.Should().Contain("1234BVC");
            _repositoryMock.Verify(r => r.GetByMatricula("1234BVC"), Times.Once);
        }

        [Test]
        public void Save_ConCitaReservada_DeveriaDevolverError() {
            var hoy = DateTime.Today;
            var citas = new Cita {
                Matricula = "1234BCD", Marca = "Seat", Cilindrada = 1200,
                TipoMotor = Motor.Gasolina, DniPropietario = "12345678Z",
                FechaInspeccion = hoy.AddDays(5)
            };
            _valMock.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns((Cita v) => Result.Success<Cita, DomainError>(v));
            
            _repositoryMock.Setup(r => r.Create(It.IsAny<Cita>()))
                .Returns(Result.Failure<Cita, DomainError>(CitaErrors.MatriculaInspeccionDuplicada("1234BCD")));
            
            var res = _service.Save(citas);
            
            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<CitaError.MatriculaInspeccionDuplicada>();
            res.Error.Message.Should().Contain("1234BCD");
            _repositoryMock.Verify(r => r.Create(It.IsAny<Cita>()), Times.Once);
        }
        [Test]
        public void Save_ConDniMaxDiaCitas_DeveriaDevolverError() {
            var cita = new Cita {
                Matricula = "1234BCD", DniPropietario = "12345678Z",
                FechaInspeccion = DateTime.Today.AddDays(5)
            };
            var citasExistentes = new List<Cita> {
                new() { DniPropietario = "12345678Z", FechaInspeccion = DateTime.Today.AddDays(5) },
                new() { DniPropietario = "12345678Z", FechaInspeccion = DateTime.Today.AddDays(5) },
                new() { DniPropietario = "12345678Z", FechaInspeccion = DateTime.Today.AddDays(5) },
            };

            _valMock.Setup(v => v.Validar(It.IsAny<Cita>())).Returns(Result.Success<Cita, DomainError>(cita));
            _repositoryMock.Setup(r => r.Create(It.IsAny<Cita>()))
                .Returns(Result.Failure<Cita, DomainError>(CitaErrors.MaxCitasUsageDniError("12345678Z")));

            var res = _service.Save(cita);

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<CitaError.MaxCitasUsageDniError>();
            res.Error.Message.Should().Contain("12345678Z");
            _repositoryMock.Verify(r => r.Create(It.IsAny<Cita>()), Times.Once);
        }

        [Test]
        public void Save_ConFalloDeValidacion() {
            var cita = new Cita {
                Matricula = "1234AAA", DniPropietario = "12345678Z",
                FechaInspeccion = DateTime.Today.AddDays(5)
            };
            _valMock.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Failure<Cita, DomainError>(CitaErrors.Validation(new List<string> { "error test" })));
            
            var res = _service.Save(cita);

            res.IsFailure.Should().BeTrue();
        }

        [Test]
        public void Update_ConCitaMismoDiaYMismaMatricula_DevuelveError() {
            _repositoryMock.Setup(r => r.GetById(999)).Returns((Cita?)null);
            
            var res = _service.Update(999, new Cita { Matricula = "1234FGH", FechaInspeccion = DateTime.Today.AddDays(5)});

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<CitaError.NotFound>();
            res.Error.Message.Should().Contain("999");
            _repositoryMock.Verify(r => r.GetById(999), Times.Once);
            _repositoryMock.Verify(r => r.Update(It.IsAny<int>(), It.IsAny<Cita>()), Times.Never);
        }
        [Test]
        public void Update_ConDniMaxDiaCitas_DeveriaDevolverError() {
            var citaExistente = new Cita {
                Id = 1, DniPropietario = "12345678Z", Matricula = "1234BCD",
                FechaInspeccion = DateTime.Today.AddDays(5)
            };
            var citasExistentes = new List<Cita> {
                new() { DniPropietario = "12345678Z", FechaInspeccion = DateTime.Today.AddDays(5) },
                new() { DniPropietario = "12345678Z", FechaInspeccion = DateTime.Today.AddDays(5) },
                new() { DniPropietario = "12345678Z", FechaInspeccion = DateTime.Today.AddDays(5) },
            };

            _repositoryMock.Setup(r => r.GetById(1)).Returns(citaExistente);
            _valMock.Setup(v => v.Validar(It.IsAny<Cita>()))
                .Returns(Result.Success<Cita, DomainError>(new Cita()));
            _repositoryMock.Setup(r => r.Update(1, It.IsAny<Cita>()))
                .Returns(Result.Failure<Cita, DomainError>(CitaErrors.MaxCitasUsageDniError("12345678Z")));


            var res = _service.Update(1, new Cita { DniPropietario = "12345678Z", FechaInspeccion = DateTime.Today.AddDays(5) });

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<CitaError.MaxCitasUsageDniError>();
            res.Error.Message.Should().Contain("12345678Z");
            _repositoryMock.Verify(r => r.Update(It.IsAny<int>(), It.IsAny<Cita>()), Times.Once);
        }
        
        [Test]
        public void Restore_ConCitaNoExistente_DeberiaRetornarError() {
            _repositoryMock.Setup(r => r.Restore(999))
                .Returns(Result.Failure<Cita, DomainError>(CitaErrors.NotFound("999")));

            var res = _service.Restore(999);

            res.IsFailure.Should().BeTrue();
            res.Error.Should().BeOfType<CitaError.NotFound>();
        }
        [Test]
        public void Restore_ConCitaExistenteEliminada_Restaurar() {
            var c = new Cita { Id = 1, Matricula = "1234BBB", IsDeleted = true };
            _repositoryMock.Setup(v => v.Restore(1))
                .Returns(Result.Success<Cita, DomainError>(new Cita
                    { Id = 1, Matricula = "1234-BBB", IsDeleted = false }));
            
            var r = _service.Restore(1);

            r.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(v => v.Restore(1), Times.Once);
        }
    }
}

