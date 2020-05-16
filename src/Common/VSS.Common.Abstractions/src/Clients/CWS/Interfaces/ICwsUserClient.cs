using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsUserClient
  {
    Task<UserResponseModel> GetUser(Guid userId, IHeaderDictionary customHeaders = null);
  }
}
