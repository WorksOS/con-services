using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.Project.WebAPI.Common.Models.DeviceStatus;
using VSS.Productivity3D._3DAssetMgmt.Abstractions;

namespace VSS.MasterData.Project.WebAPI.Internal
{
  public static class MockDeviceRepository
  {
    private static readonly DeviceStatus _deviceStatus = new DeviceStatus
    {
      deviceId = "14a4177f9ce1a112ac2",
      deviceName = "Tablet-1294012",
      lat = 39.8971033631906,
      lon = -105.11554773936436,
      designName = "Pond.V11",
      designId = "5c9bcbd1c6769c105ea09b5",
      deviceNickname = "ChubuddysBuddyDozer",
      assetType = "Tablet",
      projectName = "The Pond",
      lastReported = "2019-06-12T18:53:52Z",
      assetSerialNumber = "Tablet-T71SY-1300-001",
      swWarrantyExpUtc = "2019-06-12",
      osName = "Win32NT",
      appName = "Trimble Groundworks",
      correctionSource = "Wifi,DaVinci",
      ts = "2019-06-12T18:53:52Z",
      h = 0.27,
      operatorName = "ChubuddyG",
      isLlhSiteLocal = true,
      language = "en-US",
      coordinateSystemHash = "120EA8A25E5D487BF68B5F7",
      isDataLogging = false,
      antennaType = "SPS985",
      targetType = "AT360",
      rodHeight = 1052.89,
      radioIntegrity = 100,
      systemStatus = "Disconnected",
      attachmentName = "Bucket",
      attachmentWearUpdateUtc = "2019-06-12",
      workOrderName = "pond cut",
      designType = "CutFill",
      designSurfaceName = "pondSufaceCut",
      designVertOffset = 0.61,
      designPerpOffset = 164.99,
      assetNickname = "ChubuddysBuddy",
      assetMake = "EC520",
      assetModel = "336F",
      osVersion = "Microsoft Windows NT 6.1.7601 Service Pack 1",
      appVersion = "1.1.19200.96",
      freeSpace = 1024000,
      batteryPercent = 75,
      powerSource = "battery",
      licenseCodes = "C0821",
      baseStationName = "PondBaseStation",
      baseStationLat = 35.8971033631906,
      baseStationLon = -103.11554773936436,
      baseStationHeight = 1523.31,
      internalTemp = 18,
      totalRunTime = 345,
      totalCellTime = 225,
      totalWifiTime = 236,
      totalAppTime = 223,
      totalAutomaticsTime = 178,
      networks = new List<Network>
      {
        new Network
        {
          type = "Cell",
          signal = -92,
          state = "connected",
          dbm = 40,
          uptime = 30,
          carrier = "Verizon",
          roaming = false,
          cellTech = "4G",
          mcc = "404",
          mnc = "10",
          simId = "9320B39J",
          iccId = "25772365112890019904",
          phoneNumber = "7194530922",
          modemResets = 19216801,
          apn = "icecreamapn",
          apnUsername = "icecreamun",
          apnPasswordSet = false,
          txData = 1024,
          rxData = 1024,
          regulatoryDomain = "FCC",
          regulatoryDomainMethod = "2.4GHz"
        },
        new Network
        {
          type = "Cell",
          signal = -92,
          state = "connected",
          dbm = 40,
          uptime = 30,
          carrier = "Verizon",
          roaming = false,
          cellTech = "4G",
          mcc = "404",
          mnc = "10",
          simId = "9320B39J",
          iccId = "25772365112890019904",
          phoneNumber = "7194530922",
          modemResets = 19216801,
          apn = "icecreamapn",
          apnUsername = "icecreamun",
          apnPasswordSet = false,
          txData = 1024,
          rxData = 1024,
          regulatoryDomain = "FCC",
          regulatoryDomainMethod = "2.4GHz"
        }
      },
      gnss = new List<GNSSAntenna>
      {
        new GNSSAntenna
        {
          antennaLocation = "primary",
          antennaSerialNumber = "SN391832445",
          svsUsed = new SatelliteVehicle
          {
            gps = 12,
            gln = 7350053850019,
            bds = 17,
            gal = 10,
            irnss = 83
          }
        },
        new GNSSAntenna
        {
          antennaLocation = "primary",
          antennaSerialNumber = "SN391832445",
          svsUsed = new SatelliteVehicle
          {
            gps = 12,
            gln = 7350053850019,
            bds = 17,
            gal = 10,
            irnss = 83
          }
        }
      },
      devices = new List<ConnectedDevice>
      {
        new ConnectedDevice
        {
          nickname = "ChubuddysBuddyDozer",
          serialNumber = "23426644",
          model = "Tablet",
          firmware = "1.0.1.9876",
          batteryPercent = 79,
          licenseCodes = "D0928",
          swWarrantyExpUtc = "2019-06-12"
        },
        new ConnectedDevice
        {
          nickname = "ChubuddysBuddyDozer",
          serialNumber = "23426644",
          model = "Tablet",
          firmware = "1.0.1.9876",
          batteryPercent = 79,
          licenseCodes = "D0928",
          swWarrantyExpUtc = "2019-06-12"
        },
        new ConnectedDevice
        {
          nickname = "ChubuddysBuddyDozer",
          serialNumber = "23426644",
          model = "Tablet",
          firmware = "1.0.1.9876",
          batteryPercent = 79,
          licenseCodes = "D0928",
          swWarrantyExpUtc = "2019-06-12"
        }
      },
      projects = new List<ProjectID>
      {
        new ProjectID
        {
          projectId = "trn::profilex:us-west-2:project:c19b974e-64c0"
        },
        new ProjectID
        {
          projectId = "trn::profilex:us-west-2:project:c19b974e-64c0"
        },
        new ProjectID
        {
          projectId = "trn::profilex:us-west-2:project:c19b974e-64c0"
        },
        new ProjectID
        {
          projectId = "trn::profilex:us-west-2:project:c19b974e-64c0"
        },
        new ProjectID
        {
          projectId = "trn::profilex:us-west-2:project:c19b974e-64c0"
        }
      } 
    };

