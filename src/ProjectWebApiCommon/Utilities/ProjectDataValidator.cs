using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  /// <summary>
  /// Validates all project event data sent to the Web API
  /// </summary>
  public class ProjectDataValidator 
  {
    private const int MAX_FILE_NAME_LENGTH = 256;
    protected static ProjectErrorCodesProvider projectErrorCodesProvider = new ProjectErrorCodesProvider();

    /// <summary>
    /// Validate the coordinateSystem filename
    /// </summary>
    /// <param name="fileName">FileName</param>
    public static void ValidateFileName(string fileName)
    {
      if (fileName.Length > MAX_FILE_NAME_LENGTH || string.IsNullOrEmpty(fileName) ||
          fileName.IndexOfAny(Path.GetInvalidPathChars()) > 0 || String.IsNullOrEmpty(Path.GetFileName(fileName)))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(2),
            projectErrorCodesProvider.FirstNameWithOffset(2)));
    }

    /// <summary>
    /// Validate list of geofenceTypes
    /// </summary>
    /// <param name="geofenceTypes">FileName</param>
    /// <param name="projectType"></param>
    public static bool ValidateGeofenceTypes(List<GeofenceType> geofenceTypes, ProjectType? projectType = null)
    {
      if (geofenceTypes == null || geofenceTypes.Count == 0 )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(73),
            projectErrorCodesProvider.FirstNameWithOffset(73)));
      }

      if (geofenceTypes.Count != 1 || geofenceTypes[0] != GeofenceType.Landfill)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(102),
            projectErrorCodesProvider.FirstNameWithOffset(102)));
      }

      // future-proofing here, geofence types will be dependant on projectType
      if (projectType != null && projectType != ProjectType.LandFill )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(102),
            projectErrorCodesProvider.FirstNameWithOffset(102)));
      }

      return true;
    }

    /// <summary>
    /// Validate the coordinateSystem filename
    /// </summary>
    public static BusinessCenterFile ValidateBusinessCentreFile(BusinessCenterFile businessCenterFile)
    {
      if (businessCenterFile == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(82),
            projectErrorCodesProvider.FirstNameWithOffset(82)));
      }
      ProjectDataValidator.ValidateFileName(businessCenterFile.Name);

      if (string.IsNullOrEmpty(businessCenterFile.Path) || businessCenterFile.Path.Length < 5)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(83),
            projectErrorCodesProvider.FirstNameWithOffset(83)));
      }
      // Validates the BCC file path. Checks if path contains / in the beginning and NOT at the and of the path. Otherwise add/remove it.
      if (businessCenterFile.Path[0] != '/')
        businessCenterFile.Path = businessCenterFile.Path.Insert(0, "/");
      if (businessCenterFile.Path[businessCenterFile.Path.Length - 1] == '/')
        businessCenterFile.Path = businessCenterFile.Path.Remove(businessCenterFile.Path.Length - 1);
      
      if (string.IsNullOrEmpty(businessCenterFile.FileSpaceId) || businessCenterFile.FileSpaceId.Length > 50)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(84),
            projectErrorCodesProvider.FirstNameWithOffset(84)));
      }

      return businessCenterFile;
    }


    /// <summary>
    /// Validates the data of a specific project event
    /// </summary>
    /// <param name="evt">The event containing the data to be validated</param>
    /// <param name="repo">Project repository to use in validation</param>
    /// <param name="serviceExceptionHandler"></param>
    public static void Validate(IProjectEvent evt, IProjectRepository repo, IServiceExceptionHandler serviceExceptionHandler)
    {
      IProjectRepository projectRepo = repo;
      if (projectRepo == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(3),
            projectErrorCodesProvider.FirstNameWithOffset(3)));
      }
      if (evt.ActionUTC == DateTime.MinValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(4),
            projectErrorCodesProvider.FirstNameWithOffset(4)));
      }
      if (evt.ProjectUID == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(5),
            projectErrorCodesProvider.FirstNameWithOffset(5)));
      }
      //Note: don't check if project exists for associate events.
      //We don't know the workflow for NG so associate may come before project creation.
      bool checkExists = evt is CreateProjectEvent || evt is UpdateProjectEvent || evt is DeleteProjectEvent;
      if (checkExists)
      {
        bool exists = projectRepo.ProjectExists(evt.ProjectUID.ToString()).Result;
        bool isCreate = evt is CreateProjectEvent;
        if ((isCreate && exists) || (!isCreate && !exists))
        {
          var messageId = isCreate ? 6 : 7;
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(messageId),
              projectErrorCodesProvider.FirstNameWithOffset(messageId)));
        }
        if (isCreate)
        {
          var createEvent = evt as CreateProjectEvent;

          if (string.IsNullOrEmpty(createEvent.ProjectBoundary))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(8),
                projectErrorCodesProvider.FirstNameWithOffset(8)));
          }
          
          ProjectRequestHelper.ValidateProjectBoundary(createEvent.ProjectBoundary, serviceExceptionHandler);

          if (string.IsNullOrEmpty(createEvent.ProjectTimezone))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(9),
                projectErrorCodesProvider.FirstNameWithOffset(9)));
          }
          if (PreferencesTimeZones.WindowsTimeZoneNames().Contains(createEvent.ProjectTimezone) == false)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(10),
                projectErrorCodesProvider.FirstNameWithOffset(10)));
          }
          if (string.IsNullOrEmpty(createEvent.ProjectName))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(11),
                projectErrorCodesProvider.FirstNameWithOffset(11)));
          }
          if (createEvent.ProjectName.Length > 255)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(12),
                projectErrorCodesProvider.FirstNameWithOffset(12)));
          }
          if (!string.IsNullOrEmpty(createEvent.Description) && createEvent.Description.Length > 2000)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(13),
                projectErrorCodesProvider.FirstNameWithOffset(13)));
          }
          if (createEvent.ProjectStartDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(14),
                projectErrorCodesProvider.FirstNameWithOffset(14)));
          }
          if (createEvent.ProjectEndDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(15),
                projectErrorCodesProvider.FirstNameWithOffset(15)));
          }
          if (createEvent.ProjectStartDate > createEvent.ProjectEndDate)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(16),
                projectErrorCodesProvider.FirstNameWithOffset(16)));
          }
        }
        else if (evt is UpdateProjectEvent)
        {
          var updateEvent = evt as UpdateProjectEvent;
          if (string.IsNullOrEmpty(updateEvent.ProjectName))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(11),
                projectErrorCodesProvider.FirstNameWithOffset(11)));
          }
          if (updateEvent.ProjectName.Length > 255)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(12),
                projectErrorCodesProvider.FirstNameWithOffset(12)));
          }
          if (!string.IsNullOrEmpty(updateEvent.Description) && updateEvent.Description.Length > 2000)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(13),
                projectErrorCodesProvider.FirstNameWithOffset(13)));
          }
          if (updateEvent.ProjectEndDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(15),
                projectErrorCodesProvider.FirstNameWithOffset(15)));
          }
          var project = projectRepo.GetProjectOnly(evt.ProjectUID.ToString()).Result;
          if (project.StartDate > updateEvent.ProjectEndDate)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(16),
                projectErrorCodesProvider.FirstNameWithOffset(16)));
          }
          if (!string.IsNullOrEmpty(updateEvent.ProjectTimezone) &&
              !project.ProjectTimeZone.Equals(updateEvent.ProjectTimezone))
          {
            throw new ServiceException(HttpStatusCode.Forbidden,
              new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(17),
                projectErrorCodesProvider.FirstNameWithOffset(17)));
          }
          if (!string.IsNullOrEmpty(updateEvent.ProjectBoundary))
          {
            ProjectRequestHelper.ValidateProjectBoundary(updateEvent.ProjectBoundary, serviceExceptionHandler);
          }
        }
        //Nothing else to check for DeleteProjectEvent
      }
      else if (evt is AssociateProjectCustomer)
      {
        var associateEvent = evt as AssociateProjectCustomer;
        if (associateEvent.CustomerUID == Guid.Empty)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(19),
              projectErrorCodesProvider.FirstNameWithOffset(19)));
        }
        if (projectRepo.CustomerProjectExists(evt.ProjectUID.ToString()).Result)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(20),
              projectErrorCodesProvider.FirstNameWithOffset(20)));
        }
      }
      else if (evt is DissociateProjectCustomer)
      {
        throw new ServiceException(HttpStatusCode.NotImplemented,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(21),
            projectErrorCodesProvider.FirstNameWithOffset(21)));
      }
      else if (evt is AssociateProjectGeofence)
      {
        var associateEvent = evt as AssociateProjectGeofence;
        if (associateEvent.GeofenceUID == Guid.Empty)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(22),
              projectErrorCodesProvider.FirstNameWithOffset(22)));
        }
      }
    }


    /// <summary>
    /// Seee if any free subs for a project type
    /// </summary>
    /// <param name="customerUid">The customer uid.</param>
    /// <param name="projectType"></param>
    /// <param name="log"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="subscriptionRepo"></param>
    /// <returns></returns>
    public static async Task ValidateFreeSub(string customerUid, ProjectType projectType,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      ISubscriptionRepository subscriptionRepo)
    {
      if (projectType == ProjectType.LandFill || projectType == ProjectType.ProjectMonitoring)
      {
        var availableFreeSub =
          (await subscriptionRepo.GetFreeProjectSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date)
            .ConfigureAwait(false))
          .Where(s => s.ServiceTypeID == (int)projectType.MatchSubscriptionType()).ToImmutableList();

        if (!availableFreeSub.Any())
        {
          log.LogInformation($"There are no free subscriptions for project type {projectType}");
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 37);
        }
      }
    }

    /// <summary>
    /// Validates a projectname. Must be unique amoungst active projects for the Customer.
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="projectName"></param>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="log"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="projectRepo"></param>
    /// <returns></returns>
    public static async Task ValidateProjectName(string customerUid, string projectName, string projectUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      IProjectRepository projectRepo)
    {
      log.LogInformation($"ValidateProjectName projectName: {projectName} projectUid: {projectUid}");
      var duplicateProjectNames =
        (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false))
        .Where(
          p => p.IsDeleted == false &&
                string.Equals(p.Name, projectName, StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase))
        .ToList();
      if (duplicateProjectNames.Any())
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 109, $"Count:{duplicateProjectNames.Count} projectUid: {duplicateProjectNames[0].ProjectUID}");
      }

      log.LogInformation($"ValidateProjectName Any duplicateProjectNames? {JsonConvert.SerializeObject(duplicateProjectNames)} retrieved");
    }
   
  }
}
