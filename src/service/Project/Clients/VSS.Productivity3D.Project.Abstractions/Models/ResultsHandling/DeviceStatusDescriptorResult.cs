using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{

  /// <summary>
  /// List of device status descriptors
  /// </summary>
  public class DeviceStatusDescriptorsListResult : ContractExecutionResult
  {
    private List<DeviceStatusDescriptor> _deviceStatusDescriptors { get; set; }

    public DeviceStatusDescriptorsListResult() => _deviceStatusDescriptors = new List<DeviceStatusDescriptor>();

    public DeviceStatusDescriptorsListResult(List<DeviceStatusDescriptor> deviceStatusDescriptors)
        => _deviceStatusDescriptors = deviceStatusDescriptors;

    public void Add(DeviceStatusDescriptor deviceStatusDescriptor) => _deviceStatusDescriptors.Add(deviceStatusDescriptor);
    public List<DeviceStatusDescriptor> DeviceStatusDescriptors { get { return _deviceStatusDescriptors; } set { _deviceStatusDescriptors = value; } }
  }

  /// <summary>
  ///   Single device status descriptor
  /// </summary>
  public class DeviceStatusDescriptorSingleResult : ContractExecutionResult
  {
    private DeviceStatusDescriptor _deviceStatusDescriptor;

    public DeviceStatusDescriptorSingleResult(DeviceStatusDescriptor deviceStatusDescriptor)
      => _deviceStatusDescriptor = deviceStatusDescriptor;

    public DeviceStatusDescriptor DeviceDescriptor { get { return _deviceStatusDescriptor; } set { _deviceStatusDescriptor = value; } }
  }


  /// <summary>
  ///   Describes lastKnown device status
  /// </summary>
  public class DeviceStatusDescriptor
  {
    public string DeviceUid { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public CWSDeviceTypeEnum DeviceType { get; set; }
    public string SerialNumber { get; set; }
    public string DeviceName { get; set; }
    public string ProjectName { get; set; }
    public DateTime? LastReportedUtc { get; set; }

    public override bool Equals(object obj)
    {
      var otherDevice = obj as DeviceStatusDescriptor;
      if (otherDevice == null) return false;
      return otherDevice.DeviceUid == DeviceUid &&
              Math.Abs(otherDevice.Latitude - Latitude) < 1e-10 &&
              Math.Abs(otherDevice.Longitude - Longitude) < 1e-10 &&
              otherDevice.DeviceType == DeviceType &&
              otherDevice.SerialNumber == SerialNumber &&
              otherDevice.DeviceName == DeviceName &&
              otherDevice.ProjectName == ProjectName &&
              otherDevice.LastReportedUtc == LastReportedUtc
          ;
    }

    public override int GetHashCode() { return 0; }
  }
}
