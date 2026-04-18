    using System.Globalization;
    using GestionItv.Models;
    using ITV_Avanzado.Dto;
    using ITV_Avanzado.Entity;

    namespace ITV_Avanzado.Mapper;

    public static class VehiculoMapper {
        //Formato para fecha con hora para el CreateAt y UpdateAt
        private const string IsoFormat = "s";
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

         public static VehiculoDto ToDto(this Vehiculo vehiculo) {
            return new VehiculoDto(
                vehiculo.Id,
                vehiculo.Matricula,
                vehiculo.Marca,
                vehiculo.Cilindrada,
                vehiculo.TipoMotor.ToString(),
                vehiculo.DniPropietario,
                vehiculo.IsDeleted,
                vehiculo.CreatedAt.ToString(IsoFormat, InvariantCulture),
                vehiculo.UpdatedAt.ToString(IsoFormat, InvariantCulture)
            );
        }

        public static Vehiculo ToModel(this VehiculoDto dto) {
            var createdAt = DateTime.Parse(dto.CreatedAt, InvariantCulture);
            var updatedAt = DateTime.Parse(dto.UpdatedAt, InvariantCulture);
            
            return new Vehiculo{
                Id = dto.Id,
                Matricula = dto.Matricula,
                Marca = dto.Marca,
                Cilindrada = dto.Cilindrada,
                TipoMotor = Enum.TryParse(dto.TipoMotor, out Motor tipo) ? tipo : Motor.Diesel,
                DniPropietario = dto.DniPropietario,
                IsDeleted = dto.IsDelete,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
        }

        public static Vehiculo? ToModel(this VehiculoEntity? entity) {
            if (entity == null) return null;
            return new Vehiculo {
                Id = entity.Id,
                Matricula = entity.Matricula,
                Marca = entity.Marca,
                Cilindrada = entity.Cilindrada,
                TipoMotor = (Motor)entity.Motor,
                DniPropietario = entity.Dni,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static VehiculoEntity ToEntity(this Vehiculo vehiculo) {
            return new VehiculoEntity {
                Id = vehiculo.Id,
                Matricula = vehiculo.Matricula,
                Marca = vehiculo.Marca,
                Cilindrada = vehiculo.Cilindrada,
                Motor = (int)vehiculo.TipoMotor,
                Dni = vehiculo.DniPropietario,
                IsDeleted = vehiculo.IsDeleted,
                CreatedAt = vehiculo.CreatedAt,
                UpdatedAt = vehiculo.UpdatedAt
            };
        }

        public static IEnumerable<Vehiculo> ToModel(this IEnumerable<VehiculoEntity> entities) {
            return entities.Select(ToModel).OfType<Vehiculo>();
        }
    }