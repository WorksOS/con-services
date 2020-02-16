using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.BaseEvents
{
  [Serializable]
  public class EndpointDefinition
  {
    public string Name { get; set; }

    public long EndpointDefinitionId { get; set; }

    public string Url { get; set; }

    public string ContentType { get; set; }

    public string UserName { get; set; }

    public byte[] EncryptedPwd { get; set; }
  }
}
