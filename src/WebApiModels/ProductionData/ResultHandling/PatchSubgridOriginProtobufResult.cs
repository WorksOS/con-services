using Newtonsoft.Json;
using ProtoBuf;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A subgrid of information within a patch result
  /// </summary>
  [ProtoContract (SkipConstructor = true)]
  public class PatchSubgridOriginProtobufResult : PatchSubgridResultBase
  {
    private PatchSubgridOriginProtobufResult()
    { }

    /// <summary>
    /// Gets the northing patch origin in meters, as a delta.
    /// </summary>
    [ProtoMember(1, IsRequired = true)]
    [JsonProperty(PropertyName = "patchOriginN")]
    public double PatchOriginN { get; private set; }

    /// <summary>
    /// Gets the easting patch origin in meters, as a delta.
    /// </summary>
    [ProtoMember(2, IsRequired = true)]
    [JsonProperty(PropertyName = "patchOriginE")]
    public double PatchOriginE { get; private set; }

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
    /// Static constructor.
    /// </summary>
    public static PatchSubgridOriginProtobufResult Create(double patchOriginN, double patchOriginE, bool isNull, float elevationOrigin, PatchCellHeightResult[] cells)
    {
      return new PatchSubgridOriginProtobufResult
      {
        PatchOriginN = patchOriginN,
        PatchOriginE = patchOriginE,
        IsNull = isNull,
        ElevationOrigin = elevationOrigin,
        Cells = cells
      };
    }
  }
}
