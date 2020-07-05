using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ISupportsPaging<T> where T: IMasterDataModel
  {
    bool HasMore { get; }
    List<T> Models { get; set; }
  }
}
