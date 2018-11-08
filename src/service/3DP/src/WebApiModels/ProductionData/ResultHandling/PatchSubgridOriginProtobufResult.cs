using Newtonsoft.Json;
using ProtoBuf;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A subgrid of information within a patch result
  /// </summary>
  [ProtoContract(SkipConstructor = true)]
  public class PatchSubgridOriginProtobufResult : PatchSubgridResultBase
  {
    /// <summary>
    /// Gets the northing patch origin in meters, as a delta.
    /// </summary>
    [ProtoMember(1, IsRequired = true)]
    [JsonProperty(PropertyName = "worldOriginX")]
    public double SubgridOriginX { get; private set; }

    /// <summary>
    /// Gets the easting patch origin in meters, as a delta.
    /// </summary>
    [ProtoMember(2, IsRequired = true)]
    [JsonProperty(PropertyName = "worldOriginY")]
    public double SubgridOriginY { get; private set; }

    /// <summary>
    /// The grid of cells that make up this subgrid in the patch
    /// </summary>
    /// <remarks>
    /// This class differs from <see cref="PatchSubgridOriginResult"/> in the way Cells is defined. Protobuf cannot serialize multi dimensional arrays so
    /// in this case we linearize it.
    ///
    /// When the CellController 'api/v1/productiondata/patches/worldorigin' endpoint is no longer required for testing by the CTCT team we can remove it and
    /// remove <see cref="PatchSubgridOriginResult"/> and associated methods/classes.
    /// </remarks>
    [ProtoMember(3, IsRequired = true)]
    [JsonProperty(PropertyName = "cells")]
    protected PatchCellHeightResult[] Cells { get; set; }

    /// <summary>
    /// The elevation origin referenced by all cell elevations in the binary representation of the patch subgrids.
    /// UTC expressed as Unix time in seconds.
    /// </summary>
    [ProtoMember(4, IsRequired = true)]
    [JsonProperty(PropertyName = "timeOrigin")]
    protected uint TimeOrigin { get; set; }

    public bool ShouldSerializeTimeOrigin()
    {
      return TimeOrigin != uint.MaxValue;
    }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchSubgridOriginProtobufResult Create(double subgridOriginX, double subgridOriginY, float elevationOrigin, uint timeOrigin, PatchCellHeightResult[] cells)
    {
      return new PatchSubgridOriginProtobufResult
      {
        SubgridOriginX = subgridOriginX,
        SubgridOriginY = subgridOriginY,
        ElevationOrigin = elevationOrigin,
        TimeOrigin = timeOrigin,
        Cells = cells
      };
    }
  }
}
