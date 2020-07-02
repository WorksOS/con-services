using System;
using System.Collections.Generic;
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
    ///   cws team to generate a detailed list in 1 shot CCSSSCON-409
    /// </summary>
    public async Task<ProjectDetailListResponseModel> GetProjectsForCustomer(Guid customerUid, Guid? userUid = null, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForCustomer)}: customerUid {customerUid} userUid {userUid}");

      var projectSummaryListResponseModel = await GetProjectsForMyCustomer(customerUid, userUid, customHeaders);
      var projectDetailListResponseModel = new ProjectDetailListResponseModel();
      foreach (var project in projectSummaryListResponseModel.Projects)
      {
        // Convert the summary model into a details model.
        //If the project doesn't belong to the user and the user is not admin there will not be a boundary.
        //We can get the boundary (currently only 3dp projects) using metadata.
        ProjectDetailResponseModel details = null;
        if (project.Boundary == null)
        {
          details = await GetProject(new Guid(project.ProjectId), userUid, true, customHeaders);
        }
        else
        {
          details = new ProjectDetailResponseModel
          {
            AccountTRN = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT),
            ProjectTRN = project.ProjectTRN,
            ProjectName = project.ProjectName,
            ProjectType = project.ProjectType,
            Status = project.Status,
            UserProjectRole = project.UserProjectRole,
            ProjectSettings = new ProjectSettingsModel {Boundary = project.Boundary, TimeZone = project.TimeZone}
          };
        }
        projectDetailListResponseModel.Projects.Add(details);
      }

      log.LogDebug($"{nameof(GetProjectsForCustomer)}: projectSummaryListResponseModel {JsonConvert.SerializeObject(projectSummaryListResponseModel)}");
      return projectDetailListResponseModel;
    }

    /// <summary>
    ///   Gets projects using a user token.
    ///   The user must have access to this account.
    ///   Cache to include userUid as different users have access to a different project set
    ///   This ONLY works with a user token.
    /// </summary>
    public async Task<ProjectSummaryListResponseModel> GetProjectsForMyCustomer(Guid customerUid, Guid? userUid = null, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectsForMyCustomer)}: customerUid {customerUid} userUid {userUid}");

      var accountTrn = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT);
      ProjectSummaryListResponseModel projectSummaryListResponseModel;

      try
      {
        projectSummaryListResponseModel = await GetAllPagedData<ProjectSummaryListResponseModel, ProjectSummaryResponseModel>($"/accounts/{accountTrn}/projects", customerUid, userUid, null, customHeaders);
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
    ///   This ONLY works with a user token.
    /// </summary>
    public async Task<ProjectDetailResponseModel> GetMyProject(Guid projectUid, Guid? userUid = null, IHeaderDictionary customHeaders = null)
    {
      return await GetProject(projectUid, userUid, false, customHeaders);
    }

    /// <summary>
    /// Get project details. Metadata required if getting project boundary for a project not belonging to user who is not an admin.
    /// </summary>
    private async Task<ProjectDetailResponseModel> GetProject(Guid projectUid, Guid? userUid = null, bool useMetadata = false, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProject)}: projectUid {projectUid} userUid {userUid}");

      var projectTrn = TRNHelper.MakeTRN(projectUid);
      ProjectDetailResponseModel projectDetailResponseModel = null;

      try
      {
        var route = $"/projects/{projectTrn}";
        if (useMetadata)
          route = $"{route}/metadata";
        projectDetailResponseModel = await GetData<ProjectDetailResponseModel>(route, projectUid, userUid, null, customHeaders);

        // get a project, with user role always returns null, but can only be called if the role IS ADMIN.
        if (userUid != null && !useMetadata)
          projectDetailResponseModel.UserProjectRole = UserProjectRoleEnum.Admin;
      }
      catch (HttpRequestException e)
      {
        /*
          todo what are possible exceptions?
        */
        log.LogError(e, $"{nameof(GetProject)}: failed to get project. ");
        throw;
      }

      log.LogDebug($"{nameof(GetProject)}: projectDetailResponseModel {JsonConvert.SerializeObject(projectDetailResponseModel)}");
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
       BadRequest {"status":400,"code":9040,"message":"Project name is already in account","moreInfo":"Please provide this id to support, while contacting, TraceId 5ece31b18aacd06f7888ceced1a82b2e","timestamp":1590571441686,"fieldErrors":[{"field":"projectName","attemptedValue":"wotevaPMan1"}]}

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
