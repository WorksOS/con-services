using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VSS.Productivity3D.Entitlements.Abstractions.Interfaces
{
  public interface IEmsClient
  {
    Task<HttpStatusCode> GetEntitlements(Guid userUid, Guid customerUid, string wosSku, string wosFeature, IHeaderDictionary customHeaders = null);
  }
}
