using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class DeviceLKSListResponseModel : IMasterDataModel
  {
    public DeviceLKSListResponseModel()
    {
      Devices = new List<DeviceLKSResponseModel>();
    }

    public List<DeviceLKSResponseModel> Devices { get; set; }

    public List<string> GetIdentifiers()
    {
      return Devices != null ? Devices.SelectMany(p => p.GetIdentifiers()).ToList() : new List<string>();
    }
  }
}
