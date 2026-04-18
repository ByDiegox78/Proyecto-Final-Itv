using FluentAssertions;
using ITV_Avanzado.Cache;

namespace ITV_Test.Cache;

[TestFixture]
public class CacheLruTest {
    [TestFixture]
    public class CasosPositivos {
        [SetUp]
        public void SetUp() {
            _cache = new CacheLru<int, string>(2);
        }
        private CacheLru<int, string> _cache = null!;

        [Test]
        public void Add_ElementoValido_GuardaEnCache() {
            _cache.Add(1, "Seat");

            _cache.Get(1).Should().Be("Seat");
        }

        [Test]
        public void Add_ConVariosElementos_GuardaTodos() {
            _cache.Add(1, "Seat");
            _cache.Add(2, "Fiat");
            _cache.Add(3, "Alfa");

            _cache.Get(1).Should().BeNull();
            _cache.Get(2).Should().Be("Fiat");
            _cache.Get(3).Should().Be("Alfa");
        }

        [Test]
        public void Remove_SiExiste_EliminaElemento() {
            _cache.Add(1, "Seat");
            _cache.Add(2, "Fiat");

            var res = _cache.Remove(1);

            res.Should().BeTrue();
            _cache.Get(1).Should().BeNull();
            _cache.Get(2).Should().Be("Fiat");
        }

        [Test]
        public void Remove_SiNoExiste_DevuelveFalse() {
            var res = _cache.Remove(1);
            res.Should().BeFalse();
        }

        [Test]
        public void Get_SiNoExiste_DevuelveFalse() {
            var res = _cache.Get(1);
            res.Should().BeNull();
        }

        [Test]
        public void Add_ClaveExistente_DebeActualizarValor() {
            _cache.Add(1, "Fiat");
            _cache.Add(1, "Alfa");

            _cache.Get(1).Should().Be("Alfa");
        }
    }
    [TestFixture]
    public class CasosNegativos {
        [Test]
        public void Contructor_CapacidadCero_LanzaExcepcion() {
            var action = () => new CacheLru<int, string>(0);

            // Assert
            action.Should().Throw<ArgumentException>();
        }
    }
    
}