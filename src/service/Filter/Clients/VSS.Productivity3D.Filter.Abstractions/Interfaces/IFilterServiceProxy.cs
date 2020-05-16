using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;

namespace VSS.Productivity3D.Filter.Abstractions.Interfaces
{
  public interface IFilterServiceProxy : ICacheProxy
  {
    Task<FilterDescriptor> GetFilter(string projectUid, string filterUid,
      IHeaderDictionary customHeaders = null);

    Task<FilterDescriptorSingleResult> CreateFilter(string projectUid, FilterRequest request, IHeaderDictionary customHeaders = null);

    Task<List<FilterDescriptor>> GetFilters(string projectUid, IHeaderDictionary customHeaders = null);

    void ClearCacheListItem(string projectUid, string userId = null);
  }
}
