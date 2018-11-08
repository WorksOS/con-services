using Newtonsoft.Json;
using ProtoBuf;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
#pragma warning disable 1570
  /// <summary>
  /// A structured representation of the data retruned by the Patch request
  /// </summary>
  /// <remarks>
  /// In order for a consumer to use the type it's best to create a .proto file that defines the message object.
  /// e.g. string proto = Serializer.GetProto<PatchResult>();
  /// See https://github.com/mgravell/protobuf-net/blob/master/src/Examples/ProtoGeneration.cs.
  /// </remarks>
#pragma warning restore 1570
  [ProtoContract (SkipConstructor = true)]
  public class PatchResult : ContractExecutionResult
  {
    protected PatchResult()
    { }

    /// <summary>
    /// All cells in the patch are of this size. All measurements relating to the cell in the patch are made at the center point of each cell.
    /// </summary>
    [ProtoMember(1, IsRequired = true)]
    [JsonProperty(PropertyName = "cellSize")]
    public double CellSize { get; protected set; }

    /// <summary>
    /// The number of subgrids returned in this patch request
    /// </summary>
    [ProtoMember(2, IsRequired = true)]
    [JsonProperty(PropertyName = "numSubgridsInPatch")]
    public int NumSubgridsInPatch { get; protected set; }

    /// <summary>
    /// The total number of patch requests that must be made to retrieve all the information identified by the parameters of the patch query. Only returned for requests
    /// that identify patch number 0 in the set to be retrieved.
    /// </summary>
    [ProtoMember(3, IsRequired = true)]
    [JsonProperty(PropertyName = "totalNumPatchesRequired")]
    public int TotalNumPatchesRequired { get; protected set; }

    /// <summary>
    /// The collection of subgrids returned in this patch request result.
    /// </summary>
    [ProtoMember(4, IsRequired = true)]
    [JsonProperty(PropertyName = "subgrids")]
    public PatchSubgridResultBase[] Subgrids { get; protected set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchResult Create(double cellSize, int numSubgridsInPatch, int totalNumPatchesRequired, PatchSubgridResultBase[] subgrids)
    {
      return new PatchResult
      {
        CellSize = cellSize,
        NumSubgridsInPatch = numSubgridsInPatch,
        TotalNumPatchesRequired = totalNumPatchesRequired,
        Subgrids = subgrids
      };
    }
  }
}
