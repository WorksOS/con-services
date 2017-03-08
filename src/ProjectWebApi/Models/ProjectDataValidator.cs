using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ProjectWebApi.Models
{
  /// <summary>
  /// Validates all project event data sent to the Web API
  /// </summary>
  public class ProjectDataValidator
  {
    /// <summary>
    /// Validates the data of a specific project event
    /// </summary>
    /// <param name="evt">The event containing the data to be validated</param>
    /// <param name="repo">Project repository to use in validation</param>
    public static void Validate(IProjectEvent evt, IRepository<IProjectEvent> repo)
    {
      var projectRepo = repo as ProjectRepository;
      if (projectRepo == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          "Missing Project Repository in ProjectDataValidator.Validate");
      }
      if (evt.ActionUTC == DateTime.MinValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            "Missing ActionUTC");
      }
      if (evt.ProjectUID == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            "Missing ProjectUID");
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
          throw new ServiceException(HttpStatusCode.BadRequest, message);
        }
        if (isCreate)
        {
          var createEvent = evt as CreateProjectEvent;
          //Note: ProjectBoundary is NOT USED. Boundary is obtained from project geofence associated with project.
          if (string.IsNullOrEmpty(createEvent.ProjectBoundary))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              "Missing ProjectBoundary");
          }
          if (string.IsNullOrEmpty(createEvent.ProjectTimezone))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              "Missing ProjectTimezone");
          }
          if (string.IsNullOrEmpty(createEvent.ProjectName))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              "Missing ProjectName");
          }
          if (createEvent.ProjectStartDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              "Missing ProjectStartDate");
          }
          if (createEvent.ProjectEndDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              "Missing ProjectEndDate");
          }
          if (createEvent.ProjectEndDate < DateTime.UtcNow)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              "ProjectEndDate must be in the future");
          }
          if (createEvent.ProjectStartDate > createEvent.ProjectEndDate)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              "Start date must be earlier than end date");
          }
          if (createEvent.ProjectID <= 0)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              "Missing legacy ProjectID");
          }
        }
        else if (evt is UpdateProjectEvent)
        {
          var updateEvent = evt as UpdateProjectEvent;
          if (string.IsNullOrEmpty(updateEvent.ProjectName))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
                "ProjectName cannot be empty");
          }
          if (updateEvent.ProjectEndDate == DateTime.MinValue)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
                "ProjectEndDate cannot be empty");
          }
          var project = projectRepo.GetProjectOnly(evt.ProjectUID.ToString()).Result;
          if (project.StartDate > updateEvent.ProjectEndDate)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
                "ProjectEndDate must be later than start date");
          }
          if (!project.ProjectTimeZone.Equals(updateEvent.ProjectTimezone))
          {
            throw new ServiceException(HttpStatusCode.Forbidden,
              "Project timezone cannot be updated");
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
              "Missing CustomerUID");
        }
        if (associateEvent.LegacyCustomerID <= 0)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
             "Missing legacy CustomerID");
        }
        if (projectRepo.CustomerProjectExists(evt.ProjectUID.ToString()).Result)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              "Project already associated with a customer");
        }
      }
      else if (evt is DissociateProjectCustomer)
      {
        throw new ServiceException(HttpStatusCode.NotImplemented,
          "Dissociating projects from customers is not supported");
      }
      else if (evt is AssociateProjectGeofence)
      {
        var associateEvent = evt as AssociateProjectGeofence;
        if (associateEvent.GeofenceUID == Guid.Empty)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              "Missing GeofenceUID");
        }
      }
    }
  }
}
