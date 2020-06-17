using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Response;

namespace VSS.Productivity3D.Entitlements.Abstractions.Interfaces
{
    public interface IEntitlementProxy 
    {
      Task<EntitlementResponseModel> IsEntitled(EntitlementRequestModel request, IHeaderDictionary customHeaders = null);
    }
}
