using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IAccountProxy : ICacheProxy
  {
    // todoMaverick not ProjectData
    Task<DeviceLicenseResponseModel> GetDeviceLicenses(string accountTrn);
  }
}
