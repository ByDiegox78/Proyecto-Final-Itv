using GestionItv.Models;
using ITV_Avanzado.Front.ViewModels.Form;

namespace ITV_Avanzado.Front.Mapper;

public static class CitaMapper {
    public static CitaFormData ToFromData(this Vehiculo model) {
        return new CitaFormData {
            Id = model.Id,
            Matricula = model.Matricula,
            Marca = model.Marca,
            Cilindrada = model.Cilindrada,
            Motor = model.TipoMotor,
            DniPropietario = model.DniPropietario,
            FechaMatriculacion = model.FechaMatriculacion,
            FechaInspeccion = model.FechaInspeccion,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted
        };
    }

    public static Vehiculo ToModel(this CitaFormData formData) {
        return new Vehiculo {
            Id = formData.Id,
            Matricula = formData.Matricula,
            Marca = formData.Marca,
            Cilindrada = formData.Cilindrada,
            TipoMotor = formData.Motor,
            DniPropietario = formData.DniPropietario,
            FechaMatriculacion = formData.FechaMatriculacion,
            FechaInspeccion = formData.FechaInspeccion,
            CreatedAt = formData.CreatedAt,
            UpdatedAt = formData.UpdatedAt,
            IsDeleted = formData.IsDeleted
        };
    }
}