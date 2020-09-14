using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A structured representation of the data by the Patch executor
  /// </summary>
  public class PatchSubgridsRawResult : ContractExecutionResult
  {
    protected PatchSubgridsRawResult()
    { }

    /// <summary>
    /// All cells in the patch are of this size. All measurements relating to the cell in the patch are made at the center point of each cell.
    /// </summary>
    public double CellSize { get; protected set; }

    /// <summary>
    /// The collection of subgrids returned in this patch request result.
    /// </summary>
    public PatchSubgridOriginProtobufResult[] Subgrids { get; protected set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchSubgridsRawResult Create(double cellSize, PatchSubgridOriginProtobufResult[] subgrids)
    {
      return new PatchSubgridsRawResult
      {
        CellSize = cellSize,        
        Subgrids = subgrids
      };
    }
  }
}
