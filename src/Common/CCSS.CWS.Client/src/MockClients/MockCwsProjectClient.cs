using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client.MockClients
{
  /// <summary>
  /// Mocks to use until we can get the real endpoints
  /// </summary>
  public class MockCwsProjectClient : CwsProfileManagerClient, ICwsProjectClient
  {
    public MockCwsProjectClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    public Task<ProjectDetailListResponseModel> GetProjectsForCustomer(Guid customerUid, Guid userUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForCustomer)} Mock: customerUid {customerUid} userUid {userUid}");

      var projectDetailListResponseModel = new ProjectDetailListResponseModel();

      log.LogDebug($"{nameof(GetProjectsForCustomer)} Mock: projectDetailListResponseModel {JsonConvert.SerializeObject(projectDetailListResponseModel)}");
      return Task.FromResult(projectDetailListResponseModel);
    }

    public Task<ProjectSummaryListResponseModel> GetProjectsForMyCustomer(Guid customerUid, Guid userUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForMyCustomer)} Mock: customerUid {customerUid} userUid {userUid}");

      var projectSummaryListResponseModel = new ProjectSummaryListResponseModel();

      log.LogDebug($"{nameof(GetProjectsForMyCustomer)} Mock: projectSummaryListResponseModel {JsonConvert.SerializeObject(projectSummaryListResponseModel)}");
      return Task.FromResult(projectSummaryListResponseModel);
    }

    public Task<ProjectDetailResponseModel> GetMyProject(Guid projectUid, Guid? userUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetMyProject)} Mock: projectUid {projectUid} userUid {userUid}");

      var customerUid = customHeaders?[HeaderConstants.X_VISION_LINK_CUSTOMER_UID];
      var customerTrn = TRNHelper.MakeTRN((string.IsNullOrEmpty(customerUid) ? new Guid() : new Guid(customerUid)), TRNHelper.TRN_ACCOUNT);
      var projectTrn = TRNHelper.MakeTRN(projectUid);
      var projectDetailResponseModel = new ProjectDetailResponseModel()
      {
        AccountTRN = customerTrn,
        ProjectTRN = projectTrn,
        LastUpdate = DateTime.UtcNow.AddDays(-1),
        ProjectSettings = new ProjectSettingsModel()
        {
          ProjectTRN = projectTrn,
          TimeZone = "Pacific/Auckland",
          Boundary = new ProjectBoundary() { type = "Polygon", coordinates = new List<double[,]>() { { new double[,] { { 150.3, 1.2 }, { 150.4, 1.2 }, { 150.4, 1.3 }, { 150.4, 1.4 }, { 150.3, 1.2 } } } } },
          Config = new List<ProjectConfigurationModel>()
        }
      };

      log.LogDebug($"{nameof(GetMyProject)} Mock: projectDetailResponseModel {JsonConvert.SerializeObject(projectDetailResponseModel)}");
      return Task.FromResult(projectDetailResponseModel);
    }

    public Task<CreateProjectResponseModel> CreateProject(CreateProjectRequestModel createProjectRequest, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(CreateProject)} Mock: createProjectRequest {JsonConvert.SerializeObject(createProjectRequest)}");

      var createProjectResponseModel = new CreateProjectResponseModel
      {
        TRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(), TRNHelper.TRN_PROJECT)
      };

      log.LogDebug($"{nameof(CreateProject)} Mock: createProjectResponseModel {JsonConvert.SerializeObject(createProjectResponseModel)}");
      return Task.FromResult(createProjectResponseModel);
    }

    public Task UpdateProjectDetails(Guid projectUid, UpdateProjectDetailsRequestModel updateProjectDetailsRequest, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectDetails)} Mock: projectUid {projectUid} updateProjectDetailsRequest {JsonConvert.SerializeObject(updateProjectDetailsRequest)}");
      return Task.CompletedTask;
    }

    public Task UpdateProjectBoundary(Guid projectUid, ProjectBoundary projectBoundary, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectBoundary)} Mock: projectUid {projectUid} projectBoundary {JsonConvert.SerializeObject(projectBoundary)}");
      return Task.CompletedTask;
    }
  }
}
