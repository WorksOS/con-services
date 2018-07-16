using System;
using System.IO;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Processors;
using Xunit;

namespace TAGFiles.Tests
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

                Assert.True(false);
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
