using System;

namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Comntains the prepared result for the client to consume
  /// </summary>
  [Serializable]
    public class PatchResult
  {
    public int TotalNumberOfPagesToCoverFilteredData;
    public int MaxPatchSize;
    public int PatchNumber;

    public SubgridDataPatchRecord_Elevation[] Patch;
  }
}
