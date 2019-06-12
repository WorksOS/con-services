using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling.Coords
{
  public class CSIBResult : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// The coordinate system definition as a string.
    /// </summary>
    public string CSIB { get; private set; }

    /// <summary>
    /// Override constructor with a parameter.
    /// </summary>
    /// <param name="csib"></param>
    public CSIBResult(string csib)
    {
      CSIB = csib;
    }

    public List<string> GetIdentifiers() => string.IsNullOrEmpty(CSIB) ? new List<string>() : new List<string>() { CSIB };
  }
}
