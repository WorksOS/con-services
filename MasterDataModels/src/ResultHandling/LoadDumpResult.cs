using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Data returned from unified productivity showing load/dump locations of asset cycles.
  /// </summary>
  public class LoadDumpResult
  {
    /// <summary>
    /// Cycle data from unified productivity.
    /// </summary>
    public List<LoadDumpLocation> cycles;
  }
}
