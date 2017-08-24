using NUnit.Framework;
using RevitTestServices;
using RTF.Framework;

namespace Tests
{
    [TestFixture]
    public class SystemTestExample : RevitNodeTestBase
    {

        [Test]
        [TestModel(@".\Models\empty.rfa")]
        public void Location()
        {
            Assert.AreEqual(1, 1);
        }
    }
}