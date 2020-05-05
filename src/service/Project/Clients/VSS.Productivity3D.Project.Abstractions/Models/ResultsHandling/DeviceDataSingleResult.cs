using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class DeviceDataSingleResult : ContractExecutionResult
  {
    private DeviceData _deviceData { get; set; }

    public DeviceDataSingleResult()
    {
      _deviceData = null;
    }

    public DeviceDataSingleResult(int code, string message, DeviceData deviceData = null)
    {
      Code = code;
      Message = message;
      _deviceData = deviceData;
      if (_deviceData != null)
        _deviceData.Code = code;
    }

    public DeviceDataSingleResult(DeviceData deviceData)
    {
      _deviceData = deviceData;
    }

    public DeviceData DeviceDescriptor { get { return _deviceData; } set { _deviceData = value; } }
  }
}
