using GestionItv.Models;

namespace ITV_Avanzado.Factory;

public static class VehiculosFactory {
    public static IEnumerable<Vehiculo> Seed() {
        var now = DateTime.Now;
        return new List<Vehiculo> {
            new Vehiculo { Id = 0, Matricula = "1234BCD", Marca = "Seat Ibiza", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "2345BCF", Marca = "Volkswagen Golf", Cilindrada = 1600, TipoMotor = Motor.Diesel, DniPropietario = "12345678Z", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "3456BCG", Marca = "Toyota Corolla", Cilindrada = 1800, TipoMotor = Motor.Hibrido, DniPropietario = "12345678Z", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "4567BCH", Marca = "Tesla Model 3", Cilindrada = 0, TipoMotor = Motor.Electrico, DniPropietario = "12345678Z", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "5678BCJ", Marca = "Ford Focus", Cilindrada = 1500, TipoMotor = Motor.Gasolina, DniPropietario = "56789012B", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "6789BCK", Marca = "BMW Serie 3", Cilindrada = 2000, TipoMotor = Motor.Diesel, DniPropietario = "67890123B", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "7890BCL", Marca = "Audi A4", Cilindrada = 2000, TipoMotor = Motor.Hibrido, DniPropietario = "78901234X", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "8901BCM", Marca = "Mercedes C", Cilindrada = 2200, TipoMotor = Motor.Gasolina, DniPropietario = "89012345E", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "9012BCN", Marca = "Nissan Leaf", Cilindrada = 0, TipoMotor = Motor.Electrico, DniPropietario = "90123456A", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "0123BCP", Marca = "Kia Ceed", Cilindrada = 1400, TipoMotor = Motor.Gasolina, DniPropietario = "01234567L", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "1234BCR", Marca = "Hyundai i30", Cilindrada = 1600, TipoMotor = Motor.Diesel, DniPropietario = "11234568B", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "2345BCR", Marca = "Renault Clio", Cilindrada = 1200, TipoMotor = Motor.Hibrido, DniPropietario = "21234569A", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "3456BCS", Marca = "Peugeot 208", Cilindrada = 1300, TipoMotor = Motor.Gasolina, DniPropietario = "31234570H", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "4567BCT", Marca = "Citroen C3", Cilindrada = 1400, TipoMotor = Motor.Diesel, DniPropietario = "41234571X", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "5678BCV", Marca = "Opel Astra", Cilindrada = 1600, TipoMotor = Motor.Gasolina, DniPropietario = "51234572W", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "6789BCW", Marca = "Mazda 3", Cilindrada = 1800, TipoMotor = Motor.Hibrido, DniPropietario = "61234573V", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "7890BCX", Marca = "Honda Civic", Cilindrada = 2000, TipoMotor = Motor.Gasolina, DniPropietario = "71234574D", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "8901BCY", Marca = "Suzuki Swift", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "81234575R", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "9012BCZ", Marca = "Ford Fiesta", Cilindrada = 1400, TipoMotor = Motor.Diesel, DniPropietario = "91234576Q", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "0123BDC", Marca = "Volkswagen Polo", Cilindrada = 1200, TipoMotor = Motor.Gasolina, DniPropietario = "10234567G", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "1234BDD", Marca = "Toyota Yaris", Cilindrada = 1000, TipoMotor = Motor.Hibrido, DniPropietario = "20234568L", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "2345BDF", Marca = "Tesla Model Y", Cilindrada = 0, TipoMotor = Motor.Electrico, DniPropietario = "30234569B", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "3456BDG", Marca = "BMW X1", Cilindrada = 2000, TipoMotor = Motor.Diesel, DniPropietario = "40234570A", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "4567BDH", Marca = "Audi Q3", Cilindrada = 1800, TipoMotor = Motor.Gasolina, DniPropietario = "50234571H", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "5678BDJ", Marca = "Mercedes GLA", Cilindrada = 2000, TipoMotor = Motor.Hibrido, DniPropietario = "60234572X", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "6789BDK", Marca = "Kia Sportage", Cilindrada = 1600, TipoMotor = Motor.Diesel, DniPropietario = "70234573W", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "7890BDL", Marca = "Hyundai Tucson", Cilindrada = 1800, TipoMotor = Motor.Gasolina, DniPropietario = "80234574V", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "8901BDM", Marca = "Renault Captur", Cilindrada = 1400, TipoMotor = Motor.Hibrido, DniPropietario = "90234575D", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "9012BDN", Marca = "Peugeot 3008", Cilindrada = 2000, TipoMotor = Motor.Diesel, DniPropietario = "01234576M", IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new Vehiculo { Id = 0, Matricula = "0123BDP", Marca = "Nissan Qashqai", Cilindrada = 1600, TipoMotor = Motor.Gasolina, DniPropietario = "11234577C", IsDeleted = false, CreatedAt = now, UpdatedAt = now }
        };
    }
}