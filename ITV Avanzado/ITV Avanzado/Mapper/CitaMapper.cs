    using System.Globalization;
    using GestionItv.Models;
    using ITV_Avanzado.Dto;
    using ITV_Avanzado.Entity;

    namespace ITV_Avanzado.Mapper;

    public static class CitaMapper {
        //Formato para fecha con hora para el CreateAt y UpdateAt
        private const string IsoFormat = "s";
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

         public static CitaDto ToDto(this Cita cita) {
            return new CitaDto(
                cita.Id,
                cita.Matricula,
                cita.Marca,
                cita.Cilindrada,
                cita.TipoMotor.ToString(),
                cita.DniPropietario,
                cita.FechaMatriculacion.ToString(IsoFormat, InvariantCulture),
                cita.FechaInspeccion.ToString(IsoFormat, InvariantCulture),
                cita.IsDeleted,
                cita.CreatedAt.ToString(IsoFormat, InvariantCulture),
                cita.UpdatedAt.ToString(IsoFormat, InvariantCulture)
            );
        }

        public static Cita ToModel(this CitaDto dto) {
            var dateTime = DateTime.Parse(dto.CreatedAt, InvariantCulture);
            
            return new Cita{
                Id = dto.Id,
                Matricula = dto.Matricula,
                Marca = dto.Marca,
                Cilindrada = dto.Cilindrada,
                TipoMotor = Enum.TryParse(dto.TipoMotor, out Motor tipo) ? tipo : Motor.Diesel,
                DniPropietario = dto.DniPropietario,
                FechaMatriculacion = DateTime.Parse(dto.FechaMatriculacion, InvariantCulture),
                FechaInspeccion = DateTime.Parse(dto.FechaInspeccion, InvariantCulture),
                IsDeleted = dto.IsDelete,
                CreatedAt = DateTime.Parse(dto.CreatedAt, InvariantCulture),
                UpdatedAt = DateTime.Parse(dto.UpdatedAt, InvariantCulture)
            };
        }

        public static Cita? ToModel(this CitaEntity? entity) {
            if (entity == null) return null;
            return new Cita {
                Id = entity.Id,
                Matricula = entity.Matricula,
                Marca = entity.Marca,
                Cilindrada = entity.Cilindrada,
                TipoMotor = (Motor)entity.Motor,
                DniPropietario = entity.Dni,
                FechaMatriculacion = entity.FechaMatriculacion,
                FechaInspeccion = entity.FechaInspeccion,
                IsDeleted = entity.IsDeleted,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static CitaEntity ToEntity(this Cita cita) {
            return new CitaEntity {
                Id = cita.Id,
                Matricula = cita.Matricula,
                Marca = cita.Marca,
                Cilindrada = cita.Cilindrada,
                Motor = (int)cita.TipoMotor,
                Dni = cita.DniPropietario,
                FechaMatriculacion = cita.FechaMatriculacion,
                FechaInspeccion = cita.FechaInspeccion,
                IsDeleted = cita.IsDeleted,
                CreatedAt = cita.CreatedAt,
                UpdatedAt = cita.UpdatedAt
            };
        }

        public static IEnumerable<Cita> ToModel(this IEnumerable<CitaEntity> entities) {
            return entities.Select(ToModel).OfType<Cita>();
        }
    }