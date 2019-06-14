using VSS.TRex.Storage.Utilities;
using System.Linq;
using System.IO;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Storage.Utilities
{
        public class MemoryStreamCompressionTests : IClassFixture<DILoggingFixture>
    {
        [Fact()]
        public void CompressTest()
        {
            byte[] someBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };

            MemoryStream ms = new MemoryStream(someBytes);
            MemoryStream result = MemoryStreamCompression.Decompress(MemoryStreamCompression.Compress(ms));

            Assert.True(result.ToArray().SequenceEqual(someBytes), "Bytes are not the same after compression/decompression cycle");
        }

        [Fact()]
        public void CompressTest_Null()
        {
            Assert.Null(MemoryStreamCompression.Compress(null));
        }

        [Fact()]
        public void DeCompressTest()
        {
            Assert.Null(MemoryStreamCompression.Decompress(null));
        }
    }
}
