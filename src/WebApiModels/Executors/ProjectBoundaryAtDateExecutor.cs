using System.Net;
using VSS.TagFileAuth.Service.WebApiModels.Interfaces;
using VSS.TagFileAuth.Service.WebApiModels.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiModels.Executors
{
  /// <summary>
  /// The executor which gets the project boundary of the project for the requested project id.
  /// </summary>
  public class ProjectBoundaryAtDateExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get project boundary request and finds active projects of the asset owner at the given date time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectBoundaryAtDateResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      GetProjectBoundaryAtDateRequest request = item as GetProjectBoundaryAtDateRequest;

      bool result = false;

      //Look for project with id request.projectId which is active at date request.tagFileUTC
      //i.e. tagFileUTC is between project start and end dates
      //and get its boundary points

      TWGS84FenceContainer fenceContainer = new TWGS84FenceContainer();
      ////Dummy data for testing
      //result = true;      
      //fenceContainer.FencePoints = new TWGS84Point[]
      //{
      //    new TWGS84Point{Lat=0.631986074660308, Lon=-2.00757760231466},
      //    new TWGS84Point{Lat=0.631907507374149, Lon=-2.00758733949739},
      //    new TWGS84Point{Lat=0.631904485465203, Lon=-2.00744352879854},
      //    new TWGS84Point{Lat=0.631987283352491, Lon=-2.00743753668608}
      //};


      if (true)//determine here if successful
      {
        return GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(result, fenceContainer);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to get project boundary"));
      }

    }

    //protected override void ProcessErrorCodes()
    //{
    //  //Nothing to do
    //}
  }
}