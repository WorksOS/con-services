using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Hosted.VLCommon
{
  public interface IHttpTDMGClientWrapper
  {
    HttpResponseMessage PostMessage(string endpointUri, string payload, string username, string password, string accesstoken = null);
    HttpResponseMessage GetMessage(string enpointUri);
  }
}
