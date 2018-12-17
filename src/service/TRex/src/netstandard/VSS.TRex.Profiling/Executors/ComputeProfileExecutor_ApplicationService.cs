using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Requests;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.Executors
{
  /// <summary>
  /// Executes business logic that calculates the profile between two points in space
  /// </summary>
  public class ComputeProfileExecutor_ApplicationService<T> where T: class, IProfileCellBase, new()
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ComputeProfileExecutor_ApplicationService<T>>();

    public ComputeProfileExecutor_ApplicationService()
    { }

    /// <summary>
    /// Executes the profiler
    /// </summary>
    public ProfileRequestResponse<T> Execute(ProfileRequestArgument_ApplicationService arg)
    {
      Log.LogInformation("Start execution");

      try
      {
        if (arg.Filters?.Filters != null && arg.Filters.Filters.Length > 0)
        {
          // Prepare the filters for use in profiling operations. Failure to prepare any filter results in this request terminating
          if (false == arg.Filters.Filters.Select(x => FilterUtilities.PrepareFilterForUse(x, arg.ProjectID)).Any(x => x != RequestErrorStatus.OK))
          {
            return new ProfileRequestResponse<T>{ResultStatus = RequestErrorStatus.FailedToPrepareFilter};
          }
        }

        ProfileRequestArgument_ClusterCompute arg2 = new ProfileRequestArgument_ClusterCompute
        {
          ProfileTypeRequired = arg.ProfileTypeRequired,
          ProjectID = arg.ProjectID,
          Filters = arg.Filters,
          ReferenceDesignUID = arg.ReferenceDesignUID,
          ReturnAllPassesAndLayers = arg.ReturnAllPassesAndLayers,
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

        ProfileRequest_ClusterCompute<T> request = new ProfileRequest_ClusterCompute<T>();
        ProfileRequestResponse<T> ProfileResponse = request.Execute(arg2);

        //... and then sort them to get the final result, as well as removing initial and duplicate null values
        // Remove null cells in the profiles list. Null cells are defined by cells with null CellLastHeight.
        // All duplicate null cells will be replaced by a by single null cell entry
        int firstNonNullIndex = 0;
        var _ProfileCells = ProfileResponse?.ProfileCells?.OrderBy(x => x.Station).ToList();
        if (_ProfileCells != null)
        {
          ProfileResponse.ProfileCells = _ProfileCells.Where((x, i) =>
          {
            // Remove all leading nulls
            if (_ProfileCells[i].IsNull() && i == firstNonNullIndex)
              {
                firstNonNullIndex++;
                return false;
              }

            // Collapse all interior nulls to single nulls, unless the null is at the end. Leave any single terminating null
            return i == 0 || !_ProfileCells[i].IsNull() || (_ProfileCells[i].IsNull() && !_ProfileCells[i - 1].IsNull());
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
