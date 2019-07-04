﻿using System;
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
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Guid ReadGuid<T>(this T item) where T : BinaryReader
        {
            byte[] bytes = new byte[16]; // TODO NETCORE: Change this to a Stackalloc when move to .Net Standard 2.1
            item.Read(bytes, 0, 16);
            return new Guid(bytes);
        }
    }
}
