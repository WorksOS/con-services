using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using System.IO;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
    [TestClass]
    public class TAGReaderTests
    {
        [TestMethod]
        public void Test_TAGReader_Creation()
        {
            //Verify the no-arg connstructor fails
            try
            {
                TAGReader stream = new TAGReader();

                Assert.Fail("TAGFileReader no arg create did not fail");
            }
            catch (Exception E)
            {
                Assert.IsTrue(E is ArgumentException, "Exception thrown n is incorrect");
            }

            TAGReader reader = new TAGReader(new MemoryStream());
            Assert.IsTrue(reader != null, "Reader with memory stream failed to construct");
        }
    }
}
