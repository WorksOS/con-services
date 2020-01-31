using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using VSS.Common.Abstractions.MasterData.Interfaces;

// todoJeannie move to common md proxies?
namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IGenericHttpProxy 
  {
    Task<T> ExecuteGenericHttpRequest<T>(string url, HttpMethod method, Stream body = null, IDictionary<string, string> customHeaders = null, int? timeout = null)
      where T : class, IMasterDataModel;
  }
}
