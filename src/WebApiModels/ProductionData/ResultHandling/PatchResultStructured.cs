using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A structured representation of the data retruned by the Patch request
  /// </summary>
  public class PatchResultStructured : ContractExecutionResult
  {
    private PatchResultStructured()
    { }

    /// <summary>
    /// All cells in the patch are of this size. All measurements relating to the cell in the patch are made at the center point of each cell.
    /// </summary>
    [JsonProperty(PropertyName = "cellSize")]
    public double CellSize { get; private set; }

    /// <summary>
    /// The number of subgrids returned in this patch request
    /// </summary>
    [JsonProperty(PropertyName = "numSubgridsInPatch")]
    public int NumSubgridsInPatch { get; private set; }

    /// <summary>
    /// The total number of patch requests that must be made to retrieve all the information identified by the parameters of the patch query. Only returned for requests
    /// that identify patch number 0 in the set to be retrieved.
    /// </summary>
    [JsonProperty(PropertyName = "totalNumPatchesRequired")]
    public int TotalNumPatchesRequired { get; private set; }

    /// <summary>
    /// The cells in theh subgrids in the patch result have had colors rendered for the thematic data in the cells.
    /// </summary>
    [JsonProperty(PropertyName = "valuesRenderedToColors")]
    public bool ValuesRenderedToColors { get; private set; }

    /// <summary>
    /// The collection of subgrids returned in this patch request result.
    /// </summary>
    [JsonProperty(PropertyName = "subgrids")]
    public PatchSubgridResultBase[] Subgrids { get; private set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchResultStructured CreatePatchResultStructured(double cellSize, int numSubgridsInPatch, int totalNumPatchesRequired, bool valuesRenderedToColors, PatchSubgridResultBase[] subgrids)
    {
      return new PatchResultStructured
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
