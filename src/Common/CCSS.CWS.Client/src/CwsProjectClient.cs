using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  /// CWS will now be the sole source of project data.
  /// Here we provide interfaces to CWS 
  /// </summary>
  public class CwsProjectClient : CwsProfileManagerClient, ICwsProjectClient
  {
    public CwsProjectClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    ///   Gets full project details using a user token.
    ///   The user must have access to this account.
    ///   Cache to include userUid as different users have access to a different project set
    ///
    ///   todo once this refactor to use cws projects is stable we may get cws team to generate a detailed list in 1 shot
    /// </summary>
    public async Task<ProjectDetailListResponseModel> GetProjectsForCustomer(Guid customerUid, Guid userUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForCustomer)}: customerUid {customerUid} userUid {userUid}");

      var projectSummaryListResponseModel = await GetProjectsForMyCustomer(customerUid, userUid, customHeaders);
      var projectDetailListReponseModel = new ProjectDetailListResponseModel();
      foreach (var project in projectSummaryListResponseModel.Projects)
      {
        // We can't query details on projects we don't have role in.
        // Return whatever info we have and caller can filter.
        if (project.UserProjectRole == UserProjectRoleEnum.Unknown)
        {
          projectDetailListReponseModel.Projects.Add(new ProjectDetailResponseModel
          {
            AccountTRN = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT),
            ProjectTRN = project.ProjectTRN,
            ProjectName = project.ProjectName,
            UserProjectRole = project.UserProjectRole
          });
          continue;
        }

        var projectDetailResponseModel = await GetMyProject(new Guid(project.ProjectId), userUid, customHeaders);
        projectDetailListReponseModel.Projects.Add(projectDetailResponseModel);
      }

      log.LogDebug($"{nameof(GetProjectsForCustomer)}: projectSummaryListResponseModel {JsonConvert.SerializeObject(projectSummaryListResponseModel)}");
      return projectDetailListReponseModel;
    }

    /// <summary>
    ///   Gets projects using a user token.
    ///   The user must have access to this account.
    ///   Cache to include userUid as different users have access to a different project set
    /// </summary>
    public async Task<ProjectSummaryListResponseModel> GetProjectsForMyCustomer(Guid customerUid, Guid userUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForMyCustomer)}: customerUid {customerUid} userUid {userUid}");

      var accountTrn = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT);
      var queryParameters = WithLimits(FromRow, RowCount);
      ProjectSummaryListResponseModel projectSummaryListResponseModel = null;
      try
      {
        projectSummaryListResponseModel = await GetData<ProjectSummaryListResponseModel>($"/accounts/{accountTrn}/projects", customerUid, userUid, queryParameters, customHeaders);
      }
      catch (HttpRequestException e)
      {
        /*
         todo what are possible exceptions?
        // account doesn't exist
        {
            "status": 403,
            "code": 9054,
            "message": "Forbidden",
            "moreInfo": "Please provide this id to support, while contacting, TraceId 5ebda82aec2fae46fed484afed7959f2",
            "timestamp": 1589487658105
        }        
        */
        log.LogError(e, $"{nameof(GetProjectsForMyCustomer)}: failed to get list of projects. ");
        throw;
      }

      log.LogDebug($"{nameof(GetProjectsForMyCustomer)}: projectSummaryListResponseModel {JsonConvert.SerializeObject(projectSummaryListResponseModel)}");
      return projectSummaryListResponseModel;
    }

    /// <summary>
    ///   Gets project using a user application token.
    ///   The user must have access to this account.
    ///   The project role depends on the user
    /// </summary>
    public async Task<ProjectDetailResponseModel> GetMyProject(Guid projectUid, Guid? userUid = null, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetMyProject)}: projectUid {projectUid} userUid {userUid}");

      var projectTrn = TRNHelper.MakeTRN(projectUid);
      ProjectDetailResponseModel projectDetailResponseModel = null;

      try
      {
        projectDetailResponseModel = await GetData<ProjectDetailResponseModel>($"/projects/{projectTrn}", projectUid, userUid, null, customHeaders);
      }
      catch (HttpRequestException e)
      {
        /*
          todo what are possible exceptions?
        */
        log.LogError(e, $"{nameof(GetMyProject)}: failed to get project. ");
        throw;
      }

      log.LogDebug($"{nameof(GetMyProject)}: projectDetailResponseModel {JsonConvert.SerializeObject(projectDetailResponseModel)}");
      return projectDetailResponseModel;
    }

    /// <summary>
    ///   Create a project with core components including boundary (not calibration file)
    /// </summary>
    public async Task<CreateProjectResponseModel> CreateProject(CreateProjectRequestModel createProjectRequest, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(CreateProject)}: createProjectRequest {JsonConvert.SerializeObject(createProjectRequest)}");

      var createProjectResponseModel = await PostData<CreateProjectRequestModel, CreateProjectResponseModel>($"/projects", createProjectRequest, null, customHeaders);

      /* todo
       BadRequest {"status":400,"code":9040,"message":"Project name is already in account","moreInfo":"Please provide this id to support, while contacting, TraceId 5ece31b18aacd06f7888ceced1a82b2e","timestamp":1590571441686,"fieldErrors":[{"field":"projectName","attemptedValue":"JeanniePMan1"}]}

      Request returned non-ok code InternalServerError with response {"status":500,"code":9047,"message":"Add project failed as project settings save failed","moreInfo":"Please provide this id to support, while contacting, TraceId 5ece3287f81ddbc95b77a00eb8e1a940","timestamp":1590571656489,"fieldErrors":[{"field":"exception","attemptedValue":"org.springframework.web.client.HttpServerErrorException: 500 null"},{"field":"Authorization","attemptedValue":"Bearer 07e73c3eb55f52fd4d0cf92f1d8d0785"},{"field":"Endpoint","attemptedValue":"https://api-stg.trimble.com/t/trimble.com/cws-profilesettings-stg/1.0/projects/{projectId}"},{"field":"Cause","attemptedValue":"500 null"},{"field":"projectId","attemptedValue":"trn::profilex:us-west-2:project:9e598d24-2e82-4641-8976-7bb8639b47fc"}]}
       */
      log.LogDebug($"{nameof(CreateProject)}: createProjectResponseModel {JsonConvert.SerializeObject(createProjectResponseModel)}");
      return createProjectResponseModel;
    }

    /// <summary>
    ///   Update a project with core detail only (not calibration file and boundary)
    /// </summary>
    public async Task UpdateProjectDetails(Guid projectUid, UpdateProjectDetailsRequestModel updateProjectDetailsRequest, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectDetails)}: projectUid {projectUid} updateProjectDetailsRequest {JsonConvert.SerializeObject(updateProjectDetailsRequest)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid);
      await UpdateData($"/projects/{projectTrn}", updateProjectDetailsRequest, null, customHeaders);
    }

    /// <summary>
    ///   Update a project boundary
    /// </summary>
    public async Task UpdateProjectBoundary(Guid projectUid, ProjectBoundary projectBoundary, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectBoundary)}: projectUid {projectUid} projectBoundary {JsonConvert.SerializeObject(projectBoundary)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid);
      await UpdateData($"/projects/{projectTrn}/boundary", projectBoundary, null, customHeaders);
    }
  }
}
