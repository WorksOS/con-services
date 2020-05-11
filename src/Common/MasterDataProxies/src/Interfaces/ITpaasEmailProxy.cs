using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITpaasEmailProxy
  {
    Task<Stream> SendEmail(EmailModel emailModel, IDictionary<string, string> customHeaders = null);
  }
}
