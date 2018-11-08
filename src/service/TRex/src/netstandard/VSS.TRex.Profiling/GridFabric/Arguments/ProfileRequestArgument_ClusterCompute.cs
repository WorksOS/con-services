using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.GridFabric.Arguments
{
  /// <summary>
  /// Defines the parameters required for a production data profile request argument on cluster compute nodes
  /// </summary>
  public class ProfileRequestArgument_ClusterCompute : BaseApplicationServiceRequestArgument
  {
    public GridDataType ProfileTypeRequired { get; set; }

    public XYZ[] NEECoords { get; set; }
    
    // todo LiftBuildSettings: TICLiftBuildSettings;
    // ExternalRequestDescriptor: TASNodeRequestDescriptor;

    public DesignDescriptor DesignDescriptor { get; set; } = DesignDescriptor.Null();

    public bool ReturnAllPassesAndLayers { get; set; } = false;

    /// <summary>
    /// Constgructs a default profile request argumnent
    /// </summary>
    public ProfileRequestArgument_ClusterCompute()
    {
    }

    /// <summary>
    /// Creates a new profile reuest argument initialised with the supplied parameters
    /// </summary>
    /// <param name="profileTypeRequired"></param>
    /// <param name="nEECoords"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="returnAllPassesAndLayers"></param>
    public ProfileRequestArgument_ClusterCompute(GridDataType profileTypeRequired, XYZ[] nEECoords,
      DesignDescriptor designDescriptor, bool returnAllPassesAndLayers)
    {
      ProfileTypeRequired = profileTypeRequired;
      NEECoords = nEECoords;
      DesignDescriptor = designDescriptor;
      ReturnAllPassesAndLayers = returnAllPassesAndLayers;
    }
  }
}
