using System.Net;
using VSS.TagFileAuth.Service.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.WebApi.Interfaces;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.Executors
{
  /// <summary>
  /// The executor which gets a list of project boundaries for the requested asset id.
  /// </summary>
  public class ProjectBoundariesAtDateExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get project boundaries request and finds active projects of the asset owner at the given date time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectBoundariesAtDateResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      GetProjectBoundariesAtDateRequest request = item as GetProjectBoundariesAtDateRequest;

      bool result = false;

      //Check valid asset Id i.e. asset Id > 0

      //Find the dealer or customer who is the owner of the asset 

      //Look for projects which are active at date time request.tagFileUTC
      //i.e. tagFileUTC is between project start and end dates
      //and which belong to the asset owner and get their boundary points

      //Dummy data for testing
      result = true;
      ProjectBoundaryPackage[] boundaries = new ProjectBoundaryPackage[]
      {
        new ProjectBoundaryPackage{
          Boundary = new TWGS84FenceContainer
          {
            FencePoints = new TWGS84Point[]
            {
              new TWGS84Point{Lat=0.631986074660308, Lon=-2.00757760231466},
              new TWGS84Point{Lat=0.631907507374149, Lon=-2.00758733949739},
              new TWGS84Point{Lat=0.631904485465203, Lon=-2.00744352879854},
              new TWGS84Point{Lat=0.631987283352491, Lon=-2.00743753668608}
            }
          },
          ProjectID = 726
        }};

      if (true)//determine here if successful
      {
        return GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(result, boundaries);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to get project boundaries"));
      }

    }

    //protected override void ProcessErrorCodes()
    //{
    //  //Nothing to do
    //}
  }
}