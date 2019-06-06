using System;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.TAGFiles.Classes.Processors;
using Xunit;

namespace TAGFiles.Tests
{
    public class TAGReaderTests
    {
        [Fact]
        public void Test_TAGReader_Creation()
        {
            TAGReader reader = new TAGReader(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION));
            Assert.NotNull(reader);
        }
    }
}
