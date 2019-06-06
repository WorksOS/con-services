using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling.Coords
{
  public class CSIBResult : ContractExecutionResult
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
  }
}
