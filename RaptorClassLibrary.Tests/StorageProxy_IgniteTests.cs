using VSS.VisionLink.Raptor.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VSS.VisionLink.Raptor.Storage.Tests
{
        public class StorageProxy_IgniteTests
    {
        [Fact()]
        public void Test_StorageProxy_Ignite_Creation()
        {
            var proxy = new StorageProxy_Ignite(StorageMutability.Immutable);

            Assert.NotNull(proxy);

            proxy = new StorageProxy_Ignite(StorageMutability.Mutable);

            Assert.NotNull(proxy);
        }

        [Fact()]
        public void Test_StorageProxy_Ignite_Instance()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_StorageProxy_Ignite_ReadSpatialStreamFromPersistentStore()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_StorageProxy_Ignite_ReadStreamFromPersistentStore()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_StorageProxy_Ignite_ReadStreamFromPersistentStoreTest1()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_StorageProxy_Ignite_ReadStreamFromPersistentStoreDirect()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_StorageProxy_Ignite_RemoveStreamFromPersistentStore()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_StorageProxy_Ignite_WriteSpatialStreamToPersistentStore()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_StorageProxy_Ignite_WriteStreamToPersistentStore()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_StorageProxy_Ignite_WriteStreamToPersistentStoreDirect()
        {
            Assert.Fail();
        }
    }
}