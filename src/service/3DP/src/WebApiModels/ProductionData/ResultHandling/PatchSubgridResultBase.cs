using Newtonsoft.Json;
using ProtoBuf;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  [ProtoContract (SkipConstructor = true), ProtoInclude(10, typeof(PatchSubgridOriginProtobufResult))]
  public abstract class PatchSubgridResultBase
  {
    /// <summary>
    /// If true there are no non-null cells of information retruned by the query for this subgrid.
    /// </summary>
    [JsonProperty(PropertyName = "isNull")]
    protected bool IsNull { get; set; }

    /// <summary>
    /// The elevation origin referenced by all cell elevations in the binary representation of the patch subgrids. Values are expressed in meters.
    /// </summary>
    [ProtoMember(1, IsRequired = true)]
    [JsonProperty(PropertyName = "elevationOrigin")]
    protected float ElevationOrigin { get; set; }
  }
}
