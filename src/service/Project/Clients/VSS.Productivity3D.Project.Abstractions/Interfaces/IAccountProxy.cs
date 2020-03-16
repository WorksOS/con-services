using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IAccountProxy : ICacheProxy
  {
    Task<DeviceLicenseResponseModel> GetDeviceLicenses(string accountUid);
  }
}
