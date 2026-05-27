using System.Runtime.InteropServices.JavaScript;
using FluentAssertions;
using GestionItv.Models;
using ITV_Avanzado.Dto;
using ITV_Avanzado.Entity;
using ITV_Avanzado.Mapper;

namespace ITV_Test.Mapper;

[TestFixture]
public static class CitaMapperTest {
    [TestFixture]
    public class CasosPositivos {
        [SetUp]
        public void SetUp() {
            _cita = new Cita {
                Id = 1, 
                Matricula = "1234BCD", 
                Marca = "Seat Ibiza", 
                Cilindrada = 1200, 
                TipoMotor = Motor.Gasolina, 
                DniPropietario = "01234567L", 
                IsDeleted = false, 
                CreatedAt = new DateTime(2024, 01, 17), 
                UpdatedAt = new DateTime(2024, 01, 17)
            };
            _citaDto = new CitaDto(
                1,
                "1234BCD",
                "Seat Ibiza",
                1200,
                "Gasolina",
                "01234567L",
                "2024-01-17T00:00:00",
                "2024-01-17T00:00:00",
                false,
                "2024-01-17T00:00:00",
                "2024-01-17T00:00:00"
            );
            _citaEntity = new CitaEntity {
                Id = 1,
                Matricula = "1234BCD",
                Marca = "Seat Ibiza",
                Cilindrada = 1200,
                Motor = 0,
                Dni = "01234567L",
                IsDeleted = false,
                CreatedAt = new DateTime(2024, 01, 17, 0, 0, 0),
                UpdatedAt = new DateTime(2024, 01, 17, 0, 0, 0)
            };
        }

        private Cita _cita = null!;
        private CitaDto _citaDto = null!;
        private CitaEntity _citaEntity = null!;

        [Test]
        public void ToModel_VehiculoDto_Correcto() {
            var res = _citaDto.ToModel();
            res.Should().NotBeNull();
            res.Id.Should().Be(1);
            res.Matricula.Should().Be("1234BCD");
            res.Marca.Should().Be("Seat Ibiza");
            res.Cilindrada.Should().Be(1200);
            res.TipoMotor.Should().Be(Motor.Gasolina);
            res.DniPropietario.Should().Be("01234567L");
        }

        [Test]
        public void ToDto_Vehiculo_Correcto() {
            var res = _cita.ToDto();
            res.Should().NotBeNull();
            res.Id.Should().Be(1);
            res.Matricula.Should().Be("1234BCD");
            res.Marca.Should().Be("Seat Ibiza");
            res.Cilindrada.Should().Be(1200);
            res.TipoMotor.Should().Be("Gasolina");
            res.DniPropietario.Should().Be("01234567L");
        }

        [Test]
        public void ToModel_VehiculoEntity_Correcto() {
            var res = _citaEntity.ToModel();

            res.Should().NotBeNull();
            res!.Id.Should().Be(1);
            res.Matricula.Should().Be("1234BCD");
            res.Marca.Should().Be("Seat Ibiza");
            res.Cilindrada.Should().Be(1200);
            res.TipoMotor.Should().Be(Motor.Gasolina);
            res.DniPropietario.Should().Be("01234567L");
        }

        [Test]
        public void ToEntity_Vehiculo_Correcto() {
            var res = _cita.ToEntity();
            
            res.Should().NotBeNull();
            res.Id.Should().Be(1);
            res.Matricula.Should().Be("1234BCD");
            res.Marca.Should().Be("Seat Ibiza");
            res.Cilindrada.Should().Be(1200);
            res.Motor.Should().Be(0);
            res.Dni.Should().Be("01234567L");

        }
        [Test]
        public void ToModel_ListaEntity_DevuelveTodos() {
            var entities = new List<CitaEntity> { _citaEntity };

            var res = entities.ToModel();

            res.Should().HaveCount(1);
        }
    }

    [TestFixture]
    public class CasosInvalidos {
        [Test]
        public void ToModel_VehiculoDto_UsaValorPorDefectoConEntradaInvalida() {
            var dto = new CitaDto(
                1,
                "1234BCD",
                "Seat Ibiza",
                1200,
                "Hidrogeno",
                "01234567L",
                "2024-01-17T00:00:00",
                "2024-01-17T00:00:00",
                false,
                "2024-01-17T00:00:00",
                "2024-01-17T00:00:00"
            );

            var res = dto.ToModel();

            res.Should().NotBeNull();
            res.TipoMotor.Should().Be(Motor.Diesel);
        }
        [Test]
        public void ToModel_VehiculoEntity_EsNullDevuelveDull() {
            CitaEntity? entity = null;

            var res = entity.ToModel();
        
            res.Should().BeNull();
        }
        
    }

    
}