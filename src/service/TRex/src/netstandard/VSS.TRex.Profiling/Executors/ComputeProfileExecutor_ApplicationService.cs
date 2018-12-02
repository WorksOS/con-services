using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Requests;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Profiling.Executors
{
  /// <summary>
  /// Executes business logic that calculates the profile between two points in space
  /// </summary>
  public class ComputeProfileExecutor_ApplicatonService
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ComputeProfileExecutor_ApplicatonService>();

    public ComputeProfileExecutor_ApplicatonService()
    { }

    /// <summary>
    /// Executes the profiler
    /// </summary>
    public ProfileRequestResponse Execute(ProfileRequestArgument_ApplicationService arg)
    {
      Log.LogInformation("Start execution");

      try
      {
        ProfileRequestArgument_ClusterCompute arg2 = new ProfileRequestArgument_ClusterCompute()
        {
          ProfileTypeRequired = arg.ProfileTypeRequired,
          ProjectID = arg.ProjectID,
          Filters = arg.Filters,
          ReferenceDesignID = arg.ReferenceDesignID,
          ReturnAllPassesAndLayers = arg.ReturnAllPassesAndLayers,
          DesignDescriptor = arg.DesignDescriptor,
          TRexNodeID = arg.TRexNodeID
        };

        // Perform coordinate conversion on the argument before broadcasting it:
        if (arg.PositionsAreGrid)
        {
          arg2.NEECoords = ConvertCoordinates.NullWGSLLToXY(new[] { arg.StartPoint, arg.EndPoint });
        }
        else
        {
          ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);

          arg2.NEECoords = ConvertCoordinates.WGS84ToCalibration(siteModel.CSIB(), new[] { arg.StartPoint, arg.EndPoint });
        }

        ProfileRequest_ClusterCompute request = new ProfileRequest_ClusterCompute();
        //ProfileRequestComputeFunc_ClusterCompute func = new ProfileRequestComputeFunc_ClusterCompute();

        ProfileRequestResponse ProfileResponse = request.Execute(arg2);

        //... and then sort them to get the final result, as well as removing initial and duplicate null values
        ProfileResponse?.ProfileCells?.OrderBy(x => x.Station);

        // Remove null cells in the profiles list. NUll cells are defined by cells with null CellLastHeight.
        // All duplicate null cells will be replaced by a by single null cell entry
        int firstNonNullIndex = 0;
        var _ProfileCells = ProfileResponse?.ProfileCells;
        if (_ProfileCells != null)
        {
          ProfileResponse.ProfileCells = _ProfileCells.Where((x, i) =>
          {
            // Remove all leading nulls
            if (((ProfileCell) _ProfileCells[i]).CellLastElev == Consts.NullHeight && i == firstNonNullIndex)
              {
                firstNonNullIndex++;
                return false;
              }

            // Collapse all interior nulls to single nulls, unless the null is at the end. Leave any single terminating null
            return i == 0 ||
                     ((ProfileCell)_ProfileCells[i]).CellLastElev != Consts.NullHeight ||
                     ((ProfileCell)_ProfileCells[i]).CellLastElev == Consts.NullHeight && ((ProfileCell)_ProfileCells[i - 1]).CellLastElev != Consts.NullHeight;
          }).ToList();
        }

        // Return the care package to the caller
        return ProfileResponse;
      }
      finally
      {
        Log.LogInformation("End execution");
      }
    }
  }
}
