namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class PatchResult
  {
    public int TotalNumberOfPagesToCoverFilteredData;
    public int MaxPatchSize;
    public int PatchNumber;

    public SubgridDataPatchRecord_Elevation[] Patch;
  }
}
