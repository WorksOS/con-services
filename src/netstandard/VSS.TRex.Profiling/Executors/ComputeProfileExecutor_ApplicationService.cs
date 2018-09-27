using System.Linq;
using Microsoft.Extensions.Logging;
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
    {
    }

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
          arg2.NEECoords = CoordinateSystems.Convert.NullWGSLLToXY(new[] {arg.StartPoint, arg.EndPoint});
        else
        {
          ISiteModel SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);

          arg2.NEECoords =
            CoordinateSystems.Convert.WGS84ToCalibration(SiteModel.CSIB(), new[] {arg.StartPoint, arg.EndPoint});
        }

        ProfileRequest_ClusterCompute request = new ProfileRequest_ClusterCompute();
        //ProfileRequestComputeFunc_ClusterCompute func = new ProfileRequestComputeFunc_ClusterCompute();

        ProfileRequestResponse ProfileResponse = request.Execute(arg2);

        //... and then sort them to get the final result
        ProfileResponse?.ProfileCells?.OrderBy(x => x.Station);

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
