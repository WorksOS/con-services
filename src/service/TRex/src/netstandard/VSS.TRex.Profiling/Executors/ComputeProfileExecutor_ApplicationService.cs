using System.Linq;
using System.Threading.Tasks;
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
    public async Task<ProfileRequestResponse<T>> ExecuteAsync(ProfileRequestArgument_ApplicationService arg)
    {
      Log.LogInformation("Start execution");

      try
      {
        if (arg.Filters?.Filters != null && arg.Filters.Filters.Length > 0)
        {
          // Prepare the filters for use in profiling operations. Failure to prepare any filter results in this request terminating
          if (!(arg.Filters.Filters.Select(x => FilterUtilities.PrepareFilterForUse(x, arg.ProjectID)).All(x => x.Result == RequestErrorStatus.OK)))
          {
            return new ProfileRequestResponse<T>{ResultStatus = RequestErrorStatus.FailedToPrepareFilter};
          }
        }

        var arg2 = new ProfileRequestArgument_ClusterCompute
        {
          ProfileTypeRequired = arg.ProfileTypeRequired,
          ProfileStyle = arg.ProfileStyle,
          ProjectID = arg.ProjectID,
          Filters = arg.Filters,
          ReferenceDesign = arg.ReferenceDesign,
          ReturnAllPassesAndLayers = arg.ReturnAllPassesAndLayers,
          TRexNodeID = arg.TRexNodeID,
          VolumeType = arg.VolumeType,
          Overrides = arg.Overrides,
          LiftParams = arg.LiftParams
        };

        // Perform coordinate conversion on the argument before broadcasting it:
        if (arg.PositionsAreGrid)
        {
          arg2.NEECoords = DIContext.Obtain<IConvertCoordinates>().NullWGSLLToXY(new[] { arg.StartPoint, arg.EndPoint });
        }
        else
        {
          var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);

          arg2.NEECoords = await DIContext.Obtain<IConvertCoordinates>().WGS84ToCalibration(siteModel.CSIB(), new[] { arg.StartPoint, arg.EndPoint });
        }

        var request = new ProfileRequest_ClusterCompute<T>();
        var profileResponse = await request.ExecuteAsync(arg2);

        //... and then sort them to get the final result, as well as removing initial and duplicate null values
        // Remove null cells in the profiles list. Null cells are defined by cells with null CellLastHeight.
        // All duplicate null cells will be replaced by a by single null cell entry
        int firstNonNullIndex = 0;
        var _profileCells = profileResponse?.ProfileCells?.OrderBy(x => x.Station).ToList();
        if (_profileCells != null)
        {
          profileResponse.ProfileCells = _profileCells.Where((x, i) =>
          {
            // Remove all leading nulls
            if (_profileCells[i].IsNull() && i == firstNonNullIndex)
              {
                firstNonNullIndex++;
                return false;
              }

            // Collapse all interior nulls to single nulls, unless the null is at the end. Leave any single terminating null
            return i == 0 || !_profileCells[i].IsNull() || (_profileCells[i].IsNull() && !_profileCells[i - 1].IsNull());
          }).ToList();
        }

        // Return the care package to the caller
        return profileResponse;
      }
      finally
      {
        Log.LogInformation("End execution");
      }
    }
  }
}
