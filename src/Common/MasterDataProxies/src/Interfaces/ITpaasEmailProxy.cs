using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITpaasEmailProxy
  {
    Task<Stream> SendEmail(EmailModel emailModel, IHeaderDictionary customHeaders = null);
  }
}
