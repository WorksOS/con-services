using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ILoadDumpProxy
  {
    Task<List<LoadDumpLocation>> GetLoadDumpLocations(string projectUid, IHeaderDictionary customHeaders);
  }
}
