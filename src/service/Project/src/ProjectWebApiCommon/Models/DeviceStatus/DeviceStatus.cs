using System.Collections.Generic;

namespace VSS.MasterData.Project.WebAPI.Common.Models.DeviceStatus
{
  public class DeviceStatus
  {
    public string deviceId { get; set; }
    public string deviceName { get; set; }
    public double lat { get; set; }
    public double lon { get; set; }
    public string designName { get; set; }
    public string designId { get; set; }
    public string deviceNickname { get; set; }
    public string assetType { get; set; }
    public string projectName { get; set; }
    public string lastReported { get; set; }
    public string assetSerialNumber { get; set; }
    public string swWarrantyExpUtc { get; set; }
    public string osName { get; set; }
    public string appName { get; set; }
    public string correctionSource { get; set; }
    public string ts { get; set; }
    public double h { get; set; }
    public string operatorName { get; set; }
    public bool isLlhSiteLocal { get; set; }
    public string language { get; set; }
    public string coordinateSystemHash { get; set; }
    public bool isDataLogging { get; set; }
    public string antennaType { get; set; }
    public string targetType { get; set; }
    public double rodHeight { get; set; }
    public short radioIntegrity { get; set; }
    public string systemStatus { get; set; }
    public string attachmentName { get; set; }
    public string attachmentWearUpdateUtc { get; set; }
    public string workOrderName { get; set; }
    public string designType { get; set; }
    public string designSurfaceName { get; set; }
    public double designVertOffset { get; set; }
    public double designPerpOffset { get; set; }
    public string assetNickname { get; set; }
    public string assetMake { get; set; }
    public string assetModel { get; set; }
    public string osVersion { get; set; }
    public string appVersion { get; set; }
    public double freeSpace { get; set; }
    public short batteryPercent { get; set; }
    public string powerSource { get; set; }
    public string licenseCodes { get; set; }
    public string baseStationName { get; set; }
    public double baseStationLat { get; set; }
    public double baseStationLon { get; set; }
    public double baseStationHeight { get; set; }
    public short internalTemp { get; set; }
    public short totalRunTime { get; set; }
    public short totalCellTime { get; set; }
    public short totalWifiTime { get; set; }
    public short totalAppTime { get; set; }
    public short totalAutomaticsTime { get; set; }
    public List<Network> networks { get; set; }
    public List<GNSSAntenna> gnss { get; set; }
    public List<ConnectedDevice> devices { get; set; }
    public List<ProjectID> projects { get; set; }
  }

  public class ProjectID
  {
    public string projectId { get; set; }
  }
}
