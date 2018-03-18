using System;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using System.IO;
using Xunit;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
        public class TAGReaderTests
    {
        [Fact]
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
                Assert.True(E is ArgumentException, "Exception thrown n is incorrect");
            }

            TAGReader reader = new TAGReader(new MemoryStream());
            Assert.NotNull(reader);
        }
    }
}
