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
            TAGReader reader = new TAGReader(new MemoryStream());
            Assert.NotNull(reader);
        }
    }
}
