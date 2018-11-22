using System.IO;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.TRex.Gateway.Common.ResultHandling
{
  public class PatchDataResult : ContractExecutionResult
  {
    public byte[] PatchData { get; private set; }

    /// <summary>
    /// Constructor with a parameter.
    /// </summary>
    public PatchDataResult(byte[] patchData)
    {
      PatchData = patchData;
    }
  }
}
