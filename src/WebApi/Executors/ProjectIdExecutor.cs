using System.Net;
using VSS.TagFileAuth.Service.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.WebApi.Interfaces;
using VSS.TagFileAuth.Service.WebApi.Models;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectIdExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get project id request and finds the id of the project corresponding to the given asset location and date time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectIdResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      GetProjectIdRequest request = item as GetProjectIdRequest;

      long projectId = -1;
      bool result = false;


      if (request.assetId > 0)
      {
        //Look for projects with (request.latitude, request.longitude) inside their boundary
        //and belonging to customers who have a Project Monitoring subscription
        //for asset with id request.assetId at time request.timeOfPosition 
        //and the customer owns the asset. (In VL multiple customers can have subscriptions
        //for an asset but only the owner gets the tag file data).
      }
      //VL merges this with a query for landfill projects here which can be ignored for BNA

      //If zero found then return -1
      //If one found then return its id
      //If > 1 found then return -2

      //result = true;
      //projectId = 645;  //Dummy data for testing

      if (true)//determine here if successful
      {
        return GetProjectIdResult.CreateGetProjectIdResult(result, projectId);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to get project id"));
      }

    }

    // todo what is this?
    //protected override void ProcessErrorCodes()
    //{
    //  //Nothing to do
    //}
  }
}
