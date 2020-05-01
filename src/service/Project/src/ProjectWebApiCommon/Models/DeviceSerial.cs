using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class DeviceSerial
  {
    public string SerialNumber { get; set; }

    private DeviceSerial()
    { }

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
