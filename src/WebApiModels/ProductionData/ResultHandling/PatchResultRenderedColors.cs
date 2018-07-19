using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A structured representation of the data retruned by the Patch request
  /// </summary>
  public class PatchResultRenderedColors : PatchResult
  {
    /// <summary>
    /// The cells in the subgrids in the patch result have had colors rendered for the thematic data in the cells.
    /// </summary>
    [JsonProperty(PropertyName = "valuesRenderedToColors")]
    public bool ValuesRenderedToColors { get; private set; }


    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchResultRenderedColors Create(double cellSize, int numSubgridsInPatch, int totalNumPatchesRequired, bool valuesRenderedToColors, PatchSubgridResultBase[] subgrids)
    {
      return new PatchResultRenderedColors
      {
        CellSize = cellSize,
        NumSubgridsInPatch = numSubgridsInPatch,
        TotalNumPatchesRequired = totalNumPatchesRequired,
        ValuesRenderedToColors = valuesRenderedToColors,
        Subgrids = subgrids,
      };
    }
  }
}
