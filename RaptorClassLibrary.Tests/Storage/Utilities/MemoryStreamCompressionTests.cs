using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Storage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VSS.VisionLink.Raptor.Storage.Utilities.Tests
{
    [TestClass()]
    public class MemoryStreamCompressionTests
    {
        [TestMethod()]
        public void CompressTest()
        {
            byte[] someBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };

            MemoryStream ms = new MemoryStream(someBytes);
            MemoryStream result = MemoryStreamCompression.Decompress(MemoryStreamCompression.Compress(ms));

            Assert.IsTrue(result.ToArray().SequenceEqual(someBytes), "Bytes are not the same after compression/decompression cycle");
        }

        [TestMethod()]
        public void CompressTest_Null()
        {
            Assert.IsTrue(MemoryStreamCompression.Compress(null) == null, "Compression of null stream did not return null stream");
        }

        [TestMethod()]
        public void DeCompressTest()
        {
            Assert.IsTrue(MemoryStreamCompression.Decompress(null) == null, "Decompression of null stream did not return null stream");
        }
    }
}