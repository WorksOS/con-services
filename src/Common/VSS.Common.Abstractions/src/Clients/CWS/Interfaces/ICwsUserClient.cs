using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsUserClient
  {
    Task<UserResponseModel> GetUser(Guid userId, IDictionary<string, string> customHeaders = null);
  }
}
