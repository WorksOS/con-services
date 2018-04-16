using System;
using System.IO;
using System.Linq;
using System.Net;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories;
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
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    /// <summary>
    /// Validate the coordinateSystem filename
    /// </summary>
    /// <param name="fileName">FileName</param>
    public static void ValidateFileName(string fileName)
    {
      if (fileName.Length > MAX_FILE_NAME_LENGTH || string.IsNullOrEmpty(fileName) ||
          fileName.IndexOfAny(Path.GetInvalidPathChars()) > 0 || String.IsNullOrEmpty(Path.GetFileName(fileName)))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(2),
            contractExecutionStatesEnum.FirstNameWithOffset(2)));
    }

    /// <summary>
    /// Validate the coordinateSystem filename
    /// </summary>
    public static BusinessCenterFile ValidateBusinessCentreFile(BusinessCenterFile businessCenterFile)
    {
      if (businessCenterFile == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(82),
            contractExecutionStatesEnum.FirstNameWithOffset(82)));
      }
      ProjectDataValidator.ValidateFileName(businessCenterFile.Name);

      if (string.IsNullOrEmpty(businessCenterFile.Path) || businessCenterFile.Path.Length < 5)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(83),
            contractExecutionStatesEnum.FirstNameWithOffset(83)));
      }
      // Validates the BCC file path. Checks if path contains / in the beginning and NOT at the and of the path. Otherwise add/remove it.
      if (businessCenterFile.Path[0] != '/')
        businessCenterFile.Path = businessCenterFile.Path.Insert(0, "/");
      if (businessCenterFile.Path[businessCenterFile.Path.Length - 1] == '/')
        businessCenterFile.Path = businessCenterFile.Path.Remove(businessCenterFile.Path.Length - 1);
      
      if (string.IsNullOrEmpty(businessCenterFile.FileSpaceId) || businessCenterFile.FileSpaceId.Length > 50)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(84),
            contractExecutionStatesEnum.FirstNameWithOffset(84)));
      }

      return businessCenterFile;
    }


    /// <summary>
    /// Validates the data of a specific project event
    /// </summary>
    /// <param name="evt">The event containing the data to be validated</param>
    /// <param name="repo">Project repository to use in validation</param>
    public static void Validate(IProjectEvent evt, IProjectRepository repo)
    {
      IProjectRepository projectRepo = repo;
      if (projectRepo == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(3),
            contractExecutionStatesEnum.FirstNameWithOffset(3)));
      }
      if (evt.ActionUTC == DateTime.MinValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(4),
            contractExecutionStatesEnum.FirstNameWithOffset(4)));
      }
      if (evt.ProjectUID == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(5),
            contractExecutionStatesEnum.FirstNameWithOffset(5)));
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
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(messageId),
              contractExecutionStatesEnum.FirstNameWithOffset(messageId)));
        }
        if (isCreate)
        {
          var createEvent = evt as CreateProjectEvent;

          if (string.IsNullOrEmpty(createEvent.ProjectBoundary))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(8),
                contractExecutionStatesEnum.FirstNameWithOffset(8)));
          }
          if (string.IsNullOrEmpty(createEvent.ProjectTimezone))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(9),
                contractExecutionStatesEnum.FirstNameWithOffset(9)));
          }
          if (PreferencesTimeZones.WindowsTimeZoneNames().Contains(createEvent.ProjectTimezone) == false)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(10),
                contractExecutionStatesEnum.FirstNameWithOffset(10)));
          }
          if (string.IsNullOrEmpty(createEvent.ProjectName))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(11),
                contractExecutionStatesEnum.FirstNameWithOffset(11)));
          }
          if (createEvent.ProjectName.Length > 255)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(12),
                contractExecutionStatesEnum.FirstNameWithOffset(12)));
          }
          if (!string.IsNullOrEmpty(createEvent.Description) && createEvent.Description.Length > 2000)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(13),
                contractExecutionStatesEnum.FirstNameWithOffset(13)));
          }
          if (createEvent.ProjectStartDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(14),
                contractExecutionStatesEnum.FirstNameWithOffset(14)));
          }
          if (createEvent.ProjectEndDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(15),
                contractExecutionStatesEnum.FirstNameWithOffset(15)));
          }
          if (createEvent.ProjectStartDate > createEvent.ProjectEndDate)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(16),
                contractExecutionStatesEnum.FirstNameWithOffset(16)));
          }
        }
        else if (evt is UpdateProjectEvent)
        {
          var updateEvent = evt as UpdateProjectEvent;
          if (string.IsNullOrEmpty(updateEvent.ProjectName))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(11),
                contractExecutionStatesEnum.FirstNameWithOffset(11)));
          }
          if (updateEvent.ProjectName.Length > 255)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(12),
                contractExecutionStatesEnum.FirstNameWithOffset(12)));
          }
          if (!string.IsNullOrEmpty(updateEvent.Description) && updateEvent.Description.Length > 2000)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(13),
                contractExecutionStatesEnum.FirstNameWithOffset(13)));
          }
          if (updateEvent.ProjectEndDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(15),
                contractExecutionStatesEnum.FirstNameWithOffset(15)));
          }
          var project = projectRepo.GetProjectOnly(evt.ProjectUID.ToString()).Result;
          if (project.StartDate > updateEvent.ProjectEndDate)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(16),
                contractExecutionStatesEnum.FirstNameWithOffset(16)));
          }
          if (!string.IsNullOrEmpty(updateEvent.ProjectTimezone) &&
              !project.ProjectTimeZone.Equals(updateEvent.ProjectTimezone))
          {
            throw new ServiceException(HttpStatusCode.Forbidden,
              new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(17),
                contractExecutionStatesEnum.FirstNameWithOffset(17)));
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
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(19),
              contractExecutionStatesEnum.FirstNameWithOffset(19)));
        }
        if (projectRepo.CustomerProjectExists(evt.ProjectUID.ToString()).Result)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(20),
              contractExecutionStatesEnum.FirstNameWithOffset(20)));
        }
      }
      else if (evt is DissociateProjectCustomer)
      {
        throw new ServiceException(HttpStatusCode.NotImplemented,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(21),
            contractExecutionStatesEnum.FirstNameWithOffset(21)));
      }
      else if (evt is AssociateProjectGeofence)
      {
        var associateEvent = evt as AssociateProjectGeofence;
        if (associateEvent.GeofenceUID == Guid.Empty)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(22),
              contractExecutionStatesEnum.FirstNameWithOffset(22)));
        }
      }
    }
  }
}
