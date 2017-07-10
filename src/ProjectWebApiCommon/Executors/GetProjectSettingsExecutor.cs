using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Repositories;
using VSS.GenericConfiguration;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.ProjectWebApiCommon.Executors
{
  /// <summary>
  /// The executor which gets the project settings for the project
  /// </summary>
  public class GetProjectSettingsExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetProjectSettingsExecutor(IRepository<IProjectEvent> projectRepo, ILoggerFactory logger, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler) : base(projectRepo, configStore, logger, serviceExceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetProjectSettingsExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }

    /// <summary>
    /// Processes the GetProjectSettings request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ProjectSettingsResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
        string projectUid = item as string;

        var projectSettings =
          // (await projectRepo.GetProject(projectUid).ConfigureAwait(false));
          // todo temp until dbRepo done
          ProjectSettingsResult.CreateProjectSettingsResult(
            projectUid,
            "<ProjectSettings> <VolumeSettings>< ApplyShrinkageAndBulking > false </ ApplyShrinkageAndBulking ></ ProjectSettings >"
          );
        result = ProjectSettingsResult.CreateProjectSettingsResult(projectUid, projectSettings.Settings);
      }
      catch( Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68, e.Message);
      }
      return result;
    }

    protected override void ProcessErrorCodes()
    {
    }
   
  }
}