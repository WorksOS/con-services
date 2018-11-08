using System;
using System.IO;

namespace VSS.TRex.Utilities.ExtensionMethods
{
    /// <summary>
    /// Handy extensions for the binary reader class
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Decorates BinaryReader with a ReadGuid() method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Guid ReadGuid<T>(this T item) where T : BinaryReader
        {
            byte[] bytes = new byte[16];
            item.Read(bytes, 0, 16);
            return new Guid(bytes);
        }
    }
}
