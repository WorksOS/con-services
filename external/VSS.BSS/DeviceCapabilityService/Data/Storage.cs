using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers;
using ED = VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.Nighthawk.DeviceCapabilityService.Data
{
  public class Storage : IStorage
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly NHOPFactory _nhOpFactory;
    private readonly ICacheManager _cacheManager;
    private readonly IStringEncryptor _stringEncryptor;
    private readonly int _deviceTypeCacheLifetimeMinutes;
    private readonly int _endpointCacheLifetimeMinutes;

    public Storage(NHOPFactory nhopFactory, ICacheManager cacheManager, IStringEncryptor stringEncryptor,
      int deviceTypeCacheLifetimeMinutes, int endpointCacheLifetimeMinutes)
    {
      _nhOpFactory = nhopFactory;
      _cacheManager = cacheManager;
      _stringEncryptor = stringEncryptor;
      _deviceTypeCacheLifetimeMinutes = deviceTypeCacheLifetimeMinutes;
      _endpointCacheLifetimeMinutes = endpointCacheLifetimeMinutes;
    }

    public ED.DeviceTypeEnum? GetDeviceTypeForDevice(long deviceId)
    {
      var deviceKey = String.Format("Device.{0}", deviceId);
      Log.IfDebugFormat("Retrieving device type for device ({0}) from cache", deviceId);
      ED.DeviceTypeEnum? deviceTypeOfDevice = (ED.DeviceTypeEnum?)_cacheManager.GetData(deviceKey);

      if (deviceTypeOfDevice == null)
      {
        Log.IfDebugFormat("Retrieving device type for device ({0}) from database", deviceId);
        try
        {
          using (var nhOp = _nhOpFactory())
          {
            string deviceTypeName = (
                                      from device in nhOp.DeviceReadOnly
                                      join deviceType in nhOp.DeviceTypeReadOnly on device.fk_DeviceTypeID equals deviceType.ID
                                      where device.ID == deviceId
                                      select deviceType.Name
                                    )
              .FirstOrDefault();

            deviceTypeOfDevice = (deviceTypeName == null)
                                   ? (ED.DeviceTypeEnum?)null
                                   : deviceTypeName.ToEnum<ED.DeviceTypeEnum>();
          }
        }
        catch (Exception e)
        {
          Log.IfError("Error accessing the database", e);
          throw;
        }
        _cacheManager.Add(
          deviceKey,
          deviceTypeOfDevice,
          _deviceTypeCacheLifetimeMinutes);
      }

      return deviceTypeOfDevice;
    }

    public ED.DeviceTypeEnum? GetDeviceTypeForAsset(long assetId)
    {
      var assetKey = String.Format("Asset.{0}", assetId);
      Log.IfDebugFormat("Retrieving device type for asset ({0}) from cache", assetId);
      ED.DeviceTypeEnum? deviceTypeOfDevice = (ED.DeviceTypeEnum?)_cacheManager.GetData(assetKey);

      if (deviceTypeOfDevice == null)
      {
        Log.IfDebugFormat("Retrieving device type for asset ({0}) from database", assetId);
        try
        {
          using (var nhOp = _nhOpFactory())
          {
            string deviceTypeName = (
                                      from asset in nhOp.AssetReadOnly
                                      join device in nhOp.DeviceReadOnly on asset.fk_DeviceID equals device.ID
                                      join deviceType in nhOp.DeviceTypeReadOnly on device.fk_DeviceTypeID equals deviceType.ID
                                      where asset.AssetID == assetId
                                      select deviceType.Name
                                    )
              .FirstOrDefault();

            deviceTypeOfDevice = (deviceTypeName == null)
                                   ? (ED.DeviceTypeEnum?)null
                                   : deviceTypeName.ToEnum<ED.DeviceTypeEnum>();
          }
        }
        catch (Exception e)
        {
          Log.IfError("Error accessing the database", e);
          throw;
        }
        _cacheManager.Add(
          assetKey,
          deviceTypeOfDevice,
          _deviceTypeCacheLifetimeMinutes);
      }

      return deviceTypeOfDevice;
    }

    public IEnumerable<EndpointDescriptor> GetEndpointDescriptorsForNames(IEnumerable<string> endpointNames)
    {
      var descriptors = new List<EndpointDescriptor>();

      foreach (var endpointName in endpointNames)
      {
        var endpointKey = String.Format("Endpoint.{0}", endpointName);
        Log.IfDebugFormat("Retrieving endpoint ({0}) from cache", endpointName);
        var descriptor = (EndpointDescriptor) _cacheManager.GetData(endpointKey);

        if (descriptor == null)
        {
          Log.IfDebugFormat("Retrieving endpoint ({0}) from database", endpointName);
          try
          {
            using (var nhOp = _nhOpFactory())
            {
              descriptor = (
                from serviceProvider in nhOp.ServiceProviderReadOnly
                where endpointName == serviceProvider.ProviderName
                select new EndpointDescriptor
                {
                  ContentType = serviceProvider.MessageContentType,
                  Id = serviceProvider.ID,
                  Name = serviceProvider.ProviderName,
                  Username = serviceProvider.UserName,
                  EncryptedPwd = serviceProvider.Password,
                  Url = serviceProvider.ServerIPAddress
                }
                ).FirstOrDefault();
            }

            if (descriptor == null)
            {
              throw new Exception(string.Format("Endpoint descriptor of {0} could not be found.", endpointName));
            }

            descriptor.EncryptedPwd = EncryptString(descriptor.EncryptedPwd);

            _cacheManager.Add(
              endpointKey,
              descriptor,
              _endpointCacheLifetimeMinutes);
          }
          catch (Exception e)
          {
            Log.IfError("Error accessing the database", e);
            throw;
          }
        }

        descriptors.Add(descriptor);
      }

      return descriptors;
    }

    private string EncryptString(string clearText)
    {
      return Convert.ToBase64String(_stringEncryptor.EncryptStringToBytes(clearText,
        Encoding.UTF8.GetBytes(Config.Default.DispatcherAESKeyByte),
        Encoding.UTF8.GetBytes(Config.Default.DispatcherAESIVByte)));
    }
  }
}
