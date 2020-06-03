using System;
using System.IO;

namespace VSS.TRex.Common.Utilities.ExtensionMethods
{
    /// <summary>
    /// Handy extensions for the binary reader class
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Decorates BinaryReader with a ReadGuid() method
        /// </summary>
        public static Guid ReadGuid<T>(this T item) where T : BinaryReader
        {
            // ReSharper disable once SuggestVarOrType_Elsewhere
            Span<byte> bytes = stackalloc byte[16];
            item.Read(bytes);
            return new Guid(bytes);
        }
    }
}
