using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class DeviceIsUid
  {
    public string DeviceUid { get; set; }

    private DeviceIsUid()
    { }

    public DeviceIsUid(string deviceUid)
    {
      DeviceUid = deviceUid;
    }

    public void Validate()
    { 
      if (string.IsNullOrEmpty(DeviceUid) || !Guid.TryParse(DeviceUid, out var guid))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(126, "Invalid deviceUid."));
    }
  }
}