    private static readonly List<DeviceStatus> _deviceStatuses = new List<DeviceStatus>
    { 
      new DeviceStatus(_deviceStatus),
      new DeviceStatus(_deviceStatus),
      new DeviceStatus(_deviceStatus),
      new DeviceStatus(_deviceStatus),
      new DeviceStatus(_deviceStatus)
    };

    private static readonly List<Asset> _assets = new List<Asset>
    {
      new Asset
      {
        AssetUID = "8982e5e7-1da1-4cf8-a335-ef7b0c6758b6",
        LegacyAssetID = 417113364,
        EquipmentVIN = "6758B6",
        SerialNumber = "EF7B0C",
        AssetType = "Bulldozer",
        LastActionedUtc = new DateTime(2020, 3, 26, 9, 10, 0),
        Name = "Tonka Bulldozer"
      },
      new Asset
      {
        AssetUID = "6cb6fa71-9800-4700-b7ff-c62014970deb",
        LegacyAssetID = 248925381,
        EquipmentVIN = "970DEB",
        SerialNumber = "C62014",
        AssetType = "Dump Truck",
        LastActionedUtc = new DateTime(2020, 3, 30, 14, 45, 4),
        Name = "Tonka Dump Truck"
      },
      new Asset
      {
        AssetUID = "b93dbe5a-7123-42f1-ab20-2ce8cfefa8f6",
        LegacyAssetID = 911158341,
        EquipmentVIN = "2CE8CF",
        SerialNumber = "EFA8F6",
        AssetType = "Crawler Loader",
        LastActionedUtc = new DateTime(2020, 3, 22, 13, 1, 45),
        Name = "Tonka Crawler Loader"
      },
      new Asset
      {
        AssetUID = "6b4dc385-b517-4baa-9419-d9dc58f808c5",
        LegacyAssetID = 1434204015,
        EquipmentVIN = "F808C5",
        SerialNumber = "D9DC58",
        AssetType = "Scraper",
        LastActionedUtc = new DateTime(2020, 3, 30, 18, 11, 23),
        Name = "Tonka Scraper"
      },
      new Asset
      {
        AssetUID = "0a1c60f2-2654-450d-b919-53e806685dd3",
        LegacyAssetID = 1178833682,
        EquipmentVIN = "685DD3",
        SerialNumber = "53E806",
        AssetType = "Excavator",
        LastActionedUtc = new DateTime(2020, 2, 1, 11, 2, 0),
        Name = "Tonka Excavator"
      }
    };
    
    public static List<Asset> GetAssets(IEnumerable<Guid> uids) => _assets.FindAll(x => uids.Any(y => y.ToString().Contains(x.AssetUID)));
    public static List<DeviceStatus> GetDevicesWithLKS() => GetUpdatedDeviceList();
    public static DeviceStatus GetDeviceWithLKS() => _deviceStatus;

    private static List<DeviceStatus> GetUpdatedDeviceList()
    {
      _deviceStatuses[0].deviceId = "8982e5e7-1da1-4cf8-a335-ef7b0c6758b6";
      _deviceStatuses[0].deviceName = "Backhoe Loader";
      _deviceStatuses[0].assetType = "BACKHOE_LOADER";
      _deviceStatuses[0].lat = -114.94628053154341;
      _deviceStatuses[0].lon = 36.23050406683231;

      _deviceStatuses[1].deviceId = "6cb6fa71-9800-4700-b7ff-c62014970deb";
      _deviceStatuses[1].deviceName = "Dump Truck";
      _deviceStatuses[1].assetType = "DUMP_TRUCK";
      _deviceStatuses[1].lat = -114.94670430086103;
      _deviceStatuses[1].lon = 36.23184072028269;

      _deviceStatuses[2].deviceId = "b93dbe5a-7123-42f1-ab20-2ce8cfefa8f6";
      _deviceStatuses[2].deviceName = "Excavator";
      _deviceStatuses[2].assetType = "EXCAVATOR";
      _deviceStatuses[2].lat = -114.94694764287715;
      _deviceStatuses[2].lon = 36.23007284059864;

      _deviceStatuses[3].deviceId = "6b4dc385-b517-4baa-9419-d9dc58f808c5";
      _deviceStatuses[3].deviceName = "CB460";
      _deviceStatuses[3].assetType = "MC_DISPLAY";
      _deviceStatuses[3].lat = -114.94508567462547;
      _deviceStatuses[3].lon = 36.2347641658546;

      _deviceStatuses[4].deviceId = "0a1c60f2-2654-450d-b919-53e806685dd3";
      _deviceStatuses[4].deviceName = "TSC3";
      _deviceStatuses[4].assetType = "SURVEY_DEVICE";
      _deviceStatuses[4].lat = -114.94659355301343;
      _deviceStatuses[4].lon = 36.23016854679474;

      return _deviceStatuses;
    }
  }
}
