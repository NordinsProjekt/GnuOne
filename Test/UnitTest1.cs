using Library.HelpClasses;
namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Denna kommer alltid att faila, för det krävs mycket mer för att kunna köra RSADecryptIt
            MegaCrypt megaCrypt = new MegaCrypt();
            Assert.IsTrue(megaCrypt.RSADecryptIt("", ""));
        }
        [TestMethod]
        public void TestMethod2()
        {

        }
    }
}