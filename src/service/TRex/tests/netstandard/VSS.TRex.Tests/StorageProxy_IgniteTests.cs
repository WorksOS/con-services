using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests
{
    public class StorageProxy_IgniteTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
    {
        [Fact]
        public void Test_StorageProxy_Ignite_Creation()
        {
            var proxy = new StorageProxy_Ignite(StorageMutability.Immutable);

            Assert.NotNull(proxy);

            proxy = new StorageProxy_Ignite(StorageMutability.Mutable);

            Assert.NotNull(proxy);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_ReadSpatialStreamFromPersistentStore()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_ReadStreamFromPersistentStore()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_ReadStreamFromPersistentStoreTest1()
        {
            Assert.True(false);
        }

        [Fact(Skip = "WIP")]
        public void Test_StorageProxy_Ignite_RemoveStreamFromPersistentStore_Mutable_NonSpatial()
        {
          var proxy = new StorageProxy_Ignite(StorageMutability.Mutable);

        }

        [Fact]
        public void Test_StorageProxy_Ignite_RemoveStreamFromPersistentStore_Mutable_Spatial_Existing()
        {
          var proxy = new StorageProxy_Ignite(StorageMutability.Immutable);

          var projectUid = Guid.NewGuid();
          var streamName = "StreamToDelete";

          proxy.WriteStreamToPersistentStore(projectUid, streamName, FileSystemStreamType.Designs, new MemoryStream(), null);
          proxy.RemoveStreamFromPersistentStore(projectUid, streamName).Should().Be(FileSystemErrorStatus.OK);
        }

        [Fact]
        public void Test_StorageProxy_Ignite_RemoveStreamFromPersistentStore_Mutable_Spatial_NotExisting()
        {
          var proxy = new StorageProxy_Ignite(StorageMutability.Immutable);
    
          var projectUid = Guid.NewGuid();
          var streamName = "StreamToDelete";
    
          proxy.RemoveStreamFromPersistentStore(projectUid, streamName).Should().Be(FileSystemErrorStatus.OK);
        } 

        [Fact]
        public void Test_StorageProxy_Ignite_RemoveStreamFromPersistentStore_Immutable_NonSpatial_Existing()
        {
          var proxy = new StorageProxy_Ignite(StorageMutability.Immutable);
    
          var projectUid = Guid.NewGuid();
          var streamName = "StreamToDelete";
    
          proxy.WriteStreamToPersistentStore(projectUid, streamName, FileSystemStreamType.Designs, new MemoryStream(), null);
          proxy.RemoveStreamFromPersistentStore(projectUid, streamName).Should().Be(FileSystemErrorStatus.OK);
        }
    
        [Fact]
        public void Test_StorageProxy_Ignite_RemoveStreamFromPersistentStore_Immutable_NonSpatial_NotExisting()
        {
          var proxy = new StorageProxy_Ignite(StorageMutability.Immutable);
     
          var projectUid = Guid.NewGuid();
          var streamName = "StreamToDelete";
     
          proxy.RemoveStreamFromPersistentStore(projectUid, streamName).Should().Be(FileSystemErrorStatus.OK);
        }
     
        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_WriteSpatialStreamToPersistentStore()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_WriteStreamToPersistentStore()
        {
            Assert.True(false);
        }
    }
}
