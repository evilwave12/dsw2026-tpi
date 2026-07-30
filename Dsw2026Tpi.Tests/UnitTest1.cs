namespace Dsw2026Tpi.Tests;
    using NSubstitute;

    public class UnitTest1 //se le pone NombreClaseTests
    {
        [Fact] //atributo para que se reconozca la prueba
        public void Test1() //NombreMetodo_CuandoYSituacionHipotetica_EntoncesResultado
        {
            //Paso 1:Arrange

            //pongo lo que se necesite para hacer la prueba, por ej una lista de precios, lista de medicos, una instancia de la clase etc
            //si queremos probar por ej un servicio que necesita inyecciones y dependencias, se lo hace mediante mocks

            //Paso 2: Act

            //invoco el metodo que quiero probar

            //Paso 3:Assert

            //Assert.Equal(valor_esperado, valor_actual); se compara lo que se quiere obtener y lo que realmente se obtiene
        }

        //si queremos probar que se lance una excepcion, se combinan act y assert en la misma linea

        //Arrange

        //seguimos definiendo lo que se necesita

        //Act y Assert

        //Assert.Throws<NombreExcepcion>(() => metodoQueLanzaExcepcion(parametros)); //se espera que se lance la excepcion al invocar el metodo
    }

//para utilizar mocks, lo instanciamos en la prueba por ej private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>(); y luego lo probamos
//el mock se configura dependiendo del metodo y camino que vayamos a probar