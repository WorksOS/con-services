using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Designs;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.GridFabric.Arguments
{
  /// <summary>
  /// Defines the parameters required for a production data profile request argument on the application service node
  /// </summary>
    public class ProfileRequestArgument_ApplicationService : BaseApplicationServiceRequestArgument
    {
      public GridDataType ProfileTypeRequired { get; set; }

      public WGS84Point StartPoint = new WGS84Point(0, 0);
      public WGS84Point EndPoint = new WGS84Point(0, 0);

      public bool PositionsAreGrid { get; set; } = false;

    // todo LiftBuildSettings: TICLiftBuildSettings;
    // ExternalRequestDescriptor: TASNodeRequestDescriptor;

      public DesignDescriptor DesignDescriptor { get; set; } = DesignDescriptor.Null();

      public bool ReturnAllPassesAndLayers { get; set; } = false;

      /// <summary>
      /// Constgructs a default profile request argumnent
      /// </summary>
      public ProfileRequestArgument_ApplicationService()
      {
      }

      /// <summary>
      /// Creates a new profile reuest argument initialised with the supplied parameters
      /// </summary>
      /// <param name="profileTypeRequired"></param>
      /// <param name="startPoint"></param>
      /// <param name="endPoint"></param>
      /// <param name="positionsAreGrid"></param>
      /// <param name="designDescriptor"></param>
      /// <param name="returnAllPassesAndLayers"></param>
      public ProfileRequestArgument_ApplicationService(GridDataType profileTypeRequired, WGS84Point startPoint, WGS84Point endPoint, bool positionsAreGrid, DesignDescriptor designDescriptor, bool returnAllPassesAndLayers)
      {
        ProfileTypeRequired = profileTypeRequired;
        StartPoint = startPoint;
        EndPoint = endPoint;
        PositionsAreGrid = positionsAreGrid;
        DesignDescriptor = designDescriptor;
        ReturnAllPassesAndLayers = returnAllPassesAndLayers;
      }
  }
}
