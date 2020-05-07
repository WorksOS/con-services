using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class DeviceDescriptorSingleResult : ContractExecutionResult
  {
    private DeviceData _deviceData { get; set; }

    public DeviceDescriptorSingleResult()
    {
      _deviceData = null;
    }

    public DeviceDescriptorSingleResult(int code, string message, DeviceData deviceData = null)
    {
      Code = code;
      Message = message;
      _deviceData = deviceData;
      if (_deviceData != null)
        _deviceData.Code = code;
    }

    public DeviceDescriptorSingleResult(DeviceData deviceData)
    {
      _deviceData = deviceData;
    }

    public DeviceData DeviceDescriptor { get { return _deviceData; } set { _deviceData = value; } }
  }
}
