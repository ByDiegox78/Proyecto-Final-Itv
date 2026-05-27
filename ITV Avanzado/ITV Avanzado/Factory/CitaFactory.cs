using GestionItv.Models;

namespace ITV_Avanzado.Factory;
/// <summary>
///     Factoría con datos semilla fijos para registros inmutables de citas.
/// </summary>
public static class CitasFactory {
    /// <summary>
    ///     Genera la semilla de datos inicial.
    /// </summary>
    /// <returns>Enumerable con datos de demostración</returns>
    public static IEnumerable<Cita> Seed() {
        var now = DateTime.Now;
        var today = now.Date;
        
        return new List<Cita> {
            new Cita { Id = 0, Matricula = "4196FMR", Marca = "Alfa Romeo", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "54407737H", 
                FechaMatriculacion = today.AddYears(-5), FechaInspeccion = today, IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Cita { Id = 0, Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L", 
                FechaMatriculacion = today.AddYears(-5), FechaInspeccion = today, IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Cita { Id = 0, Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L", 
                FechaMatriculacion = today.AddYears(-15), FechaInspeccion = today.AddDays(1), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "2345BCF", Marca = "Volkswagen Golf", Cilindrada = 1600, TipoMotor = Motor.Diesel, DniPropietario = "12345678Z", 
                FechaMatriculacion = today.AddYears(-3), FechaInspeccion = today.AddDays(5), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "3456BCG", Marca = "Toyota Corolla", Cilindrada = 1800, TipoMotor = Motor.Hibrido, DniPropietario = "12345678Z", 
                FechaMatriculacion = today.AddYears(-2), FechaInspeccion = today.AddDays(15), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "4567BCH", Marca = "Tesla Model 3", Cilindrada = 0, TipoMotor = Motor.Electrico, DniPropietario = "12345678Z", 
                FechaMatriculacion = today.AddYears(-1), FechaInspeccion = today.AddDays(20), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "5678BCJ", Marca = "Ford Focus", Cilindrada = 1500, TipoMotor = Motor.Gasolina, DniPropietario = "56789012B", 
                FechaMatriculacion = today.AddYears(-4), FechaInspeccion = today.AddDays(25), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "6789BCK", Marca = "BMW Serie 3", Cilindrada = 2000, TipoMotor = Motor.Diesel, DniPropietario = "67890123B", 
                FechaMatriculacion = today.AddYears(-6), FechaInspeccion = today.AddDays(30), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "7890BCL", Marca = "Audi A4", Cilindrada = 2000, TipoMotor = Motor.Hibrido, DniPropietario = "78901234X", 
                FechaMatriculacion = today.AddYears(-8), FechaInspeccion = today.AddDays(7), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "8901BCM", Marca = "Mercedes C", Cilindrada = 2200, TipoMotor = Motor.Gasolina, DniPropietario = "89012345E", 
                FechaMatriculacion = today.AddYears(-10), FechaInspeccion = today.AddDays(12), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "9012BCN", Marca = "Nissan Leaf", Cilindrada = 0, TipoMotor = Motor.Electrico, DniPropietario = "90123456A", 
                FechaMatriculacion = today.AddYears(-7), FechaInspeccion = today.AddDays(18), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "0123BCP", Marca = "Kia Ceed", Cilindrada = 1400, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L", 
                FechaMatriculacion = today.AddYears(-5), FechaInspeccion = today.AddDays(22), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "1234BCR", Marca = "Hyundai i30", Cilindrada = 1600, TipoMotor = Motor.Diesel, DniPropietario = "11234568B", 
                FechaMatriculacion = today.AddYears(-4), FechaInspeccion = today.AddDays(28), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "2345BCR", Marca = "Renault Clio", Cilindrada = 1200, TipoMotor = Motor.Hibrido, DniPropietario = "21234569A", 
                FechaMatriculacion = today.AddYears(-3), FechaInspeccion = today.AddDays(3), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "3456BCS", Marca = "Peugeot 208", Cilindrada = 1300, TipoMotor = Motor.Gasolina, DniPropietario = "31234570H", 
                FechaMatriculacion = today.AddYears(-2), FechaInspeccion = today.AddDays(14), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "4567BCT", Marca = "Citroen C3", Cilindrada = 1400, TipoMotor = Motor.Diesel, DniPropietario = "41234571X", 
                FechaMatriculacion = today.AddYears(-6), FechaInspeccion = today.AddDays(17), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "5678BCV", Marca = "Opel Astra", Cilindrada = 1600, TipoMotor = Motor.Gasolina, DniPropietario = "51234572W", 
                FechaMatriculacion = today.AddYears(-5), FechaInspeccion = today.AddDays(21), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "6789BCW", Marca = "Mazda 3", Cilindrada = 1800, TipoMotor = Motor.Hibrido, DniPropietario = "61234573V", 
                FechaMatriculacion = today.AddYears(-4), FechaInspeccion = today.AddDays(26), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "7890BCX", Marca = "Honda Civic", Cilindrada = 2000, TipoMotor = Motor.Gasolina, DniPropietario = "71234574D", 
                FechaMatriculacion = today.AddYears(-7), FechaInspeccion = today.AddDays(9), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "8901BCY", Marca = "Suzuki Swift", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "81234575R", 
                FechaMatriculacion = today.AddYears(-3), FechaInspeccion = today.AddDays(13), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "9012BCZ", Marca = "Ford Fiesta", Cilindrada = 1400, TipoMotor = Motor.Diesel, DniPropietario = "91234576Q", 
                FechaMatriculacion = today.AddYears(-2), FechaInspeccion = today.AddDays(19), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "0123BDC", Marca = "Volkswagen Polo", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "10234567G", 
                FechaMatriculacion = today.AddYears(-5), FechaInspeccion = today.AddDays(23), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "1234BDD", Marca = "Toyota Yaris", Cilindrada = 1000, TipoMotor = Motor.Hibrido, DniPropietario = "20234568L", 
                FechaMatriculacion = today.AddYears(-4), FechaInspeccion = today.AddDays(29), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "2345BDF", Marca = "Tesla Model Y", Cilindrada = 0, TipoMotor = Motor.Electrico, DniPropietario = "30234569B", 
                FechaMatriculacion = today.AddYears(-2), FechaInspeccion = today.AddDays(2), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "3456BDG", Marca = "BMW X1", Cilindrada = 2000, TipoMotor = Motor.Diesel, DniPropietario = "40234570A", 
                FechaMatriculacion = today.AddYears(-8), FechaInspeccion = today.AddDays(11), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "4567BDH", Marca = "Audi Q3", Cilindrada = 1800, TipoMotor = Motor.Gasolina, DniPropietario = "50234571H", 
                FechaMatriculacion = today.AddYears(-6), FechaInspeccion = today.AddDays(16), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "5678BDJ", Marca = "Mercedes GLA", Cilindrada = 2000, TipoMotor = Motor.Hibrido, DniPropietario = "60234572X", 
                FechaMatriculacion = today.AddYears(-7), FechaInspeccion = today.AddDays(24), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "6789BDK", Marca = "Kia Sportage", Cilindrada = 1600, TipoMotor = Motor.Diesel, DniPropietario = "70234573W", 
                FechaMatriculacion = today.AddYears(-5), FechaInspeccion = today.AddDays(27), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "7890BDL", Marca = "Hyundai Tucson", Cilindrada = 1800, TipoMotor = Motor.Gasolina, DniPropietario = "80234574V", 
                FechaMatriculacion = today.AddYears(-4), FechaInspeccion = today.AddDays(1), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "8901BDM", Marca = "Renault Captur", Cilindrada = 1400, TipoMotor = Motor.Hibrido, DniPropietario = "90234575D", 
                FechaMatriculacion = today.AddYears(-3), FechaInspeccion = today.AddDays(8), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "9012BDN", Marca = "Peugeot 3008", Cilindrada = 2000, TipoMotor = Motor.Diesel, DniPropietario = "01234576M", 
                FechaMatriculacion = today.AddYears(-9), FechaInspeccion = today.AddDays(6), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            
            new Cita { Id = 0, Matricula = "0123BDP", Marca = "Nissan Qashqai", Cilindrada = 1600, TipoMotor = Motor.Gasolina, DniPropietario = "11234577C", 
                FechaMatriculacion = today.AddYears(-5), FechaInspeccion = today.AddDays(4), IsDeleted = false, CreatedAt = now, UpdatedAt = now }
        };
    }
}