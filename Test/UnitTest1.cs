using Library.HelpClasses;
namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Denna kommer alltid att faila, f�r det kr�vs mycket mer f�r att kunna k�ra RSADecryptIt
            MegaCrypt megaCrypt = new MegaCrypt();
            Assert.IsTrue(megaCrypt.RSADecryptIt("", ""));
        }
        [TestMethod]
        public void TestMethod2()
        {

        }
    }
}