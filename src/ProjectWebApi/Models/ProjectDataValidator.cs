using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KafkaConsumer;
using VSS.Project.Data;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ProjectWebApi.Models
{
  public class ProjectDataValidator
  {
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
        //Nothing else to check for UpdateProjectEvent and DeleteProjectEvent
      }
      else if (evt is AssociateProjectCustomer)
      {
        if (projectRepo.CustomerProjectExists(evt.ProjectUID.ToString()).Result)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              "Project already associated with a customer");
        }
      }
      else if (evt is DissociateProjectCustomer)
      {
        if (!projectRepo.CustomerProjectExists(evt.ProjectUID.ToString()).Result)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              "Project not associated with a customer");
        }
      }
      //Nothing else to check for AssociateGeofence
    }
  }
}
