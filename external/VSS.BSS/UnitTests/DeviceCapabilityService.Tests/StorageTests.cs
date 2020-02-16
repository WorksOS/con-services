using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;
using ED= VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Hosted.VLCommon.NH_OPMockObjectSet;
using VSS.Nighthawk.DeviceCapabilityService.Data;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests
{
  [TestClass]
  public class StorageTests
  {
    [TestMethod]
    public void TestGetDeviceTypeForDevice_DeviceNotInCache_DeviceIsInDatabase_Success()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.DeviceReadOnly).Returns(GetDeviceRecords());
      _mockNhOp.SetupGet(o => o.DeviceTypeReadOnly).Returns(GetDeviceTypeRecords());
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      ED.DeviceTypeEnum? actualDeviceType = storage.GetDeviceTypeForDevice(123);
      Assert.AreEqual(ED.DeviceTypeEnum.TAP66, actualDeviceType);
      _mockCacheManager.Verify(o => o.Add("Device.123", ED.DeviceTypeEnum.TAP66, 1), Times.Once());
    }

    [TestMethod]
    public void TestGetDeviceTypeForDevice_DeviceNotInCache_DeviceIsNotInDatabase_Success()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.DeviceReadOnly).Returns(GetDeviceRecords());
      _mockNhOp.SetupGet(o => o.DeviceTypeReadOnly).Returns(GetDeviceTypeRecords());
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      ED.DeviceTypeEnum? actualDeviceType = storage.GetDeviceTypeForDevice(111);
      Assert.IsNull(actualDeviceType);
      _mockCacheManager.Verify(o => o.Add("Device.111", null, 1), Times.Once());
    }

    [TestMethod]
    [ExpectedException(typeof (Exception), "db exception")]
    public void TestGetDeviceTypeForDevice_DeviceNotInCache_AccessingDatabaseThrowsException()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.DeviceReadOnly).Returns(GetDeviceRecords());
      _mockNhOp.SetupGet(o => o.DeviceTypeReadOnly).Throws(new Exception("db exception"));
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      storage.GetDeviceTypeForDevice(123);
    }

    [TestMethod]
    public void TestGetDeviceTypeForDevice_DeviceIsInCache_Success()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData("Device.123")).Returns(ED.DeviceTypeEnum.TAP66);
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      ED.DeviceTypeEnum? actualDeviceType = storage.GetDeviceTypeForDevice(123);
      Assert.AreEqual(ED.DeviceTypeEnum.TAP66, actualDeviceType);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), It.IsAny<ED.DeviceTypeEnum>(), 1), Times.Never());
    }
    [TestMethod]
    public void TestGetDeviceTypeForAsset_AssetNotInCache_AssetIsInDatabase_Success()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.AssetReadOnly).Returns(GetAssetRecords());
      _mockNhOp.SetupGet(o => o.DeviceReadOnly).Returns(GetDeviceRecords());
      _mockNhOp.SetupGet(o => o.DeviceTypeReadOnly).Returns(GetDeviceTypeRecords());
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      ED.DeviceTypeEnum? actualDeviceType = storage.GetDeviceTypeForAsset(1111111);
      Assert.AreEqual(ED.DeviceTypeEnum.TAP66, actualDeviceType);
      _mockCacheManager.Verify(o => o.Add("Asset.1111111", ED.DeviceTypeEnum.TAP66, 1), Times.Once());
    }

    [TestMethod]
    public void TestGetDeviceTypeForAsset_AssetNotInCache_AssetIsNotInDatabase_Success()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.AssetReadOnly).Returns(GetAssetRecords());
      _mockNhOp.SetupGet(o => o.DeviceReadOnly).Returns(GetDeviceRecords());
      _mockNhOp.SetupGet(o => o.DeviceTypeReadOnly).Returns(GetDeviceTypeRecords());
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      ED.DeviceTypeEnum? actualDeviceType = storage.GetDeviceTypeForAsset(2222222);
      Assert.IsNull(actualDeviceType);
      _mockCacheManager.Verify(o => o.Add("Asset.2222222", null, 1), Times.Once());
    }

    [TestMethod]
    public void TestGetDeviceTypeForAsset_AssetNotInCache_DeviceIsNotInDatabase_Success()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.AssetReadOnly).Returns(GetAssetRecords());
      _mockNhOp.SetupGet(o => o.DeviceReadOnly).Returns(new MockObjectSet<Device>());
      _mockNhOp.SetupGet(o => o.DeviceTypeReadOnly).Returns(new MockObjectSet<DeviceType>());
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      ED.DeviceTypeEnum? actualDeviceType = storage.GetDeviceTypeForAsset(1111111);
      Assert.IsNull(actualDeviceType);
      _mockCacheManager.Verify(o => o.Add("Asset.1111111", null, 1), Times.Once());
    }

    [TestMethod]
    [ExpectedException(typeof(Exception), "db exception")]
    public void TestGetDeviceTypeForAsset_AssetNotInCache_AccessingDatabaseThrowsException()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      _mockNhOp.SetupGet(o => o.AssetReadOnly).Returns(GetAssetRecords());
      _mockNhOp.SetupGet(o => o.DeviceReadOnly).Returns(GetDeviceRecords());
      _mockNhOp.SetupGet(o => o.DeviceTypeReadOnly).Throws(new Exception("db exception"));
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      storage.GetDeviceTypeForAsset(1111111);
    }

    [TestMethod]
    public void TestGetDeviceTypeForAsset_AssetIsInCache_Success()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData("Asset.1111111")).Returns(ED.DeviceTypeEnum.TAP66);
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      ED.DeviceTypeEnum? actualDeviceType = storage.GetDeviceTypeForAsset(1111111);
      Assert.AreEqual(ED.DeviceTypeEnum.TAP66, actualDeviceType);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), It.IsAny<ED.DeviceTypeEnum>(), 1), Times.Never());
    }

    [TestMethod]
    public void TestGetDeviceTypeForDevice_GetEndpointDescriptorsForNames_DescriptorNotInCache_DescriptorInDatabase_Success()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      MockObjectSet<ServiceProvider> serviceProviderRecord = GetServiceProviderRecord("application/xml", 1, "TNL", "username",
        "password", "tnl.com");
      IEnumerable<string> endpointNames = new [] {"TNL"};
      _mockNhOp.SetupGet(o => o.ServiceProviderReadOnly).Returns(serviceProviderRecord);
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      IEnumerable<IEndpointDescriptor> descriptors = storage.GetEndpointDescriptorsForNames(endpointNames);
      IEndpointDescriptor tnlDescriptor = descriptors.FirstOrDefault(o => o.Name == "TNL");
      Assert.IsNotNull(tnlDescriptor);
      Assert.AreEqual("TNL", tnlDescriptor.Name);
      Assert.AreEqual(1, tnlDescriptor.Id);
      Assert.AreEqual("tnl.com", tnlDescriptor.Url);
      Assert.AreEqual("application/xml", tnlDescriptor.ContentType);
      Assert.AreEqual("username", tnlDescriptor.Username);
      Assert.AreEqual("V/+rdrJ1QKY/p8Sw+I0fbQ==", tnlDescriptor.EncryptedPwd);
      _mockCacheManager.Verify(o => o.Add("Endpoint.TNL", tnlDescriptor, 1), Times.Once());
    }

    [TestMethod]
    [ExpectedException(typeof (Exception), "could not be found")]
    public void
      TestGetDeviceTypeForDevice_GetEndpointDescriptorsForNames_DescriptorNotInCache_DescriptorNotInDatabase_ThrowsException
      ()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      MockObjectSet<ServiceProvider> serviceProviderRecord = GetServiceProviderRecord("application/xml", 1, "TNL",
        "username",
        "password", "tnl.com");
      IEnumerable<string> endpointNames = new[] {"LNT"};
      _mockNhOp.SetupGet(o => o.ServiceProviderReadOnly).Returns(serviceProviderRecord);
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      storage.GetEndpointDescriptorsForNames(endpointNames);
    }

    [TestMethod]
    [ExpectedException(typeof (Exception), "db exception")]
    public void
      TestGetDeviceTypeForDevice_GetEndpointDescriptorsForNames_DescriptorNotInCache_AccessingDatabase_ThrowsException()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(null);
      IEnumerable<string> endpointNames = new[] {"TNL"};
      _mockNhOp.SetupGet(o => o.ServiceProviderReadOnly).Throws(new Exception("db exception"));
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      storage.GetEndpointDescriptorsForNames(endpointNames);
    }

    [TestMethod]
    public void TestGetDeviceTypeForDevice_GetEndpointDescriptorsForNames_DescriptorIsInCache_Success()
    {
      Mock<INH_OP> _mockNhOp = new Mock<INH_OP>();
      Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();
      IStringEncryptor _stringEncryptor = new StringEncryptor();
      NHOPFactory _nhOpFactory = new NHOPFactory(() => { return _mockNhOp.Object; });
    
      IEndpointDescriptor expectedEndpointDescriptor = new EndpointDescriptor
      {
        ContentType = "application/xml",
        Id = 1,
        Name = "TNL",
        Url = "tnl.com",
        Username = "username",
        EncryptedPwd = "encodedPassword"
      };
      _mockCacheManager.Setup(o => o.GetData(It.IsAny<string>())).Returns(expectedEndpointDescriptor);
      IEnumerable<string> endpointNames = new[] {"TNL"};
      IStorage storage = new Storage(_nhOpFactory, _mockCacheManager.Object, _stringEncryptor, 1, 1);
      IEnumerable<IEndpointDescriptor> descriptors = storage.GetEndpointDescriptorsForNames(endpointNames);
      IEndpointDescriptor tnlDescriptor = descriptors.FirstOrDefault(o => o.Name == "TNL");
      Assert.IsNotNull(tnlDescriptor);
      Assert.AreEqual("TNL", tnlDescriptor.Name);
      Assert.AreEqual(1, tnlDescriptor.Id);
      Assert.AreEqual("tnl.com", tnlDescriptor.Url);
      Assert.AreEqual("application/xml", tnlDescriptor.ContentType);
      Assert.AreEqual("username", tnlDescriptor.Username);
      Assert.AreEqual("encodedPassword", tnlDescriptor.EncryptedPwd);
      _mockCacheManager.Verify(o => o.Add(It.IsAny<string>(), It.IsAny<IEndpointDescriptor>(), 1), Times.Never());
    }

    private MockObjectSet<Device> GetDeviceRecords()
    {
      MockObjectSet<Device> records = new MockObjectSet<Device>();
      records.AddObject(GetDeviceRecord(123, "tap66device", DeviceTypeEnum.TAP66));
      return records;
    }

    private Device GetDeviceRecord(long id, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      return new Device {ID = id, GpsDeviceID = gpsDeviceID, fk_DeviceTypeID = (int) deviceType};
    }

    private MockObjectSet<DeviceType> GetDeviceTypeRecords()
    {
      MockObjectSet<DeviceType> records = new MockObjectSet<DeviceType>();
      foreach (var deviceType in Enum.GetValues(typeof(DeviceTypeEnum)))
      {
        records.AddObject(new DeviceType {ID = (int)deviceType, Name = deviceType.ToString()});
      }
      return records;
    }

    private MockObjectSet<Asset> GetAssetRecords()
    {
      MockObjectSet<Asset> records = new MockObjectSet<Asset>();
      records.AddObject(GetAssetRecord(1111111, 123));
      return records;
    }

    private Asset GetAssetRecord(long assetID, long fk_deviceId)
    {
      return new Asset {AssetID = assetID, fk_DeviceID = fk_deviceId};
    }

    private MockObjectSet<ServiceProvider> GetServiceProviderRecord(string contentType, long id, string providerName,
      string userName, string password, string url)
    {
      MockObjectSet<ServiceProvider> record = new MockObjectSet<ServiceProvider>();
      record.AddObject(new ServiceProvider
      {
        MessageContentType = contentType,
        ID = id,
        ProviderName = providerName,
        UserName = userName,
        Password = password,
        ServerIPAddress = url
      });
      return record;
    }
  }
}
