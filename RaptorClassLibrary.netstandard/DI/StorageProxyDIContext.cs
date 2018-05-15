using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.DI
{
    /// <summary>
    /// Contains dependency injected dependecy state for storage proxy access in TRex
    /// </summary>
    public static class StorageProxyDIContext
    {
        /// <summary>
        /// The dependency injected factory for TRex storage proxies
        /// </summary>
        public static IStorageProxyFactory storageProxyFactory { get; set; } = null;
    }
}
