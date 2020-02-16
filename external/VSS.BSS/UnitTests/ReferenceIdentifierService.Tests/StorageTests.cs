using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.NH_OPMockObjectSet;
using VSS.Nighthawk.ReferenceIdentifierService.Data;
using VSS.Nighthawk.ReferenceIdentifierService.Encryption;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Implementations.Helpers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;
using VSS.Nighthawk.ReferenceIdentifierService.Tests.Helpers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests
{
  [TestClass]
  public class StorageTests
  {
    #region AssetReference Tests

    [TestMethod]
    public void TestGetAssetReference_AssetReferenceNotInCache_AssetReferenceIsInDatabase_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var mockAssetReferenceRecords = CreateAssetReferenceRecords();
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().AssetReferenceReadOnly).Returns(mockAssetReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object,_mockStringEncryptor.Object, 1, 1, 1, 1);
      AssetReference assetReferenceRecord = mockAssetReferenceRecords.First(o => o.ID == 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = assetReferenceRecord.fk_StoreID,
        Alias = assetReferenceRecord.Alias,
        Value = assetReferenceRecord.Value
      };
      Guid? actualUid = storage.FindAssetReference(idDef);
      Assert.AreEqual(assetReferenceRecord.UID, actualUid);
      string cacheKey = string.Format("AssetReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Once());
    }

    [TestMethod]
    public void TestGetAssetReference_AssetReferenceNotInCache_AssetReferenceHasDuplicates()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var mockAssetReferenceRecords = CreateAssetReferenceRecords();
      // set up duplicate Alias-Value pairs
      const long storeId = 1;
      mockAssetReferenceRecords.AddObject(GetAssetReferenceRecord(3, storeId, "sn_make", "AAA_CAT", new UUIDSequentialGuid().CreateGuid()));
      mockAssetReferenceRecords.AddObject(GetAssetReferenceRecord(4, storeId, "sn_make", "AAA_CAT", new UUIDSequentialGuid().CreateGuid()));
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().AssetReferenceReadOnly).Returns(mockAssetReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      AssetReference assetReferenceRecord = mockAssetReferenceRecords.First(o => o.Alias == "sn_make" && o.Value == "AAA_CAT");
      var idDef = new IdentifierDefinition
      {
        StoreId = assetReferenceRecord.fk_StoreID,
        Alias = assetReferenceRecord.Alias,
        Value = assetReferenceRecord.Value
      };
      AssertEx.Throws<DuplicateAssetReferenceFoundException>(() => storage.FindAssetReference(idDef), "Asset");
    }

    [TestMethod]
    public void TestGetAssetReference_AssetReferenceNotInCache_InvalidOperation()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      _mockNhOp.Setup(e => e.CreateContext()).Throws(new InvalidOperationException("Invalid Operation"));
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      
      var idDef = new IdentifierDefinition
      {
        StoreId =1,
        Alias = "2",
        Value = "3"
      };
      AssertEx.Throws<InvalidOperationException>(() => storage.FindAssetReference(idDef), "Invalid Operation");
    }


    [TestMethod]
    public void TestGetAssetReference_AssetReferenceNotInCache_AssetReferenceIsNotInDatabase_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var mockAssetReferenceRecords = CreateAssetReferenceRecords();
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().AssetReferenceReadOnly).Returns(mockAssetReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue"
      };
      Guid? actualUid = storage.FindAssetReference(idDef);
      Assert.IsNull(actualUid);
      string cacheKey = string.Format("AssetReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Once());
    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "db exception")]
    public void TestGetAssetReference_AssetReferenceNotInCache_AccessingDatabaseThrowsException()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().AssetReferenceReadOnly).Throws((new Exception("db exception")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue"
      };
      storage.FindAssetReference(idDef);
    }

    [TestMethod]
    public void TestGetAssetReference_AssetReferenceIsInCache_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockAssetReferenceRecords = CreateAssetReferenceRecords();
      AssetReference assetReferenceRecord = mockAssetReferenceRecords.First(o => o.ID == 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = assetReferenceRecord.fk_StoreID,
        Alias = assetReferenceRecord.Alias,
        Value = assetReferenceRecord.Value
      };
      string cacheKey = string.Format("AssetReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Setup(o => o.GetData(cacheKey)).Returns(assetReferenceRecord.UID);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      Guid? actualUid = storage.FindAssetReference(idDef);
      Assert.AreEqual(assetReferenceRecord.UID, actualUid);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Never());
    }

    [TestMethod]
    public void TestGetAssetReference_NullIdentifierDefinition_ReturnsNull()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      Guid? actualUid = storage.FindAssetReference(null);
      Assert.IsNull(actualUid);
    }

    [TestMethod]
    public void TestAddAssetReference()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      Guid uid = new UUIDSequentialGuid().CreateGuid();
      var idDef = new IdentifierDefinition
      {
        StoreId = 5,
        Alias = "newAlias",
        Value = "newValue",
        UID = uid
      };

      var assetReferenceRecords = new List<AssetReference>();
      _mockNhOp.Setup(o => o.CreateContext().AssetReference.AddObject(It.IsAny<AssetReference>()))
        .Callback<AssetReference>(assetReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      storage.AddAssetReference(idDef);
      _mockNhOp.Verify(o => o.CreateContext().AssetReference.AddObject(It.IsAny<AssetReference>()), Times.Once());
      _mockNhOp.Verify(o => o.CreateContext().SaveChanges(), Times.Once());
      Assert.AreEqual(1, assetReferenceRecords.Count);
      AssetReference addedAssetReference = assetReferenceRecords.First();
      Assert.AreEqual(idDef.StoreId, addedAssetReference.fk_StoreID);
      Assert.AreEqual(idDef.Value, addedAssetReference.Value);
      Assert.AreEqual(idDef.Alias, addedAssetReference.Alias);
      Assert.AreEqual(idDef.UID, addedAssetReference.UID);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), addedAssetReference.UID, 1), Times.Once());
    }

    [TestMethod]
    public void TestAddAssetReference_Duplicate_Failure()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      Guid uid = new UUIDSequentialGuid().CreateGuid();
      var idDef = new IdentifierDefinition
      {
        StoreId = 5,
        Alias = "newAlias",
        Value = "newValue",
        UID = uid
      };

      var assetReferenceRecords = new List<AssetReference>();
      _mockNhOp.Setup(o => o.CreateContext().AssetReference.AddObject(It.IsAny<AssetReference>()))
        .Callback<AssetReference>(assetReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges())
        .Throws(new UpdateException("An error occured while updating the entries. See the inner exception for details.",
          BuildSqlException("Violation of UNIQUE KEY constraint 'ck_Store_Alias_Value'. Cannot insert duplicate key in object 'dbo.AssetReference'.", 
          "The statement has been terminated.")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      AssertEx.Throws<CreatingDuplicateAssetReferenceException>(() => storage.AddAssetReference(idDef), "Asset");
    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "db exception")]
    public void TestAddAssetReference_AccessingDatabaseThrowsException()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      _mockNhOp.Setup(o => o.CreateContext().AssetReference.AddObject(It.IsAny<AssetReference>())).Throws((new Exception("db exception")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue",
        UID = new UUIDSequentialGuid().CreateGuid()
      };
      storage.AddAssetReference(idDef);
    }

    [TestMethod]
    public void TestAddAssetReference_NullIdentifierDefinition_DoesNotAddRowToDatabase()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var assetReferenceRecords = new List<AssetReference>();
      _mockNhOp.Setup(o => o.CreateContext().AssetReference.AddObject(It.IsAny<AssetReference>()))
        .Callback<AssetReference>(assetReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      storage.AddAssetReference(null);
      _mockNhOp.Verify(o => o.CreateContext().AssetReference.AddObject(It.IsAny<AssetReference>()), Times.Never());
      _mockNhOp.Verify(o => o.CreateContext().SaveChanges(), Times.Never());
      Assert.AreEqual(0, assetReferenceRecords.Count);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), It.IsAny<Guid>(), 1), Times.Never());
    }

    [TestMethod]
    public void TestGetAssociatedDevices_EmptyAssetUid_ReturnNull()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var actual = storage.GetAssociatedDevices(Guid.Empty);
      Assert.IsNull(actual);
    }

    [TestMethod]
    public void TestFindOwner_EmptyAssetUid_ReturnsNull()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var actual = storage.FindOwner(Guid.Empty);
      Assert.IsNull(actual);
    }

    [TestMethod]
    public void TestGetAssociatedDevices_EmptyAsset_ReturnsEmptyList()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      MockObjectSet<Device> deviceObject = new MockObjectSet<Device>();
      MockObjectSet<Asset> assetObject = new MockObjectSet<Asset>();
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      _mockNhOp.Setup(o => o.CreateContext().DeviceReadOnly).Returns(deviceObject);
      _mockNhOp.Setup(o => o.CreateContext().AssetReadOnly).Returns(assetObject);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      var actual = storage.GetAssociatedDevices(Guid.NewGuid());
      Assert.IsNotNull(actual);
      Assert.AreEqual(0, actual.Count);
    }

    [TestMethod]
    public void TestGetAssociatedDevicse_NH_OP_Exception_Throws_Exception()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      _mockNhOp.Setup(e => e.CreateContext()).Throws(new Exception("an exception"));
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      try
      {
        var actual = storage.GetAssociatedDevices(Guid.NewGuid());
        Assert.Fail();
      }
      catch (Exception) { }
    }

    [TestMethod]
    public void TestFindOwner_NoDatabaseRecords_ReturnsNull()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      MockObjectSet<Device> deviceObject = new MockObjectSet<Device>();
      MockObjectSet<Asset> assetObject = new MockObjectSet<Asset>();
      MockObjectSet<Customer> customerObject = new MockObjectSet<Customer>();
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      _mockNhOp.Setup(o => o.CreateContext().DeviceReadOnly).Returns(deviceObject);
      _mockNhOp.Setup(o => o.CreateContext().AssetReadOnly).Returns(assetObject);
      _mockNhOp.Setup(o => o.CreateContext().CustomerReadOnly).Returns(customerObject);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      var actual = storage.FindOwner(Guid.NewGuid());
      Assert.IsNull(actual);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "darn")]
    public void TestFindOwner_DatabaseException()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      MockObjectSet<Device> deviceObject = new MockObjectSet<Device>();
      MockObjectSet<Asset> assetObject = new MockObjectSet<Asset>();
      MockObjectSet<Customer> customerObject = new MockObjectSet<Customer>();
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      _mockNhOp.Setup(o => o.CreateContext()).Throws(new Exception("darn"));
      var actual = storage.FindOwner(Guid.NewGuid());
    }

    [TestMethod]
    public void TestGetAssociatedDevices_AssetUID_ReturnsCorrectDeviceUid()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      MockObjectSet<Device> deviceObject = new MockObjectSet<Device>();
      Device device = new Device();
      device.ID = 1111111;
      device.DeviceUID = Guid.NewGuid();
      deviceObject.AddObject(device);
      MockObjectSet<Asset> assetObject = new MockObjectSet<Asset>();
      Asset asset = new Asset();
      asset.AssetUID = Guid.NewGuid();
      asset.AssetID = 1;
      asset.fk_DeviceID = device.ID;
      assetObject.AddObject(asset);
      _mockNhOp.Setup(o=>o.CreateContext().DeviceReadOnly).Returns(deviceObject);
      _mockNhOp.Setup(o => o.CreateContext().AssetReadOnly).Returns(assetObject);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var actual = storage.GetAssociatedDevices(asset.AssetUID.Value);

      Assert.IsNotNull(actual);
      Assert.AreEqual(1, actual.Count);
      Assert.AreEqual(device.DeviceUID, actual[0]);
    }

    [TestMethod]
    public void TestFindOwner_AssetUID_ReturnsCorrectOwnerUid()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var customerBSSId = "customerBSSId";
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();

      MockObjectSet<Device> deviceObject = new MockObjectSet<Device>();
      Device device = new Device();
      device.ID = 111;
      device.OwnerBSSID = customerBSSId;
      deviceObject.AddObject(device);

      MockObjectSet<Asset> assetObject = new MockObjectSet<Asset>();
      Asset asset = new Asset();
      asset.AssetUID = assetUid;
      asset.AssetID = 222;
      asset.fk_DeviceID = device.ID;
      assetObject.AddObject(asset);

      MockObjectSet<Customer> customerObject = new MockObjectSet<Customer>();
      Customer customer = new Customer();
      customer.CustomerUID = customerUid;
      customer.BSSID = customerBSSId;
      customerObject.AddObject(customer);

      _mockNhOp.Setup(o=>o.CreateContext().DeviceReadOnly).Returns(deviceObject);
      _mockNhOp.Setup(o => o.CreateContext().AssetReadOnly).Returns(assetObject);
      _mockNhOp.Setup(o => o.CreateContext().CustomerReadOnly).Returns(customerObject);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);

      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var actual = storage.FindOwner(assetUid);

      Assert.IsNotNull(actual);
      Assert.AreEqual(customerUid, actual);
    }
    #endregion

    #region CustomerReference Tests

    [TestMethod, Ignore]
    public void TestCustomerReferenceIndexViolation()
    {
      try
      {
        using (var nhOp = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          nhOp.CustomerReference.AddObject(new CustomerReference
          {
            Alias = "BSSID",
            fk_StoreID = 3,
            UID = Guid.NewGuid(),
            Value = "DUPCUST234"
          });
          nhOp.CustomerReference.AddObject(new CustomerReference
          {
            Alias = "BSSID",
            fk_StoreID = 3,
            UID = Guid.NewGuid(),
            Value = "DUPCUST234"
          });
          nhOp.SaveChanges();
        }
      }
      catch (Exception e)
      {
        string msg = e.Message;
        Console.WriteLine(msg);
      }
    }

    [TestMethod]
    public void TestGetCustomerReference_CustomerReferenceNotInCache_CustomerReferenceIsInDatabase_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var mockCustomerReferenceRecords = CreateCustomerReferenceRecords();
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().CustomerReferenceReadOnly).Returns(mockCustomerReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      CustomerReference customerReferenceRecord = mockCustomerReferenceRecords.First(o => o.ID == 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = customerReferenceRecord.fk_StoreID,
        Alias = customerReferenceRecord.Alias,
        Value = customerReferenceRecord.Value
      };
      Guid? actualUid = storage.FindCustomerReference(idDef);
      Assert.AreEqual(customerReferenceRecord.UID, actualUid);
      string cacheKey = string.Format("CustomerReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Once());
    }

    [TestMethod]
    public void TestGetCustomerReference_CustomerReferenceNotInCache_CustomerReferenceHasDuplicates()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockCustomerReferenceRecords = CreateCustomerReferenceRecords();
      const long storeId = 1;
      // set up duplicate Alias-Value pairs
      mockCustomerReferenceRecords.AddObject(GetCustomerReferenceRecord(3, storeId, "BSSID", "DUPCUST123", new UUIDSequentialGuid().CreateGuid()));
      mockCustomerReferenceRecords.AddObject(GetCustomerReferenceRecord(4, storeId, "BSSID", "DUPCUST123", new UUIDSequentialGuid().CreateGuid()));
      
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().CustomerReferenceReadOnly).Returns(mockCustomerReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      CustomerReference customerReferenceRecord = mockCustomerReferenceRecords.First(o => o.Alias == "BSSID" && o.Value == "DUPCUST123");
      var idDef = new IdentifierDefinition
      {
        StoreId = customerReferenceRecord.fk_StoreID,
        Alias = customerReferenceRecord.Alias,
        Value = customerReferenceRecord.Value
      };
      AssertEx.Throws<DuplicateCustomerReferenceFoundException>(() => storage.FindCustomerReference(idDef), "Customer");
    }

    [TestMethod]
    public void TestGetCustomerReference_CustomerReferenceNotInCache_CustomerReferenceIsNotInDatabase_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockCustomerReferenceRecords = CreateCustomerReferenceRecords();
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().CustomerReferenceReadOnly).Returns(mockCustomerReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue"
      };
      Guid? actualUid = storage.FindCustomerReference(idDef);
      Assert.IsNull(actualUid);
      string cacheKey = string.Format("CustomerReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Once());

    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "db exception")]
    public void TestGetCustomerReference_CustomerReferenceNotInCache_AccessingDatabaseThrowsException()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().CustomerReferenceReadOnly).Throws((new Exception("db exception")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue"
      };
      storage.FindCustomerReference(idDef);
    }

    [TestMethod]
    public void TestGetCustomerReference_CustomerReferenceIsInCache_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockCustomerReferenceRecords = CreateCustomerReferenceRecords();
      CustomerReference customerReferenceRecord = mockCustomerReferenceRecords.First(o => o.ID == 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = customerReferenceRecord.fk_StoreID,
        Alias = customerReferenceRecord.Alias,
        Value = customerReferenceRecord.Value
      };
      string cacheKey = string.Format("CustomerReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Setup(o => o.GetData(cacheKey)).Returns(customerReferenceRecord.UID);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      Guid? actualUid = storage.FindCustomerReference(idDef);
      Assert.AreEqual(customerReferenceRecord.UID, actualUid);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Never());
    }

    [TestMethod]
    public void TestGetCustomerReference_NullIdentifierDefinition_ReturnsNull()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      Guid? actualUid = storage.FindCustomerReference(null);
      Assert.IsNull(actualUid);
    }

    [TestMethod]
    public void TestAddCustomerReference()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      Guid uid = new UUIDSequentialGuid().CreateGuid();
      var idDef = new IdentifierDefinition
      {
        StoreId = 5,
        Alias = "newAlias",
        Value = "newValue",
        UID = uid
      };

      var customerReferenceRecords = new List<CustomerReference>();
      _mockNhOp.Setup(o => o.CreateContext().CustomerReference.AddObject(It.IsAny<CustomerReference>()))
        .Callback<CustomerReference>(customerReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      storage.AddCustomerReference(idDef);
      _mockNhOp.Verify(o => o.CreateContext().CustomerReference.AddObject(It.IsAny<CustomerReference>()), Times.Once());
      _mockNhOp.Verify(o => o.CreateContext().SaveChanges(), Times.Once());
      Assert.AreEqual(1, customerReferenceRecords.Count);
      CustomerReference addedCustomerReference = customerReferenceRecords.First();
      Assert.AreEqual(idDef.StoreId, addedCustomerReference.fk_StoreID);
      Assert.AreEqual(idDef.Value, addedCustomerReference.Value);
      Assert.AreEqual(idDef.Alias, addedCustomerReference.Alias);
      Assert.AreEqual(idDef.UID, addedCustomerReference.UID);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), addedCustomerReference.UID, 1), Times.Once());
    }

    [TestMethod]
    public void TestUpdateCustomerReference()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      Guid uid = new UUIDSequentialGuid().CreateGuid();
      var idDef = new IdentifierDefinition
      {
        StoreId = 5,
        Alias = "alias",
        Value = "newValue",
        UID = uid
      };

      var data = new CustomerReference
      {
        fk_StoreID = 1,
        Alias = "alias",
        Value = "oldValue",
        UID = uid
      };
      var customerData = new MockObjectSet<CustomerReference>();
      customerData.AddObject(data);

      _mockNhOp.SetupGet(o => o.CreateContext().CustomerReference).Returns(customerData);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      storage.UpdateCustomerReference(idDef);
      _mockNhOp.Verify(o => o.CreateContext().SaveChanges(), Times.Once());
      Assert.AreEqual(idDef.Value, customerData.FirstOrDefault().Value);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), data.UID, 1), Times.Once());
    }

    [TestMethod]
    public void TestFindAccountsForDealer()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      //setup needed customers
      var customerData = new MockObjectSet<Customer>();
      var dealer = new Customer
      {
        ID = 1,
        CustomerUID = Guid.NewGuid(),
        NetworkDealerCode ="Dealer",
        IsActivated = true,
        fk_CustomerTypeID = (int)CustomerTypeEnum.Dealer
      };
      var account1 = new Customer
      {
        ID = 2,
        CustomerUID = Guid.NewGuid(),
        DealerAccountCode = "Account1",
        IsActivated = true,
        fk_CustomerTypeID = (int)CustomerTypeEnum.Account
      };
      var account2 = new Customer
      {
        ID = 3,
        CustomerUID = Guid.NewGuid(),
        DealerAccountCode = "Account2",
        IsActivated = true,
        fk_CustomerTypeID = (int)CustomerTypeEnum.Account
      };
      var account3 = new Customer
      {
        ID = 4,
        CustomerUID = Guid.NewGuid(),
        IsActivated = true,
        DealerAccountCode = "Account3",
        fk_CustomerTypeID = (int)CustomerTypeEnum.Account
      };
      customerData.AddObject(dealer);
      customerData.AddObject(account1);
      customerData.AddObject(account2);
      customerData.AddObject(account3);
      _mockNhOp.SetupGet(o => o.CreateContext().CustomerReadOnly).Returns(customerData);

      //Setup needed relationships
      var customerRelationshipData = new MockObjectSet<CustomerRelationship>();
      var dealerAccount1 = new CustomerRelationship { BSSRelationshipID = "1", fk_ClientCustomerID = account1.ID, fk_ParentCustomerID = dealer.ID, fk_CustomerRelationshipTypeID = (int)CustomerRelationshipTypeEnum.TCSDealer };
      var dealerAccount3 = new CustomerRelationship { BSSRelationshipID = "2", fk_ClientCustomerID = account3.ID, fk_ParentCustomerID = dealer.ID, fk_CustomerRelationshipTypeID = (int)CustomerRelationshipTypeEnum.TCSDealer };
      customerRelationshipData.AddObject(dealerAccount1);
      customerRelationshipData.AddObject(dealerAccount3);
      _mockNhOp.SetupGet(o => o.CreateContext().CustomerRelationshipReadOnly).Returns(customerRelationshipData);

      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var accounts = storage.FindAccountsForDealer(dealer.CustomerUID.Value);
      Assert.AreEqual(2, accounts.Count);
      accounts = accounts.OrderBy(e => e.DealerAccountCode).ToList();
      Assert.AreEqual(account1.CustomerUID, accounts[0].CustomerUid);
      Assert.AreEqual(account1.DealerAccountCode, accounts[0].DealerAccountCode);
      Assert.AreEqual(account3.CustomerUID, accounts[1].CustomerUid);
      Assert.AreEqual(account3.DealerAccountCode, accounts[1].DealerAccountCode);
    }

    [TestMethod]
    public void TestAddCustomerReference_Duplicate_Failure()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      Guid uid = new UUIDSequentialGuid().CreateGuid();
      var idDef = new IdentifierDefinition
      {
        StoreId = 5,
        Alias = "newAlias",
        Value = "newValue",
        UID = uid
      };

      var customerReferenceRecords = new List<CustomerReference>();
      _mockNhOp.Setup(o => o.CreateContext().CustomerReference.AddObject(It.IsAny<CustomerReference>()))
        .Callback<CustomerReference>(customerReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges())
        .Throws(new UpdateException("An error occured while updating the entries. See the inner exception for details.",
          BuildSqlException("Violation of UNIQUE KEY constraint 'ck_Store_Alias_Value'. Cannot insert duplicate key in object 'dbo.CustomerReference'.",
          "The statement has been terminated.")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      AssertEx.Throws<CreatingDuplicateCustomerReferenceException>(() => storage.AddCustomerReference(idDef), "Customer");
    }

    [TestMethod]
    public void TestAddCustomerReference_NonDuplicate_Failure()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      Guid uid = new UUIDSequentialGuid().CreateGuid();
      var idDef = new IdentifierDefinition
      {
        StoreId = 5,
        Alias = "newAlias",
        Value = "newValue",
        UID = uid
      };

      var customerReferenceRecords = new List<CustomerReference>();
      _mockNhOp.Setup(o => o.CreateContext().CustomerReference.AddObject(It.IsAny<CustomerReference>()))
        .Callback<CustomerReference>(customerReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges())
        .Throws(new UpdateException("An error occured while updating the entries. See the inner exception for details."));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      AssertEx.Throws<UpdateException>(() => storage.AddCustomerReference(idDef), "An error occured while updating the entries");
    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "db exception")]
    public void TestAddCustomerReference_AccessingDatabaseThrowsException()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      _mockNhOp.Setup(o => o.CreateContext().CustomerReference.AddObject(It.IsAny<CustomerReference>())).Throws((new Exception("db exception")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue",
        UID = new UUIDSequentialGuid().CreateGuid()
      };
      storage.AddCustomerReference(idDef);
    }

    [TestMethod]
    public void TestAddCustomerReference_NullIdentifierDefinition_DoesNotAddRowToDatabase()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var customerReferenceRecords = new List<CustomerReference>();
      _mockNhOp.Setup(o => o.CreateContext().CustomerReference.AddObject(It.IsAny<CustomerReference>()))
        .Callback<CustomerReference>(customerReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      storage.AddCustomerReference(null);
      _mockNhOp.Verify(o => o.CreateContext().CustomerReference.AddObject(It.IsAny<CustomerReference>()), Times.Never());
      _mockNhOp.Verify(o => o.CreateContext().SaveChanges(), Times.Never());
      Assert.AreEqual(0, customerReferenceRecords.Count);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), It.IsAny<Guid>(), 1), Times.Never());
    }

    #endregion

    #region DeviceReference Tests

    [TestMethod]
    public void TestGetDeviceReference_DeviceReferenceNotInCache_DeviceReferenceIsInDatabase_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var mockDeviceReferenceRecords = CreateDeviceReferenceRecords();
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().DeviceReferenceReadOnly).Returns(mockDeviceReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      DeviceReference deviceReferenceRecord = mockDeviceReferenceRecords.First(o => o.ID == 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = deviceReferenceRecord.fk_StoreID,
        Alias = deviceReferenceRecord.Alias,
        Value = deviceReferenceRecord.Value
      };
      Guid? actualUid = storage.FindDeviceReference(idDef);
      Assert.AreEqual(deviceReferenceRecord.UID, actualUid);
      string cacheKey = string.Format("DeviceReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Once());
    }

    [TestMethod]
    public void TestGetDeviceReference_DeviceReferenceNotInCache_DeviceReferenceHasDuplicates()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockDeviceReferenceRecords = CreateDeviceReferenceRecords();
      // set up duplicate Alias-Value pairs
      const long storeId = 1;
      mockDeviceReferenceRecords.AddObject(GetDeviceReferenceRecord(3, storeId, "GpsDeviceId", "dup234", new UUIDSequentialGuid().CreateGuid()));
      mockDeviceReferenceRecords.AddObject(GetDeviceReferenceRecord(4, storeId, "GpsDeviceId", "dup234", new UUIDSequentialGuid().CreateGuid()));
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().DeviceReferenceReadOnly).Returns(mockDeviceReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      DeviceReference deviceReferenceRecord = mockDeviceReferenceRecords.First(o => o.Alias == "GpsDeviceId" && o.Value == "dup234");
      var idDef = new IdentifierDefinition
      {
        StoreId = deviceReferenceRecord.fk_StoreID,
        Alias = deviceReferenceRecord.Alias,
        Value = deviceReferenceRecord.Value
      };
      AssertEx.Throws<DuplicateDeviceReferenceFoundException>(() => storage.FindDeviceReference(idDef), "Device");
    }

    [TestMethod]
    public void TestGetDeviceReference_DeviceReferenceNotInCache_DeviceReferenceIsNotInDatabase_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockDeviceReferenceRecords = CreateDeviceReferenceRecords();
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().DeviceReferenceReadOnly).Returns(mockDeviceReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue"
      };
      Guid? actualUid = storage.FindDeviceReference(idDef);
      Assert.IsNull(actualUid);
      string cacheKey = string.Format("DeviceReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Once());

    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "db exception")]
    public void TestGetDeviceReference_DeviceReferenceNotInCache_AccessingDatabaseThrowsException()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().DeviceReferenceReadOnly).Throws((new Exception("db exception")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue"
      };
      storage.FindDeviceReference(idDef);
    }

    [TestMethod]
    public void TestGetDeviceReference_DeviceReferenceIsInCache_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockDeviceReferenceRecords = CreateDeviceReferenceRecords();
      DeviceReference deviceReferenceRecord = mockDeviceReferenceRecords.First(o => o.ID == 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = deviceReferenceRecord.fk_StoreID,
        Alias = deviceReferenceRecord.Alias,
        Value = deviceReferenceRecord.Value
      };
      string cacheKey = string.Format("DeviceReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Setup(o => o.GetData(cacheKey)).Returns(deviceReferenceRecord.UID);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      Guid? actualUid = storage.FindDeviceReference(idDef);
      Assert.AreEqual(deviceReferenceRecord.UID, actualUid);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Never());
    }

    [TestMethod]
    public void TestGetDeviceReference_NullIdentifierDefinition_ReturnsNull()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      Guid? actualUid = storage.FindDeviceReference(null);
      Assert.IsNull(actualUid);
    }

    [TestMethod]
    public void TestAddDeviceReference()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      Guid uid = new UUIDSequentialGuid().CreateGuid();
      var idDef = new IdentifierDefinition
      {
        StoreId = 5,
        Alias = "newAlias",
        Value = "newValue",
        UID = uid
      };

      var deviceReferenceRecords = new List<DeviceReference>();
      _mockNhOp.Setup(o => o.CreateContext().DeviceReference.AddObject(It.IsAny<DeviceReference>()))
        .Callback<DeviceReference>(deviceReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      storage.AddDeviceReference(idDef);
      _mockNhOp.Verify(o => o.CreateContext().DeviceReference.AddObject(It.IsAny<DeviceReference>()), Times.Once());
      _mockNhOp.Verify(o => o.CreateContext().SaveChanges(), Times.Once());
      Assert.AreEqual(1, deviceReferenceRecords.Count);
      DeviceReference addedDeviceReference = deviceReferenceRecords.First();
      Assert.AreEqual(idDef.StoreId, addedDeviceReference.fk_StoreID);
      Assert.AreEqual(idDef.Value, addedDeviceReference.Value);
      Assert.AreEqual(idDef.Alias, addedDeviceReference.Alias);
      Assert.AreEqual(idDef.UID, addedDeviceReference.UID);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), addedDeviceReference.UID, 1), Times.Once());
    }

    [TestMethod]
    public void TestAddDeviceReference_Duplicate_Failure()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      Guid uid = new UUIDSequentialGuid().CreateGuid();
      var idDef = new IdentifierDefinition
      {
        StoreId = 5,
        Alias = "newAlias",
        Value = "newValue",
        UID = uid
      };

      var deviceReferenceRecords = new List<DeviceReference>();
      _mockNhOp.Setup(o => o.CreateContext().DeviceReference.AddObject(It.IsAny<DeviceReference>()))
        .Callback<DeviceReference>(deviceReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges())
        .Throws(new UpdateException("An error occured while updating the entries. See the inner exception for details.",
          BuildSqlException("Violation of UNIQUE KEY constraint 'ck_Store_Alias_Value'. Cannot insert duplicate key in object 'dbo.DeviceReference'.",
          "The statement has been terminated.")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      AssertEx.Throws<CreatingDuplicateDeviceReferenceException>(() => storage.AddDeviceReference(idDef), "Device");
    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "db exception")]
    public void TestAddDeviceReference_AccessingDatabaseThrowsException()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      _mockNhOp.Setup(o => o.CreateContext().DeviceReference.AddObject(It.IsAny<DeviceReference>())).Throws((new Exception("db exception")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue",
        UID = new UUIDSequentialGuid().CreateGuid()
      };
      storage.AddDeviceReference(idDef);
    }

    [TestMethod]
    public void TestAddDeviceReference_NullIdentifierDefinition_DoesNotAddRowToDatabase()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var deviceReferenceRecords = new List<DeviceReference>();
      _mockNhOp.Setup(o => o.CreateContext().DeviceReference.AddObject(It.IsAny<DeviceReference>()))
        .Callback<DeviceReference>(deviceReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      storage.AddDeviceReference(null);
      _mockNhOp.Verify(o => o.CreateContext().DeviceReference.AddObject(It.IsAny<DeviceReference>()), Times.Never());
      _mockNhOp.Verify(o => o.CreateContext().SaveChanges(), Times.Never());
      Assert.AreEqual(0, deviceReferenceRecords.Count);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), It.IsAny<Guid>(), 1), Times.Never());
    }

    [TestMethod]
    public void TestGetAssociatedAsset_EmptyDeviceUid_ReturnNull()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var actual = storage.GetAssociatedAsset(Guid.Empty);
      Assert.IsNull(actual);
    }

    [TestMethod]
    public void TestGetAssociatedAsset_NH_OP_Exception_Throws_Exception()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      _mockNhOp.Setup(e => e.CreateContext()).Throws(new Exception("an exception"));
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      try
      {
        var actual = storage.GetAssociatedAsset(Guid.NewGuid());
        Assert.Fail();
      }
      catch (Exception) { }
    }

    [TestMethod]
    public void TestGetAssociatedAsset_EmptyDevice_ReturnsEmptyList()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      MockObjectSet<Device> deviceObject = new MockObjectSet<Device>();
      MockObjectSet<Asset> assetObject = new MockObjectSet<Asset>();
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      _mockNhOp.Setup(o => o.CreateContext().DeviceReadOnly).Returns(deviceObject);
      _mockNhOp.Setup(o => o.CreateContext().AssetReadOnly).Returns(assetObject);
      var actual = storage.GetAssociatedAsset(Guid.NewGuid());
      Assert.IsNull(actual);
    }

    [TestMethod]
    public void TestGetAssociatedAsset_DeviceUID_ReturnsCorrectAssetUid()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      MockObjectSet<Device> deviceObject = new MockObjectSet<Device>();
      Device device = new Device();
      device.ID = 1111111;
      device.DeviceUID = Guid.NewGuid();
      deviceObject.AddObject(device);
      MockObjectSet<Asset> assetObject = new MockObjectSet<Asset>();
      Asset asset = new Asset();
      asset.AssetUID = Guid.NewGuid();
      asset.AssetID = 1;
      asset.fk_DeviceID = device.ID;
      assetObject.AddObject(asset);
      _mockNhOp.Setup(o => o.CreateContext().DeviceReadOnly).Returns(deviceObject);
      _mockNhOp.Setup(o => o.CreateContext().AssetReadOnly).Returns(assetObject);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var actual = storage.GetAssociatedAsset(device.DeviceUID.Value);

      Assert.IsNotNull(actual);
      Assert.AreEqual(asset.AssetUID, actual);
    }

    #endregion

    #region ServiceReference Tests

    [TestMethod]
    public void TestGetServiceReference_ServiceReferenceNotInCache_ServiceReferenceIsInDatabase_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var mockServiceReferenceRecords = CreateServiceReferenceRecords();
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReferenceReadOnly).Returns(mockServiceReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      ServiceReference serviceReferenceRecord = mockServiceReferenceRecords.First(o => o.ID == 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = serviceReferenceRecord.fk_StoreID,
        Alias = serviceReferenceRecord.Alias,
        Value = serviceReferenceRecord.Value
      };
      Guid? actualUid = storage.FindServiceReference(idDef);
      Assert.AreEqual(serviceReferenceRecord.UID, actualUid);
      string cacheKey = string.Format("ServiceReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Once());
    }

    [TestMethod]
    public void TestGetServiceReference_ServiceReferenceNotInCache_ServiceReferenceHasDuplicates()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockServiceReferenceRecords = CreateServiceReferenceRecords();
      // set up duplicate Alias-Value pairs
      const long storeId = 1;
      mockServiceReferenceRecords.AddObject(GetServiceReferenceRecord(3, storeId, "ServiceRef", "dup56", new UUIDSequentialGuid().CreateGuid()));
      mockServiceReferenceRecords.AddObject(GetServiceReferenceRecord(4, storeId, "ServiceRef", "dup56", new UUIDSequentialGuid().CreateGuid()));
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReferenceReadOnly).Returns(mockServiceReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      ServiceReference serviceReferenceRecord = mockServiceReferenceRecords.First(o => o.Alias == "ServiceRef" && o.Value == "dup56");
      var idDef = new IdentifierDefinition
      {
        StoreId = serviceReferenceRecord.fk_StoreID,
        Alias = serviceReferenceRecord.Alias,
        Value = serviceReferenceRecord.Value
      };
      AssertEx.Throws<DuplicateServiceReferenceFoundException>(() => storage.FindServiceReference(idDef), "Subscription");
    }


    [TestMethod]
    public void TestGetServiceReference_ServiceReferenceNotInCache_ServiceReferenceIsNotInDatabase_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockServiceReferenceRecords = CreateServiceReferenceRecords();
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReferenceReadOnly).Returns(mockServiceReferenceRecords);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue"
      };
      Guid? actualUid = storage.FindServiceReference(idDef);
      Assert.IsNull(actualUid);
      string cacheKey = string.Format("ServiceReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Once());

    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "db exception")]
    public void TestGetServiceReference_ServiceReferenceNotInCache_AccessingDatabaseThrowsException()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReferenceReadOnly).Throws((new Exception("db exception")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue"
      };
      storage.FindServiceReference(idDef);
    }

    [TestMethod]
    public void TestGetServiceReference_ServiceReferenceIsInCache_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockServiceReferenceRecords = CreateServiceReferenceRecords();
      ServiceReference serviceReferenceRecord = mockServiceReferenceRecords.First(o => o.ID == 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = serviceReferenceRecord.fk_StoreID,
        Alias = serviceReferenceRecord.Alias,
        Value = serviceReferenceRecord.Value
      };
      string cacheKey = string.Format("ServiceReference.{0}.{1}.{2}", idDef.StoreId, idDef.Alias, idDef.Value);
      _mockCacheManager.Setup(o => o.GetData(cacheKey)).Returns(serviceReferenceRecord.UID);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      Guid? actualUid = storage.FindServiceReference(idDef);
      Assert.AreEqual(serviceReferenceRecord.UID, actualUid);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actualUid, 1), Times.Never());
    }

    [TestMethod]
    public void TestGetServiceReference_NullIdentifierDefinition_ReturnsNull()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      Guid? actualUid = storage.FindServiceReference(null);
      Assert.IsNull(actualUid);
    }

    [TestMethod]
    public void TestAddServiceReference()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      Guid uid = new UUIDSequentialGuid().CreateGuid();
      var idDef = new IdentifierDefinition
      {
        StoreId = 5,
        Alias = "newAlias",
        Value = "newValue",
        UID = uid
      };

      var serviceReferenceRecords = new List<ServiceReference>();
      _mockNhOp.Setup(o => o.CreateContext().ServiceReference.AddObject(It.IsAny<ServiceReference>()))
        .Callback<ServiceReference>(serviceReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      storage.AddServiceReference(idDef);
      _mockNhOp.Verify(o => o.CreateContext().ServiceReference.AddObject(It.IsAny<ServiceReference>()), Times.Once());
      _mockNhOp.Verify(o => o.CreateContext().SaveChanges(), Times.Once());
      Assert.AreEqual(1, serviceReferenceRecords.Count);
      ServiceReference addedServiceReference = serviceReferenceRecords.First();
      Assert.AreEqual(idDef.StoreId, addedServiceReference.fk_StoreID);
      Assert.AreEqual(idDef.Value, addedServiceReference.Value);
      Assert.AreEqual(idDef.Alias, addedServiceReference.Alias);
      Assert.AreEqual(idDef.UID, addedServiceReference.UID);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), addedServiceReference.UID, 1), Times.Once());
    }

    [TestMethod]
    public void TestAddServiceReference_Duplicate_Failure()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      Guid uid = new UUIDSequentialGuid().CreateGuid();
      var idDef = new IdentifierDefinition
      {
        StoreId = 5,
        Alias = "newAlias",
        Value = "newValue",
        UID = uid
      };

      var serviceReferenceRecords = new List<ServiceReference>();
      _mockNhOp.Setup(o => o.CreateContext().ServiceReference.AddObject(It.IsAny<ServiceReference>()))
        .Callback<ServiceReference>(serviceReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges())
        .Throws(new UpdateException("An error occured while updating the entries. See the inner exception for details.",
          BuildSqlException("Violation of UNIQUE KEY constraint 'ck_Store_Alias_Value'. Cannot insert duplicate key in object 'dbo.ServiceReference'.",
          "The statement has been terminated.")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      AssertEx.Throws<CreatingDuplicateServiceReferenceException>(() => storage.AddServiceReference(idDef), "Subscription");
    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "db exception")]
    public void TestAddServiceReference_AccessingDatabaseThrowsException()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      _mockNhOp.Setup(o => o.CreateContext().ServiceReference.AddObject(It.IsAny<ServiceReference>())).Throws((new Exception("db exception")));
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var idDef = new IdentifierDefinition
      {
        StoreId = 10,
        Alias = "anAlias",
        Value = "aValue",
        UID = new UUIDSequentialGuid().CreateGuid()
      };
      storage.AddServiceReference(idDef);
    }

    [TestMethod]
    public void TestAddServiceReference_NullIdentifierDefinition_DoesNotAddRowToDatabase()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var serviceReferenceRecords = new List<ServiceReference>();
      _mockNhOp.Setup(o => o.CreateContext().ServiceReference.AddObject(It.IsAny<ServiceReference>()))
        .Callback<ServiceReference>(serviceReferenceRecords.Add);
      _mockNhOp.Setup(o => o.CreateContext().SaveChanges()).Returns(1);
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      storage.AddServiceReference(null);
      _mockNhOp.Verify(o => o.CreateContext().ServiceReference.AddObject(It.IsAny<ServiceReference>()), Times.Never());
      _mockNhOp.Verify(o => o.CreateContext().SaveChanges(), Times.Never());
      Assert.AreEqual(0, serviceReferenceRecords.Count);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), It.IsAny<Guid>(), 1), Times.Never());
    }

    [TestMethod]
    public void TestGetDeviceActiveServices_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      const DeviceTypeEnum deviceType = DeviceTypeEnum.PL631;
      const string deviceSerialNumber = "deviceSerialNumber";
      const int deviceId = 42;

      var service = new VSS.Hosted.VLCommon.Service
      {
        CancellationKeyDate = 99991231,
        fk_DeviceID = deviceId,
        fk_ServiceTypeID = (int)ServiceTypeEnum.Essentials,
        ServiceUID = Guid.NewGuid()
      };

      var device = new Device
      {
        ID = deviceId,
        fk_DeviceTypeID = (int)deviceType,
        GpsDeviceID = deviceSerialNumber
      };

      var serviceType = new ServiceType
      {
        ID = (int)ServiceTypeEnum.Essentials,
        Name = ServiceTypeEnum.Essentials.ToString()
      };

      var services = new MockObjectSet<VSS.Hosted.VLCommon.Service>();
      services.AddObject(service);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReadOnly).Returns(services);

      var devices = new MockObjectSet<Device>();
      devices.AddObject(device);
      _mockNhOp.SetupGet(o => o.CreateContext().DeviceReadOnly).Returns(devices);

      var serviceTypes = new MockObjectSet<ServiceType>();
      serviceTypes.AddObject(serviceType);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceTypeReadOnly).Returns(serviceTypes);

      var result = storage.GetDeviceActiveServices(deviceSerialNumber, deviceType);

      Assert.AreEqual(1, result.Count);
      var serviceLookupItem = result.First();
      Assert.AreEqual("Essentials", serviceLookupItem.Type);
      Assert.AreEqual(service.ServiceUID, serviceLookupItem.UID);
      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceTypeReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().DeviceReadOnly, Times.Once());
    }

    [TestMethod]
    public void TestGetDeviceActiveServices_CancellationDateInFuture_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      const DeviceTypeEnum deviceType = DeviceTypeEnum.PL631;
      const string deviceSerialNumber = "deviceSerialNumber";
      const int deviceId = 42;

      var service = new VSS.Hosted.VLCommon.Service
      {
        CancellationKeyDate = DateTime.UtcNow.AddDays(2).KeyDate(),
        fk_DeviceID = deviceId,
        fk_ServiceTypeID = (int)ServiceTypeEnum.Essentials,
        ServiceUID = Guid.NewGuid()
      };

      var device = new Device
      {
        ID = deviceId,
        fk_DeviceTypeID = (int)deviceType,
        GpsDeviceID = deviceSerialNumber
      };

      var serviceType = new ServiceType
      {
        ID = (int)ServiceTypeEnum.Essentials,
        Name = ServiceTypeEnum.Essentials.ToString()
      };

      var services = new MockObjectSet<VSS.Hosted.VLCommon.Service>();
      services.AddObject(service);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReadOnly).Returns(services);

      var devices = new MockObjectSet<Device>();
      devices.AddObject(device);
      _mockNhOp.SetupGet(o => o.CreateContext().DeviceReadOnly).Returns(devices);

      var serviceTypes = new MockObjectSet<ServiceType>();
      serviceTypes.AddObject(serviceType);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceTypeReadOnly).Returns(serviceTypes);

      var result = storage.GetDeviceActiveServices(deviceSerialNumber, deviceType);

      Assert.AreEqual(1, result.Count);
      var serviceLookupItem = result.First();
      Assert.AreEqual("Essentials", serviceLookupItem.Type);
      Assert.AreEqual(service.ServiceUID, serviceLookupItem.UID);

      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceTypeReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().DeviceReadOnly, Times.Once());
    }

    [TestMethod]
    public void TestGetDeviceActiveServices_NoServicesFound()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      const DeviceTypeEnum deviceType = DeviceTypeEnum.PL631;
      const string deviceSerialNumber = "deviceSerialNumber";

      var service = new VSS.Hosted.VLCommon.Service
      {
        CancellationKeyDate = 99991231,
        fk_DeviceID = 0, // this won't match the Device's ID
        fk_ServiceTypeID = (int)ServiceTypeEnum.Essentials,
        ServiceUID = Guid.NewGuid()
      };

      var device = new Device
      {
        ID = 1, // this won't match the Service's fk_DeviceID
        fk_DeviceTypeID = (int)deviceType,
        GpsDeviceID = deviceSerialNumber
      };

      var serviceType = new ServiceType
      {
        ID = (int)ServiceTypeEnum.Essentials,
        Name = ServiceTypeEnum.Essentials.ToString()
      };

      var services = new MockObjectSet<VSS.Hosted.VLCommon.Service>();
      services.AddObject(service);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReadOnly).Returns(services);

      var devices = new MockObjectSet<Device>();
      devices.AddObject(device);
      _mockNhOp.SetupGet(o => o.CreateContext().DeviceReadOnly).Returns(devices);

      var serviceTypes = new MockObjectSet<ServiceType>();
      serviceTypes.AddObject(serviceType);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceTypeReadOnly).Returns(serviceTypes);

      var result = storage.GetDeviceActiveServices(deviceSerialNumber, deviceType);

      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count);
      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceTypeReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().DeviceReadOnly, Times.Once());
    }

    [TestMethod]
    public void TestGetDeviceActiveServices_NullSerialNumber()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var result = storage.GetDeviceActiveServices(null, DeviceTypeEnum.PL631);
      Assert.IsNull(result);
    }

    [TestMethod]
    public void TestGetDeviceActiveServices_EmptySerialNumber()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var result = storage.GetDeviceActiveServices(string.Empty, DeviceTypeEnum.PL631);
      Assert.IsNull(result);
    }

    [TestMethod]
    public void TestGetAssetActiveServices_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      const DeviceTypeEnum deviceType = DeviceTypeEnum.PL631;
      const string deviceSerialNumber = "deviceSerialNumber";
      const int deviceId = 42;
      const string assetSerialNumber = "assetSerialNumber";
      const string assetMakeCode = "CAT";

      var service = new VSS.Hosted.VLCommon.Service
      {
        CancellationKeyDate = 99991231,
        fk_DeviceID = deviceId,
        fk_ServiceTypeID = (int)ServiceTypeEnum.Essentials,
        ServiceUID = Guid.NewGuid()
      };

      var device = new Device
      {
        ID = deviceId,
        fk_DeviceTypeID = (int)deviceType,
        GpsDeviceID = deviceSerialNumber
      };

      var serviceType = new ServiceType
      {
        ID = (int)ServiceTypeEnum.Essentials,
        Name = ServiceTypeEnum.Essentials.ToString()
      };

      var asset = new Asset
      {
        SerialNumberVIN = assetSerialNumber,
        fk_MakeCode = assetMakeCode,
        fk_DeviceID = deviceId
      };

      var services = new MockObjectSet<VSS.Hosted.VLCommon.Service>();
      services.AddObject(service);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReadOnly).Returns(services);

      var devices = new MockObjectSet<Device>();
      devices.AddObject(device);
      _mockNhOp.SetupGet(o => o.CreateContext().DeviceReadOnly).Returns(devices);

      var serviceTypes = new MockObjectSet<ServiceType>();
      serviceTypes.AddObject(serviceType);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceTypeReadOnly).Returns(serviceTypes);

      var assets = new MockObjectSet<Asset>();
      assets.AddObject(asset);
      _mockNhOp.SetupGet(o => o.CreateContext().AssetReadOnly).Returns(assets);

      var result = storage.GetAssetActiveServices(assetSerialNumber, assetMakeCode);

      Assert.AreEqual(1, result.Count);
      var serviceLookupItem = result.First();
      Assert.AreEqual("Essentials", serviceLookupItem.Type);
      Assert.AreEqual(service.ServiceUID, serviceLookupItem.UID);

      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceTypeReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().DeviceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().AssetReadOnly, Times.Once());
    }

    [TestMethod]
    public void TestGetAssetActiveServices_CancellationDateInFuture_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      const DeviceTypeEnum deviceType = DeviceTypeEnum.PL631;
      const string deviceSerialNumber = "deviceSerialNumber";
      const int deviceId = 42;
      const string assetSerialNumber = "assetSerialNumber";
      const string assetMakeCode = "CAT";

      var service = new VSS.Hosted.VLCommon.Service
      {
        CancellationKeyDate = DateTime.UtcNow.AddDays(2).KeyDate(),
        fk_DeviceID = deviceId,
        fk_ServiceTypeID = (int)ServiceTypeEnum.Essentials,
        ServiceUID = Guid.NewGuid()
      };

      var device = new Device
      {
        ID = deviceId,
        fk_DeviceTypeID = (int)deviceType,
        GpsDeviceID = deviceSerialNumber
      };

      var serviceType = new ServiceType
      {
        ID = (int)ServiceTypeEnum.Essentials,
        Name = ServiceTypeEnum.Essentials.ToString()
      };

      var asset = new Asset
      {
        SerialNumberVIN = assetSerialNumber,
        fk_MakeCode = assetMakeCode,
        fk_DeviceID = deviceId
      };

      var services = new MockObjectSet<VSS.Hosted.VLCommon.Service>();
      services.AddObject(service);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReadOnly).Returns(services);

      var devices = new MockObjectSet<Device>();
      devices.AddObject(device);
      _mockNhOp.SetupGet(o => o.CreateContext().DeviceReadOnly).Returns(devices);

      var serviceTypes = new MockObjectSet<ServiceType>();
      serviceTypes.AddObject(serviceType);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceTypeReadOnly).Returns(serviceTypes);

      var assets = new MockObjectSet<Asset>();
      assets.AddObject(asset);
      _mockNhOp.SetupGet(o => o.CreateContext().AssetReadOnly).Returns(assets);

      var result = storage.GetAssetActiveServices(assetSerialNumber, assetMakeCode);

      Assert.AreEqual(1, result.Count);
      var serviceLookupItem = result.First();
      Assert.AreEqual("Essentials", serviceLookupItem.Type);
      Assert.AreEqual(service.ServiceUID, serviceLookupItem.UID);

      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceTypeReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().DeviceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().AssetReadOnly, Times.Once());
    }

    [TestMethod]
    public void TestGetAssetActiveServices_NoServicesFound()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      const DeviceTypeEnum deviceType = DeviceTypeEnum.PL631;
      const string deviceSerialNumber = "deviceSerialNumber";
      const string assetSerialNumber = "assetSerialNumber";
      const string assetMakeCode = "CAT";

      var service = new VSS.Hosted.VLCommon.Service
      {
        CancellationKeyDate = 99991231,
        fk_DeviceID = 0, // this won't match the Device's ID
        fk_ServiceTypeID = (int)ServiceTypeEnum.Essentials,
        ServiceUID = Guid.NewGuid()
      };

      var device = new Device
      {
        ID = 1, // this won't match the Service's fk_DeviceID
        fk_DeviceTypeID = (int)deviceType,
        GpsDeviceID = deviceSerialNumber
      };

      var serviceType = new ServiceType
      {
        ID = (int)ServiceTypeEnum.Essentials,
        Name = ServiceTypeEnum.Essentials.ToString()
      };

      var asset = new Asset
      {
        SerialNumberVIN = assetSerialNumber,
        fk_MakeCode = assetMakeCode,
        fk_DeviceID = 2 // this won't match the Device's ID
      };

      var services = new MockObjectSet<VSS.Hosted.VLCommon.Service>();
      services.AddObject(service);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReadOnly).Returns(services);

      var devices = new MockObjectSet<Device>();
      devices.AddObject(device);
      _mockNhOp.SetupGet(o => o.CreateContext().DeviceReadOnly).Returns(devices);

      var serviceTypes = new MockObjectSet<ServiceType>();
      serviceTypes.AddObject(serviceType);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceTypeReadOnly).Returns(serviceTypes);

      var assets = new MockObjectSet<Asset>();
      assets.AddObject(asset);
      _mockNhOp.SetupGet(o => o.CreateContext().AssetReadOnly).Returns(assets);

      var result = storage.GetAssetActiveServices(assetSerialNumber, assetMakeCode);

      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count);
      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceTypeReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().DeviceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().AssetReadOnly, Times.Once());
    }

    [TestMethod]
    public void TestGetAssetActiveServices_NullSerialNumber()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var result = storage.GetAssetActiveServices(null, "CAT");
      Assert.IsNull(result);
    }

    [TestMethod]
    public void TestGetAssetActiveServices_EmptySerialNumber()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      var result = storage.GetAssetActiveServices(string.Empty, "CAT");
      Assert.IsNull(result);
    }

    [TestMethod]
    public void TestGetAssetActiveServicesAssetUid_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      const int deviceId = 42;
      Guid assetUid = Guid.NewGuid();
      Guid serviceUid = Guid.NewGuid();

      var service = new VSS.Hosted.VLCommon.Service
      {
        CancellationKeyDate = DateTime.UtcNow.AddDays(1).KeyDate(),
        fk_DeviceID = deviceId,
        ServiceUID = serviceUid
      };

      var asset = new Asset
      {
        fk_DeviceID = deviceId,
        AssetUID = assetUid
      };

      var services = new MockObjectSet<VSS.Hosted.VLCommon.Service>();
      services.AddObject(service);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReadOnly).Returns(services);

      var assets = new MockObjectSet<Asset>();
      assets.AddObject(asset);
      _mockNhOp.SetupGet(o => o.CreateContext().AssetReadOnly).Returns(assets);

      var result = storage.GetAssetActiveServices(assetUid);

      Assert.AreEqual(1, result.Count);
      var serviceLookupItem = result.First();
      Assert.IsTrue(serviceLookupItem.HasValue);
      Assert.AreEqual(serviceUid, serviceLookupItem.Value);

      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().AssetReadOnly, Times.Once());
    }

    [TestMethod]
    public void TestGetAssetActiveServicesAssetUid_ServicesCancelled()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      const int deviceId = 42;
      Guid assetUid = Guid.NewGuid();
      Guid serviceUid = Guid.NewGuid();

      var service = new VSS.Hosted.VLCommon.Service
      {
        CancellationKeyDate = DateTime.UtcNow.AddDays(-1).KeyDate(),
        fk_DeviceID = deviceId,
        ServiceUID = serviceUid
      };

      var asset = new Asset
      {
        fk_DeviceID = deviceId,
        AssetUID = assetUid
      };

      var services = new MockObjectSet<VSS.Hosted.VLCommon.Service>();
      services.AddObject(service);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceReadOnly).Returns(services);

      var assets = new MockObjectSet<Asset>();
      assets.AddObject(asset);
      _mockNhOp.SetupGet(o => o.CreateContext().AssetReadOnly).Returns(assets);

      var result = storage.GetAssetActiveServices(assetUid);

      Assert.AreEqual(0, result.Count);

      _mockNhOp.VerifyGet(o => o.CreateContext().ServiceReadOnly, Times.Once());
      _mockNhOp.VerifyGet(o => o.CreateContext().AssetReadOnly, Times.Once());
    }
    #endregion

    #region Credential Tests
    [TestMethod]
    public void TestFindCredentialsForUrl_CredentialsNotInCache_ServiceProviderIsInDatabaseSameMatch_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
    
      var mockServiceProviderRecords = CreateServiceProviderRecords();
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceProviderReadOnly).Returns(mockServiceProviderRecords);
      
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      ServiceProvider serviceProviderRecord = mockServiceProviderRecords.First(o => o.ID == 1);
      _mockStringEncryptor.Setup(o => o.EncryptStringToBytes(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(Encoding.ASCII.GetBytes(serviceProviderRecord.Password));
      
      Credentials actual = storage.FindCredentialsForUrl(serviceProviderRecord.ServerIPAddress);
      _mockStringEncryptor.Verify(o => o.EncryptStringToBytes(serviceProviderRecord.Password, It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once());
      Assert.AreEqual(serviceProviderRecord.UserName, actual.UserName);
      Assert.AreEqual(Convert.ToBase64String(Encoding.ASCII.GetBytes(serviceProviderRecord.Password)), actual.EncryptedPassword);
      string cacheKey = string.Format("Credentials.{0}", serviceProviderRecord.ServerIPAddress);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actual, 1), Times.Once());
    }

    [TestMethod]
    public void TestFindCredentialsForUrl_CredentialsNotInCache_ServiceProviderIsInDatabaseCloseMatch_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockServiceProviderRecords = CreateServiceProviderRecords();
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceProviderReadOnly).Returns(mockServiceProviderRecords);

      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      ServiceProvider serviceProviderRecord = mockServiceProviderRecords.First(o => o.ID == 1);
      _mockStringEncryptor.Setup(o => o.EncryptStringToBytes(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(Encoding.ASCII.GetBytes(serviceProviderRecord.Password));
      
      Credentials actual = storage.FindCredentialsForUrl(serviceProviderRecord.ServerIPAddress + "/TEST/ABC");
      _mockStringEncryptor.Verify(o => o.EncryptStringToBytes(serviceProviderRecord.Password, It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once());
      Assert.AreEqual(serviceProviderRecord.UserName, actual.UserName);
      Assert.AreEqual(Convert.ToBase64String(Encoding.ASCII.GetBytes(serviceProviderRecord.Password)), actual.EncryptedPassword);
      string cacheKey = string.Format("Credentials.{0}", serviceProviderRecord.ServerIPAddress);
      _mockCacheManager.Verify(o => o.Add(cacheKey, actual, 1), Times.Once());
    }

    [TestMethod]
    public void TestFindCredentialsForUrl_CredentialsInCache_Success()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();

      var mockServiceProviderRecords = CreateServiceProviderRecords();
      _mockCacheManager.Setup(o => o.GetClosestData(It.IsAny<string>())).Returns(new Credentials { UserName = "User", EncryptedPassword = "Password" });
      _mockNhOp.SetupGet(o => o.CreateContext().ServiceProviderReadOnly).Returns(mockServiceProviderRecords);

      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      ServiceProvider serviceProviderRecord = mockServiceProviderRecords.First(o => o.ID == 1);
      _mockStringEncryptor.Setup(o => o.EncryptStringToBytes(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(Encoding.ASCII.GetBytes(serviceProviderRecord.Password));

      Credentials actual = storage.FindCredentialsForUrl(serviceProviderRecord.ServerIPAddress + "/TEST/ABC");
      _mockStringEncryptor.Verify(o => o.EncryptStringToBytes(serviceProviderRecord.Password, It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Never());
      Assert.AreEqual("User", actual.UserName);
      Assert.AreEqual("Password", actual.EncryptedPassword);
      string cacheKey = string.Format("Credentials.{0}", serviceProviderRecord.ServerIPAddress + "/TEST/ABC");
      _mockCacheManager.Verify(o => o.GetClosestData(cacheKey), Times.Once());
    }

    [TestMethod]
    public void TestFindCredentialsForUrl_CredentialsIsNull_ReturnsNull()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);
      Credentials actual = storage.FindCredentialsForUrl(null);
      Assert.IsNull(actual);
    }

    [TestMethod]
    public void TestFindCredentialsForUrl_ThrowsException()
    {
      Mock<INHOpContextFactory> _mockNhOp = new Mock<INHOpContextFactory>();
      _mockNhOp.Setup(e => e.CreateContext()).Throws(new Exception("an exception"));
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      Mock<IStringEncryptor> _mockStringEncryptor = new Mock<IStringEncryptor>();
      IStorage storage = new Storage(_mockNhOp.Object, _mockCacheManager.Object, _mockStringEncryptor.Object, 1, 1, 1, 1);

      try
      {
        storage.FindCredentialsForUrl("/TEST/ABC");
        Assert.Fail();
      }
      catch(Exception)
      { }
    }

    private MockObjectSet<ServiceProvider> CreateServiceProviderRecords()
    {
      MockObjectSet<ServiceProvider> mockServiceProviderRecords = new MockObjectSet<ServiceProvider>();
      mockServiceProviderRecords.AddObject(GetServiceProviderRecord());
      mockServiceProviderRecords.AddObject(GetServiceProviderRecord(id:2, customerName:"CAT", password:"CATPass", providerName:"CATProvider", userName:"CATUser", serverIPAddress:"https://Local"));
      return mockServiceProviderRecords;
    }

    private ServiceProvider GetServiceProviderRecord(long id = 1, string customerName = "Test", 
      string messageContentType = "application/xml", string password = "password", 
      string providerName = "StoreAPI_provider", string serverIPAddress = "http://Test.com/test", 
      string sslSubjectName = null, string sslThumbPrint = null, string userName = "User")
    {
      return new ServiceProvider
      {
        ID = id,
        CustomerName = customerName,
        MessageContentType = messageContentType,
        Password = password,
        ProviderName = providerName,
        ServerIPAddress = serverIPAddress,
        SSLSubjectName = sslSubjectName,
        SSLThumbPrint = sslThumbPrint,
        UpdateUTC = DateTime.UtcNow,
        UserName = userName
      };
    }
    #endregion

    #region CustomerLookup

    [TestMethod]
    public void FindStoreByCustomerID()
    {
      var mockInhOp = new Mock<INH_OP>();
      var mockCacheManager = new Mock<ICacheManager>();
      var mockStringEncryptor = new Mock<IStringEncryptor>();
      var mockInhOpContextFactory = new Mock<INHOpContextFactory>();
      mockInhOpContextFactory
        .Setup(o => o.CreateContext())
        .Returns(mockInhOp.Object);

      var mockCustomerStoreReadOnlyObjectSet = new MockObjectSet<Hosted.VLCommon.CustomerStore>();
      mockCustomerStoreReadOnlyObjectSet.AddObject(new Hosted.VLCommon.CustomerStore
      {
        ID = 1234,
        fk_StoreID = 1,
        fk_CustomerID = 2
      });
      mockInhOp.Setup(e => e.CustomerStoreReadOnly).Returns(mockCustomerStoreReadOnlyObjectSet);
      var storage = new Storage(mockInhOpContextFactory.Object, mockCacheManager.Object, mockStringEncryptor.Object, 0, 0, 0, 0);
      var storeid = storage.FindStoreByCustomerId(2);
      Assert.AreEqual(1, storeid);
    }

    [TestMethod]
    public void FindCustomerGuidByCustomerId()
    {
      var mockInhOp = new Mock<INH_OP>();
      var mockCacheManager = new Mock<ICacheManager>();
      var mockStringEncryptor = new Mock<IStringEncryptor>();
      var mockInhOpContextFactory = new Mock<INHOpContextFactory>();
      mockInhOpContextFactory
        .Setup(o => o.CreateContext())
        .Returns(mockInhOp.Object);
      var guid = Guid.NewGuid();
      var mockCustomerReadOnlyObjectSet = new MockObjectSet<Hosted.VLCommon.Customer>();
      mockCustomerReadOnlyObjectSet.AddObject(new Hosted.VLCommon.Customer
      {
        ID = 1,
        CustomerUID = guid
      });
      mockInhOp.Setup(e => e.CustomerReadOnly).Returns(mockCustomerReadOnlyObjectSet);
      var storage = new Storage(mockInhOpContextFactory.Object, mockCacheManager.Object, mockStringEncryptor.Object, 0, 0, 0, 0);
      var customerUID = storage.FindCustomerGuidByCustomerId(1);
      Assert.AreEqual(guid, customerUID);
    }

    [TestMethod]
    public void FindOemIdentifierByCustomerId()
    {
      var mockInhOp = new Mock<INH_OP>();
      var mockCacheManager = new Mock<ICacheManager>();
      var mockStringEncryptor = new Mock<IStringEncryptor>();
      var mockInhOpContextFactory = new Mock<INHOpContextFactory>();
      mockInhOpContextFactory
        .Setup(o => o.CreateContext())
        .Returns(mockInhOp.Object);
      var guid = Guid.NewGuid();
      var mockCustomerReadOnlyObjectSet = new MockObjectSet<Hosted.VLCommon.Customer>();
      mockCustomerReadOnlyObjectSet.AddObject(new Hosted.VLCommon.Customer
      {
        ID = 1,
        CustomerUID = guid,
        fk_DealerNetworkID = (int)DealerNetworkEnum.LEEBOY
      });
      mockInhOp.Setup(e => e.CustomerReadOnly).Returns(mockCustomerReadOnlyObjectSet);
      var storage = new Storage(mockInhOpContextFactory.Object, mockCacheManager.Object, mockStringEncryptor.Object, 0, 0, 0, 0);
      var dealerNetworkID = storage.FindOemIdentifierByCustomerId(1);
      Assert.AreEqual((int)DealerNetworkEnum.LEEBOY, dealerNetworkID);
    }

    [TestMethod]
    public void FindDealers()
    {
      var mockInhOp = new Mock<INH_OP>();
      var mockCacheManager = new Mock<ICacheManager>();
      var mockStringEncryptor = new Mock<IStringEncryptor>();
      var mockInhOpContextFactory = new Mock<INHOpContextFactory>();
      mockInhOpContextFactory
        .Setup(o => o.CreateContext())
        .Returns(mockInhOp.Object);
      var guid = Guid.NewGuid();
      var mockCustomerReadOnlyObjectSet = new MockObjectSet<Hosted.VLCommon.Customer>();
      mockCustomerReadOnlyObjectSet.AddObject(new Hosted.VLCommon.Customer
      {
        ID = 2,
        CustomerUID = guid,
        fk_DealerNetworkID = (int)DealerNetworkEnum.LEEBOY
      });
      mockInhOp.Setup(e => e.CustomerReadOnly).Returns(mockCustomerReadOnlyObjectSet);
      var mockCustomerReferenceReadOnlyObjectSet = new MockObjectSet<Hosted.VLCommon.CustomerReference>();
      mockCustomerReferenceReadOnlyObjectSet.AddObject(new Hosted.VLCommon.CustomerReference { ID = 1, Alias = "2", Value = "3", UID = guid, fk_StoreID = 1 });
      var mockCustomerStoreReadOnlyObjectSet = new MockObjectSet<Hosted.VLCommon.CustomerStore>();
      mockCustomerStoreReadOnlyObjectSet.AddObject(new Hosted.VLCommon.CustomerStore
      {
        ID = 1234,
        fk_StoreID = 1,
        fk_CustomerID = 2
      });
      mockInhOp.Setup(e => e.CustomerReadOnly).Returns(mockCustomerReadOnlyObjectSet);
      mockInhOp.Setup(e => e.CustomerStoreReadOnly).Returns(mockCustomerStoreReadOnlyObjectSet);
      mockInhOp.Setup(e => e.CustomerReferenceReadOnly).Returns(mockCustomerReferenceReadOnlyObjectSet);
      IList<IdentifierDefinition> dealers = new List<IdentifierDefinition> { new IdentifierDefinition { Alias = "2", Value = "3" }};
      var storage = new Storage(mockInhOpContextFactory.Object, mockCacheManager.Object, mockStringEncryptor.Object, 0, 0, 0, 0);
      var deal = storage.FindDealers(dealers, 1);
      Assert.AreEqual(1, deal.Count);
      Assert.AreEqual("3", deal.FirstOrDefault().Value);
    }

    [TestMethod]
    public void TestFindAllCustomersForService()
    {
      var mockInhOp = new Mock<INH_OP>();
      var mockCacheManager = new Mock<ICacheManager>();
      var mockStringEncryptor = new Mock<IStringEncryptor>();
      var mockInhOpContextFactory = new Mock<INHOpContextFactory>();
      mockInhOpContextFactory
        .Setup(o => o.CreateContext())
        .Returns(mockInhOp.Object);

      var mockSvcUid = Guid.NewGuid();
      var mockServiceReadOnlyObjectSet = new MockObjectSet<Hosted.VLCommon.Service>();
      mockServiceReadOnlyObjectSet.AddObject(new Hosted.VLCommon.Service
      {
        ServiceUID = mockSvcUid,
        ID = 1234
      });
      mockInhOp
        .SetupGet(o => o.ServiceReadOnly)
        .Returns(mockServiceReadOnlyObjectSet);
      var mockServiceViewReadOnlyObjectSet = new MockObjectSet<ServiceView>();
      mockServiceViewReadOnlyObjectSet.AddObject(new ServiceView
      {
        fk_ServiceID = 1234,
        EndKeyDate = 99991231,
        ID = 1,
        fk_CustomerID = 23
      });
      mockServiceViewReadOnlyObjectSet.AddObject(new ServiceView
      {
        fk_ServiceID = 1234,
        EndKeyDate = 99991231,
        ID = 2,
        fk_CustomerID = 24
      });
      mockInhOp
        .SetupGet(o => o.ServiceViewReadOnly)
        .Returns(mockServiceViewReadOnlyObjectSet);
      var mockCustomerReadOnlyObjectSet = new MockObjectSet<Customer>();
      mockCustomerReadOnlyObjectSet.AddObject(new Customer
      {
        ID = 23,
        CustomerUID = Guid.Parse("CF1BCAED-B960-4764-9F09-CD1E13403204")
      });
      mockCustomerReadOnlyObjectSet.AddObject(new Customer
      {
        ID = 24,
        CustomerUID = Guid.Parse("21B3CE88-E63F-414B-878E-B068010512F4")
      });
      mockInhOp
        .SetupGet(o => o.CustomerReadOnly)
        .Returns(mockCustomerReadOnlyObjectSet);

      var storage = new Storage(mockInhOpContextFactory.Object, mockCacheManager.Object, mockStringEncryptor.Object, 0, 0, 0, 0);

      var customerUids = storage.FindAllCustomersForService(mockSvcUid);

      CollectionAssert.AreEquivalent(new []
        {
          Guid.Parse("CF1BCAED-B960-4764-9F09-CD1E13403204"),
          Guid.Parse("21B3CE88-E63F-414B-878E-B068010512F4")
        },
        customerUids
      );
    }

    [TestMethod]
    public void TestFindCustomerParent_ParentTypeDealer()
    {
      const int childId = 42;
      const int parentId = 43;
      Guid childUid = Guid.NewGuid();
      Guid parentUid = Guid.NewGuid();

      var mockINhOp = new Mock<INH_OP>();
      var mockCacheManager = new Mock<ICacheManager>();
      var mockStringEncryptor = new Mock<IStringEncryptor>();
      var mockInhOpContextFactory = new Mock<INHOpContextFactory>();

      mockInhOpContextFactory.Setup(o => o.CreateContext()).Returns(mockINhOp.Object);

      var mockcustomerReadOnlyObjectSet = new MockObjectSet<Customer>();
      mockcustomerReadOnlyObjectSet.AddObject(new Customer
      {
        ID = childId,
        CustomerUID = childUid
      });
      mockcustomerReadOnlyObjectSet.AddObject(new Customer
      {
        ID = parentId,
        CustomerUID = parentUid
      });
      mockINhOp.SetupGet(o => o.CustomerReadOnly).Returns(mockcustomerReadOnlyObjectSet);

      var mockcustomerRelationshipReadOnlyObjectSet = new MockObjectSet<CustomerRelationship>();
      mockcustomerRelationshipReadOnlyObjectSet.AddObject(new CustomerRelationship
      {
        fk_ClientCustomerID = childId,
        fk_CustomerRelationshipTypeID = (int)CustomerRelationshipTypeEnum.TCSDealer,
        fk_ParentCustomerID = parentId
      });
      mockINhOp.SetupGet(o => o.CustomerRelationshipReadOnly).Returns(mockcustomerRelationshipReadOnlyObjectSet);

      var storage = new Storage(mockInhOpContextFactory.Object, mockCacheManager.Object, mockStringEncryptor.Object, 0, 0, 0, 0);

      var result = storage.FindCustomerParent(childUid, CustomerTypeEnum.Dealer);
      Assert.AreEqual(parentUid, result);
    }

    [TestMethod]
    public void TestFindCustomerParent_ParentTypeCustomer()
    {
      const int childId = 42;
      const int parentId = 43;
      Guid childUid = Guid.NewGuid();
      Guid parentUid = Guid.NewGuid();

      var mockINhOp = new Mock<INH_OP>();
      var mockCacheManager = new Mock<ICacheManager>();
      var mockStringEncryptor = new Mock<IStringEncryptor>();
      var mockInhOpContextFactory = new Mock<INHOpContextFactory>();

      mockInhOpContextFactory.Setup(o => o.CreateContext()).Returns(mockINhOp.Object);

      var mockcustomerReadOnlyObjectSet = new MockObjectSet<Customer>();
      mockcustomerReadOnlyObjectSet.AddObject(new Customer
      {
        ID = childId,
        CustomerUID = childUid
      });
      mockcustomerReadOnlyObjectSet.AddObject(new Customer
      {
        ID = parentId,
        CustomerUID = parentUid
      });
      mockINhOp.SetupGet(o => o.CustomerReadOnly).Returns(mockcustomerReadOnlyObjectSet);

      var mockcustomerRelationshipReadOnlyObjectSet = new MockObjectSet<CustomerRelationship>();
      mockcustomerRelationshipReadOnlyObjectSet.AddObject(new CustomerRelationship
      {
        fk_ClientCustomerID = childId,
        fk_CustomerRelationshipTypeID = (int)CustomerRelationshipTypeEnum.TCSCustomer,
        fk_ParentCustomerID = parentId
      });
      mockINhOp.SetupGet(o => o.CustomerRelationshipReadOnly).Returns(mockcustomerRelationshipReadOnlyObjectSet);

      var storage = new Storage(mockInhOpContextFactory.Object, mockCacheManager.Object, mockStringEncryptor.Object, 0, 0, 0, 0);

      var result = storage.FindCustomerParent(childUid, CustomerTypeEnum.Customer);
      Assert.AreEqual(parentUid, result);
    }

    [TestMethod]
    public void TestFindCustomerParent_ChildNotFound()
    {
      var mockInhOp = new Mock<INH_OP>();
      var mockCacheManager = new Mock<ICacheManager>();
      var mockStringEncryptor = new Mock<IStringEncryptor>();
      var mockInhOpContextFactory = new Mock<INHOpContextFactory>();

      mockInhOpContextFactory.Setup(o => o.CreateContext()).Returns(mockInhOp.Object);
      mockInhOp.SetupGet(o => o.CustomerReadOnly).Returns(new MockObjectSet<Customer>());

      var storage = new Storage(mockInhOpContextFactory.Object, mockCacheManager.Object, mockStringEncryptor.Object, 0, 0, 0, 0);

      var result = storage.FindCustomerParent(Guid.NewGuid(), CustomerTypeEnum.Dealer);
      Assert.IsNull(result);
    }

    [TestMethod]
    public void TestFindCustomerParent_ParentTypeUnsupported()
    {
      Guid childUid = Guid.NewGuid();

      var mockInhOp = new Mock<INH_OP>();
      var mockCacheManager = new Mock<ICacheManager>();
      var mockStringEncryptor = new Mock<IStringEncryptor>();
      var mockInhOpContextFactory = new Mock<INHOpContextFactory>();

      mockInhOpContextFactory.Setup(o => o.CreateContext()).Returns(mockInhOp.Object);

      var mockcustomerReadOnlyObjectSet = new MockObjectSet<Customer>();
      mockcustomerReadOnlyObjectSet.AddObject(new Customer
      {
        ID = 42,
        CustomerUID = childUid
      });
      mockInhOp.SetupGet(o => o.CustomerReadOnly).Returns(mockcustomerReadOnlyObjectSet);

      var storage = new Storage(mockInhOpContextFactory.Object, mockCacheManager.Object, mockStringEncryptor.Object, 0, 0, 0, 0);

      var result = storage.FindCustomerParent(childUid, CustomerTypeEnum.Operations);
      Assert.IsNull(result);
    }

    [TestMethod]
    public void TestFindCustomerParent_RelationshipNotFound()
    {
      Guid childUid = Guid.NewGuid();

      var mockINhOp = new Mock<INH_OP>();
      var mockCacheManager = new Mock<ICacheManager>();
      var mockStringEncryptor = new Mock<IStringEncryptor>();
      var mockInhOpContextFactory = new Mock<INHOpContextFactory>();

      mockInhOpContextFactory.Setup(o => o.CreateContext()).Returns(mockINhOp.Object);

      var mockcustomerReadOnlyObjectSet = new MockObjectSet<Customer>();
      mockcustomerReadOnlyObjectSet.AddObject(new Customer
      {
        ID = 42,
        CustomerUID = childUid
      });
      mockcustomerReadOnlyObjectSet.AddObject(new Customer
      {
        ID = 43,
        CustomerUID = Guid.NewGuid()
      });
      mockINhOp.SetupGet(o => o.CustomerReadOnly).Returns(mockcustomerReadOnlyObjectSet);
      mockINhOp.SetupGet(o => o.CustomerRelationshipReadOnly).Returns(new MockObjectSet<CustomerRelationship>());

      var storage = new Storage(mockInhOpContextFactory.Object, mockCacheManager.Object, mockStringEncryptor.Object, 0, 0, 0, 0);

      var result = storage.FindCustomerParent(childUid, CustomerTypeEnum.Dealer);
      Assert.IsNull(result);
    }

    #endregion

    #region Privates

    private MockObjectSet<AssetReference> CreateAssetReferenceRecords()
    {
      MockObjectSet<AssetReference> mockAssetReferenceRecords = new MockObjectSet<AssetReference>();
      mockAssetReferenceRecords.AddObject(GetAssetReferenceRecord(1, 1, "sn_make", "123_CAT", new UUIDSequentialGuid().CreateGuid()));
      mockAssetReferenceRecords.AddObject(GetAssetReferenceRecord(2, 1, "sn_make", "234_CAT", new UUIDSequentialGuid().CreateGuid()));

      return mockAssetReferenceRecords;
    }

    private AssetReference GetAssetReferenceRecord(long id, long storeId, string alias, string value, Guid uid)
    {
      return new AssetReference { ID = id, fk_StoreID = storeId, Alias = alias, Value = value, UID = uid };
    }

    private MockObjectSet<CustomerReference> CreateCustomerReferenceRecords()
    {
      MockObjectSet<CustomerReference> mockCustomerReferenceRecords = new MockObjectSet<CustomerReference>();
      mockCustomerReferenceRecords.AddObject(GetCustomerReferenceRecord(1, 1, "BSSID", "111222333", new UUIDSequentialGuid().CreateGuid()));
      mockCustomerReferenceRecords.AddObject(GetCustomerReferenceRecord(2, 1, "BSSID", "222333444", new UUIDSequentialGuid().CreateGuid()));
      return mockCustomerReferenceRecords;
    }

    private CustomerReference GetCustomerReferenceRecord(long id, long storeId, string alias, string value, Guid uid)
    {
      return new CustomerReference {ID = id, fk_StoreID = storeId, Alias = alias, Value = value, UID = uid};
    }

    private MockObjectSet<DeviceReference> CreateDeviceReferenceRecords()
    {
      MockObjectSet<DeviceReference> mockDeviceReferenceRecords = new MockObjectSet<DeviceReference>();
      mockDeviceReferenceRecords.AddObject(GetDeviceReferenceRecord(1, 1, "GpsDeviceId", "g123", new UUIDSequentialGuid().CreateGuid()));
      mockDeviceReferenceRecords.AddObject(GetDeviceReferenceRecord(2, 1, "GpsDeviceId", "g234", new UUIDSequentialGuid().CreateGuid()));

      return mockDeviceReferenceRecords;
    }

    private DeviceReference GetDeviceReferenceRecord(long id, long storeId, string alias, string value, Guid uid)
    {
      return new DeviceReference { ID = id, fk_StoreID = storeId, Alias = alias, Value = value, UID = uid };
    }

    private MockObjectSet<ServiceReference> CreateServiceReferenceRecords()
    {
      MockObjectSet<ServiceReference> mockServiceReferenceRecords = new MockObjectSet<ServiceReference>();
      mockServiceReferenceRecords.AddObject(GetServiceReferenceRecord(1, 1, "ServiceRef", "123", new UUIDSequentialGuid().CreateGuid()));
      mockServiceReferenceRecords.AddObject(GetServiceReferenceRecord(2, 1, "ServiceRef", "234", new UUIDSequentialGuid().CreateGuid()));
      return mockServiceReferenceRecords;
    }

    private ServiceReference GetServiceReferenceRecord(long id, long storeId, string alias, string value, Guid uid)
    {
      return new ServiceReference { ID = id, fk_StoreID = storeId, Alias = alias, Value = value, UID = uid };
    }
    
    private T Construct<T>(params object[] p)
    {
      var ctor = typeof (T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First(t => t.GetParameters().Length == p.Length);
      return (T)ctor.Invoke(p);
    }

    private SqlException BuildSqlException(params string[] errorStrings)
    {
      var collection = Construct<SqlErrorCollection>();
      var mi = typeof(SqlErrorCollection)
          .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);

      foreach (var errorString in errorStrings)
      {
        var error = Construct<SqlError>(1, (byte)2, (byte)3, "server name", errorString, "proc", 100);
        mi.Invoke(collection, new object[] { error });
      }

      var methods = typeof (SqlException).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name.Equals("CreateException") && m.GetParameters().Length == 2).ToArray();
      var e = methods[0].Invoke(null, new object[] { collection, "7.0.0"}) as SqlException;

      return e;
    }
  }

    #endregion
}