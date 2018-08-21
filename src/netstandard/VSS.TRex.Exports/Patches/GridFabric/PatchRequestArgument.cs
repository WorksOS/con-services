using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.Models.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// The argument to be supplied to the Patchs request
  /// </summary>
  [Serializable]
  public class PatchRequestArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// The type of data requested for the patch. Single attribute only, expressed as the
    /// user-space display mode of the data
    /// </summary>
    public DisplayMode Mode { get; set; }

    private DesignDescriptor DesignDescriptor { get; set; }

    // FReferenceVolumeType : TComputeICVolumesType;

    // FICOptions : TSVOICOptions;

    /// <summary>
    /// The number of the patch of subgrids being requested within the overall set of patches that comprise the request
    /// </summary>
    public int DataPatchNumber { get; set; }

    /// <summary>
    /// The maximum number of subgrids to be returned in each patch of subgrids
    /// </summary>
    public int DataPatchSize { get; set; }
  }
}
