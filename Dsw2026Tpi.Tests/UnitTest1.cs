namespace Dsw2026Tpi.Tests
{
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
    }
}