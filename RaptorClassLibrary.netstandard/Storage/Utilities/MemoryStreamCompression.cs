using System.IO;
using System.IO.Compression;

namespace VSS.VisionLink.Raptor.Storage.Utilities
{
    /// <summary>
    /// Provides a capability to take a memory stream compress it
    /// </summary>
    public class MemoryStreamCompression
    {
        /// <summary>
        /// Accepts a memory stream containing data to be compressed
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A memory stream containing the compressed result</returns>
        public static MemoryStream Compress(MemoryStream input)
        {
            if (input == null)
            {
                return null;
            }

            MemoryStream compressStream = new MemoryStream();

            input.Position = 0;
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress, true))
            {
                input.CopyTo(compressor);
            }

            compressStream.Position = 0;
            return compressStream;
        }

        /// <summary>
        /// Accepts a memory stream containing data to be compressed
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A memory stream containing the decompressed result</returns>
        public static MemoryStream Decompress(MemoryStream input)
        {
            if (input == null)
            {
                return null;
            }

            var output = new MemoryStream();

            input.Position = 0;
            using (var decompressor = new DeflateStream(input, CompressionMode.Decompress, true))
            {
                decompressor.CopyTo(output);
            }

            output.Position = 0;
            return output;
        }
    }
}
