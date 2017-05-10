using System;
using System.Net;
using Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectWebApiCommon.ResultsHandling;
using System.IO;

namespace ProjectWebApiCommon.Models
{
  /// <summary>
  /// Validates all project event data sent to the Web API
  /// </summary>
  public class ProjectDataValidator
  {
    private const int MAX_FILE_NAME_LENGTH = 256;

    /// <summary>
    /// Validate the coordinateSystem filename
    /// </summary>
    /// <param name="fileName">FileName</param>
    public static void ValidateFileName(string fileName)
    {
      if (fileName.Length > MAX_FILE_NAME_LENGTH || string.IsNullOrEmpty(fileName) || fileName.IndexOfAny(Path.GetInvalidPathChars()) > 0 || String.IsNullOrEmpty(Path.GetFileName(fileName)))
        throw new ServiceException(HttpStatusCode.InternalServerError,
           new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                                          String.Format("Supplied CoordinateSystem filename is not valid. Exceeds the length limit of {0}, empty or contains illegal characters.", MAX_FILE_NAME_LENGTH)));      
    }

    /// <summary>
    /// Validates the data of a specific project event
    /// </summary>
    /// <param name="evt">The event containing the data to be validated</param>
    /// <param name="repo">Project repository to use in validation</param>
    public static void Validate(IProjectEvent evt, IRepository<IProjectEvent> repo, string headerCustomerUid)
    {
      var projectRepo = repo as ProjectRepository;
      if (projectRepo == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing Project Repository in ProjectDataValidator.Validate"));
      }
      if (evt.ActionUTC == DateTime.MinValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing ActionUTC"));
      }
      if (evt.ProjectUID == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing ProjectUID"));
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
          string message = isCreate ? "Project already exists" : "Project does not exist";
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              message));
        }
        if (isCreate)
        {
          var createEvent = evt as CreateProjectEvent;

          if (string.IsNullOrEmpty(createEvent.ProjectBoundary))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing ProjectBoundary"));
          }
          if (string.IsNullOrEmpty(createEvent.ProjectTimezone))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing ProjectTimezone"));
          }
          if (string.IsNullOrEmpty(createEvent.ProjectName))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing ProjectName"));
          }
          if (createEvent.ProjectName.Length > 255)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "ProjectName is longer than the 255 characters allowed"));
          }
          if (!string.IsNullOrEmpty(createEvent.Description) && createEvent.Description.Length > 2000)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Description is longer than the 2000 characters allowed"));
          }
          if (createEvent.ProjectStartDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing ProjectStartDate"));
          }
          if (createEvent.ProjectEndDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing ProjectEndDate"));
          }
          if (createEvent.ProjectEndDate < DateTime.UtcNow)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "ProjectEndDate must be in the future"));
          }
          if (createEvent.ProjectStartDate > createEvent.ProjectEndDate)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Start date must be earlier than end date"));
          }
        }
        else if (evt is UpdateProjectEvent)
        {
          var updateEvent = evt as UpdateProjectEvent;
          if (string.IsNullOrEmpty(updateEvent.ProjectName))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "ProjectName cannot be empty"));
          }
          if (updateEvent.ProjectName.Length > 255)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "ProjectName is longer than the 255 characters allowed"));
          }
          if (!string.IsNullOrEmpty(updateEvent.Description) && updateEvent.Description.Length > 2000)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Description is longer than the 2000 characters allowed"));
          }
          if (updateEvent.ProjectEndDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "ProjectEndDate cannot be empty"));
          }
          var project = projectRepo.GetProjectOnly(evt.ProjectUID.ToString()).Result;
          if (project.StartDate > updateEvent.ProjectEndDate)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "ProjectEndDate must be later than start date"));
          }
          if (!string.IsNullOrEmpty(updateEvent.ProjectTimezone) && !project.ProjectTimeZone.Equals(updateEvent.ProjectTimezone))
          {
            throw new ServiceException(HttpStatusCode.Forbidden,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Project timezone cannot be updated"));
          }
        }
        //Nothing else to check for DeleteProjectEvent
      }
      else if (evt is AssociateProjectCustomer)
      {
        var associateEvent = evt as AssociateProjectCustomer;

        if (associateEvent.CustomerUID.ToString() != headerCustomerUid)
        {
          var error = $"CustomerUid {associateEvent.CustomerUID.ToString()} differs to the requesting CustomerUid {headerCustomerUid}. Impersonation not supported.";
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
        }
        if (associateEvent.CustomerUID == Guid.Empty)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Missing CustomerUID"));
        }
        if (projectRepo.CustomerProjectExists(evt.ProjectUID.ToString()).Result)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Project already associated with a customer"));
        }
      }
      else if (evt is DissociateProjectCustomer)
      {
        throw new ServiceException(HttpStatusCode.NotImplemented,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Dissociating projects from customers is not supported"));
      }
      else if (evt is AssociateProjectGeofence)
      {
        var associateEvent = evt as AssociateProjectGeofence;
        if (associateEvent.GeofenceUID == Guid.Empty)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Missing GeofenceUID"));
        }
      }
    }
    
  }
}
