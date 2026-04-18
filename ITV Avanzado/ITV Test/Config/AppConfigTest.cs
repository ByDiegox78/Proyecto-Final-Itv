using FluentAssertions;
using ITV_Avanzado.Config;

namespace ITV_Test.Config;

[TestFixture]
public class AppConfigTest {
    [TestFixture]
    public class Propiedades {
        [Test]
        public void Locale_DeberiaSerEspania() {
            var locale = AppConfig.Locale;

            locale.Should().NotBeNull();
            locale.Name.Should().Be("es-ES");
        }

        [Test]
        public void StorageType_RetornaTipoValido() {
            var tipo = AppConfig.StorageType;

            tipo.Should().NotBeNullOrEmpty();
            tipo.ToLower().Should().BeOneOf("json", "csv", "bin", "xml");
        }
        [Test]
        public void RepositoryType_RetornaTipoValido() {
            var tipo = AppConfig.RepositoryType;

            tipo.Should().NotBeNullOrEmpty();
            tipo.ToLower().Should().BeOneOf("json", "ado","dapper", "efcore", "bin", "memory");
        }
        [Test]
        public void ConnectionString_RetornarValorNoNulo() {
            var connStr = AppConfig.ConnectionString;

            connStr.Should().NotBeNullOrEmpty();
            connStr.Should().Contain("Data Source");
        }
        [Test]
        public void CacheSize_DeberiaSerMayorQueCero() {
            var size = AppConfig.CacheSize;

            size.Should().BeGreaterThan(0);
        }
        [Test]
        public void DropData_DebeDevolverBooleano() {
            var drop = AppConfig.DropData;

            (drop || !drop).Should().BeTrue();
        }
        [Test]
        public void SeedData_DebeRetornarTrue() {
            var seed = AppConfig.SeedData;

            seed.Should().BeTrue(); 
        }
    }
    [TestFixture]
    public class Directorios {
        [Test]
        public void DataFolder_DebeDevolverRutaValida() {
            var res = AppConfig.DataFolder;

            res.Should().NotBeNullOrEmpty();
            Path.IsPathRooted(res).Should().BeTrue();
        }

        [Test]
        public void BuckupDirectory_DeberiaDevolverRutaValida() {
            var res = AppConfig.BackupDirectory;

            res.Should().NotBeNullOrEmpty();
            Path.IsPathRooted(res).Should().BeTrue();
        }
    }

    [TestFixture]
    public class Formatos {
        [Test]
        public void VehiculosFile_DebeDevlverArchivoJson() {
            var file = AppConfig.VehiculoFile;

            file.Should().NotBeNullOrEmpty();
            file.Should().EndWith(".json");
        }
        [Test]
        public void BackupFormat_DebeDevolverFormatoValido() {
            var format = AppConfig.BackupFormat;

            format.Should().NotBeNullOrEmpty();
            format.Should().BeOneOf("json", "csv", "bin", "xml");
        }
        
    }
    
    
    
    
}