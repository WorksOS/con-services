using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Storage.Tests
{
    [TestClass()]
    public class StorageProxy_IgniteTests
    {
        [TestMethod()]
        public void Test_StorageProxy_Ignite_Creation()
        {
            var proxy = new StorageProxy_Ignite(StorageMutability.Immutable);

            Assert.IsTrue(proxy != null, "Immutable storage proxy not created");

            proxy = new StorageProxy_Ignite(StorageMutability.Mutable);

            Assert.IsTrue(proxy != null, "Mutable storage proxy not created");
        }

        [TestMethod()]
        public void Test_StorageProxy_Ignite_Instance()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_StorageProxy_Ignite_ReadSpatialStreamFromPersistentStore()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_StorageProxy_Ignite_ReadStreamFromPersistentStore()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_StorageProxy_Ignite_ReadStreamFromPersistentStoreTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_StorageProxy_Ignite_ReadStreamFromPersistentStoreDirect()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_StorageProxy_Ignite_RemoveStreamFromPersistentStore()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_StorageProxy_Ignite_WriteSpatialStreamToPersistentStore()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_StorageProxy_Ignite_WriteStreamToPersistentStore()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_StorageProxy_Ignite_WriteStreamToPersistentStoreDirect()
        {
            Assert.Fail();
        }
    }
}