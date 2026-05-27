using GestionItv.Models;
using ITV_Avanzado.Front.Cita;
using ITV_Avanzado.Front.ViewModels.Form;

namespace ITV_Avanzado.Front.Mapper;

public static class CitaMapper {
    public static CitaFormData ToFromData(this GestionItv.Models.Cita model) {
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

    public static GestionItv.Models.Cita ToModel(this CitaFormData formData) {
        return new GestionItv.Models.Cita {
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
    public static CitaItemViewModel ToItemViewModel(this GestionItv.Models.Cita model) {
        return new CitaItemViewModel
        {
            Id = model.Id,
            Matricula = model.Matricula,
            DniPropietario = model.DniPropietario,
            Marca = model.Marca,
            FechaMatriculacion = model.FechaMatriculacion,
            FechaInspeccion = model.FechaInspeccion,
            TipoMotor = model.TipoMotor,
            Cilindrada = model.Cilindrada,
            IsDeleted = model.IsDeleted
        };
    }

    public static void UpdateFromFormData(this CitaItemViewModel item, CitaFormData form) {
        item.Matricula = form.Matricula;
        item.DniPropietario = form.DniPropietario;
        item.Marca = form.Marca;
        item.FechaInspeccion = form.FechaInspeccion;
        item.FechaMatriculacion = form.FechaMatriculacion;
        item.TipoMotor = form.Motor;
        item.IsDeleted = form.IsDeleted;
    }

    // --- De Item de Lista a Modelo de Dominio ---
    public static GestionItv.Models.Cita ToModel(this CitaItemViewModel item) {
        return new GestionItv.Models.Cita
        {
            Id = item.Id,
            Matricula = item.Matricula,
            DniPropietario = item.DniPropietario,
            Marca = item.Marca,
            Cilindrada = item.Cilindrada,
            FechaInspeccion = item.FechaInspeccion,
            FechaMatriculacion = item.FechaMatriculacion,
            TipoMotor = item.TipoMotor,
            IsDeleted = item.IsDeleted
        };
    }
}