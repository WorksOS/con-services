using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Data returned from unified productivity showing load/dump locations of asset cycles.
  /// </summary>
  public class LoadDumpResult : IMasterDataModel
  {
    /// <summary>
    /// Cycle data from unified productivity.
    /// </summary>
    public List<LoadDumpLocation> cycles;

    public List<string> GetIdentifiers() => new List<string>();
  }
}
