using System;
using System.IO;
using VSS.TRex.TAGFiles.Classes.Processors;
using Xunit;

namespace TAGFiles.Tests
{
        public class TAGReaderTests
    {
        [Fact]
        public void Test_TAGReader_Creation()
        {
            //Verify the no-arg constructor fails
            try
            {
                TAGReader _ = new TAGReader();

                Assert.True(false);
            }
            catch (Exception E)
            {
                Assert.True(E is ArgumentException, "Exception thrown is incorrect");
            }

            TAGReader reader = new TAGReader(new MemoryStream());
            Assert.NotNull(reader);
        }
    }
}
