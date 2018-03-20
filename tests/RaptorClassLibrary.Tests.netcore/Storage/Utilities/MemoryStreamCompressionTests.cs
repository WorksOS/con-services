using VSS.VisionLink.Raptor.Storage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Xunit;

namespace VSS.VisionLink.Raptor.Storage.Utilities.Tests
{
        public class MemoryStreamCompressionTests
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