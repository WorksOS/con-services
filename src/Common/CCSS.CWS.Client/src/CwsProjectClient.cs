using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  /// These use the cws-profilemanager controller
  ///   See comments in CwsAccountClient re TRN/Guid conversions
  /// </summary>
  public class CwsProjectClient : CwsProfileManagerClient, ICwsProjectClient
  {
    public CwsProjectClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    ///   Create a project with core components including boundary (not calibration file)
    /// </summary>
    public async Task<CreateProjectResponseModel> CreateProject(CreateProjectRequestModel createProjectRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(CreateProject)}: createProjectRequest {JsonConvert.SerializeObject(createProjectRequest)}");

      createProjectRequest.AccountId = TRNHelper.MakeTRN(createProjectRequest.AccountId, TRNHelper.TRN_ACCOUNT);
      var createProjectResponseModel = await PostData<CreateProjectRequestModel, CreateProjectResponseModel>($"/projects", createProjectRequest, null, customHeaders);
      createProjectResponseModel.Id = TRNHelper.ExtractGuidAsString(createProjectResponseModel.Id);

      log.LogDebug($"{nameof(CreateProject)}: createProjectResponseModel {JsonConvert.SerializeObject(createProjectResponseModel)}");
      return createProjectResponseModel;
    }

    /// <summary>
    ///   Update a project with core detail only (not calibration file and boundary)
    /// </summary>
    public async Task UpdateProjectDetails(Guid projectUid, UpdateProjectDetailsRequestModel updateProjectDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectDetails)}: projectUid {projectUid} updateProjectDetailsRequest {JsonConvert.SerializeObject(updateProjectDetailsRequest)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      await UpdateData($"/projects/{projectTrn}", updateProjectDetailsRequest, null, customHeaders);
    }

    /// <summary>
    ///   Update a project boundary
    /// </summary>
    public async Task UpdateProjectBoundary(Guid projectUid, ProjectBoundary projectBoundary, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectBoundary)}: projectUid {projectUid} projectBoundary {JsonConvert.SerializeObject(projectBoundary)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      await UpdateData($"/projects/{projectTrn}/boundary", projectBoundary, null, customHeaders);
    }
  }
}
