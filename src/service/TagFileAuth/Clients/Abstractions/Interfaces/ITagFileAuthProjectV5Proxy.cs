using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces
{
  public interface ITagFileAuthProjectV5Proxy
  {
    Task<GetProjectUidsResult> GetProjectUids(GetProjectUidsRequest request, IHeaderDictionary customHeaders = null);

    Task<GetProjectUidsResult> GetProjectUidsEarthWorks(GetProjectUidsEarthWorksRequest request,  IHeaderDictionary customHeaders = null);
  }
}
