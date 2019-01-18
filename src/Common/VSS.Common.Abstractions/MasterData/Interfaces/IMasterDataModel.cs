using System.Collections.Generic;

namespace VSS.Common.Abstractions.MasterData.Interfaces
{
  public interface IMasterDataModel
  {
    /// <summary>
    /// List of all Primary Key values that are represented by this model,
    /// Including nested (eg Customer -> List of Projects would contain CustomerUID and ProjectUIDs)
    /// </summary>
    List<string> GetIdentifiers();
  }

}
