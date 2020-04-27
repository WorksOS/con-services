using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  public class DeviceSerial
  {
    public string SerialNumber { get; set; }

    private DeviceSerial()
    { }

    /// <summary>
    /// Create instance of ProjectSettingsRequest
    /// </summary>
    public DeviceSerial(string serialNumber)
    {
      SerialNumber = serialNumber;
    }

    public void Validate()
    { 
      if (string.IsNullOrEmpty(SerialNumber))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2072, "Invalid serialNumber."));
    }
  }
}
