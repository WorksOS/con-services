using System;
using System.Reflection;
using System.Web.Http;
using log4net;
using VSP.MasterData.Common.KafkaWrapper.Interfaces;
using VSP.MasterData.Project.WebAPI.Helpers;
using VSP.MasterData.Common.Logging;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSP.MasterData.Project.WebAPI.Controllers.V1
{
  [RoutePrefix("v1")]
  public class ProjectV1Controller : ApiController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IProducerWrapper _producer;

    public ProjectV1Controller(IProducerWrapper producer)
    {
      _producer = producer;
    }

    // POST: api/project
    /// <summary>
    /// Create Project
    /// </summary>
    /// <param name="project">CreateProjectEvent model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("")]
    [HttpPost]
    public IHttpActionResult CreateProject([FromBody] CreateProjectEvent project)
    {
      try
      {
        project.ReceivedUTC = DateTime.UtcNow;
        var jsonHelper = new JsonHelper();
        var message = jsonHelper.SerializeObjectToJson(new { CreateProjectEvent = project });
        _producer.Publish(message, project.ProjectUID.ToString());
        Log.Debug(String.Format("Create Project Event: {0}",message));
        return Ok();
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError();
      }
    }

    // PUT: api/Project
    /// <summary>
    /// Update Project
    /// </summary>
    /// <param name="project">UpdateProjectEvent model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("")]
    [HttpPut]
    public IHttpActionResult UpdateProject([FromBody] UpdateProjectEvent project)
    {
      try
      {
        var jsonHelper = new JsonHelper();
        project.ReceivedUTC = DateTime.UtcNow;
        var message = jsonHelper.SerializeObjectToJson(new { UpdateProjectEvent = project });
        _producer.Publish(message, project.ProjectUID.ToString());
        Log.Debug(String.Format("Update Project Event: {0}", message));

        return Ok();
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError();
      }
    }

    // DELETE: api/Project/
    /// <summary>
    /// Delete Project
    /// </summary>
    /// <param name="projectUID">DeleteProjectEvent model</param>
    /// <param name="userUID">DeleteProjectEvent model</param>
    /// <param name="actionUTC">DeleteProjectEvent model</param>
    /// <remarks>Deletes project with projectUID</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>

    [Route("")]
    [HttpDelete]
    public IHttpActionResult DeleteProject(Guid projectUID, DateTime actionUTC)
    {
      try
      {
        var project = new DeleteProjectEvent();
        project.ProjectUID = projectUID;
        project.ActionUTC = actionUTC;

        project.ReceivedUTC = DateTime.UtcNow;
        var jsonHelper = new JsonHelper();

        var message = jsonHelper.SerializeObjectToJson(new { DeleteProjectEvent = project });
        _producer.Publish(message, project.ProjectUID.ToString());
        Log.Debug(String.Format("Delete Project Event: {0}", message));

        return Ok();
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError();
      }
    }


    // POST: api/project
    /// <summary>
    /// Restore Project
    /// </summary>
    /// <param name="project">CreateProjectEvent model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("Restore")]
    [HttpPost]
    public IHttpActionResult RestoreProject([FromBody] RestoreProjectEvent project)
    {
      try
      {
        project.ReceivedUTC = DateTime.UtcNow;
        var jsonHelper = new JsonHelper();
        var message = jsonHelper.SerializeObjectToJson(new { RestoreProjectEvent = project });
        _producer.Publish(message, project.ProjectUID.ToString());
        Log.Debug(String.Format("Restore Project Event: {0}", message));

        return Ok();
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError();
      }
    }

    /// <summary>
    /// Associate customer and project
    /// </summary>
    /// <param name="customerProject">Customer - project</param>
    /// <param name="topic">(Optional)Topic to publish on. Used for test purposes.</param>
    /// <remarks>Associate customer and asset</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [Route("AssociateCustomer")]
    public IHttpActionResult AssociateCustomerProject([FromBody] AssociateProjectCustomer customerProject)
    {
      try
      {
        customerProject.ReceivedUTC = DateTime.UtcNow;
        var message = new JsonHelper().SerializeObjectToJson(new { AssociateCustomerAssetEvent = customerProject });
        _producer.Publish(message, customerProject.CustomerUID.ToString());
        Log.Debug(String.Format("AssociateProjectCustomer Project Event: {0}", message));

        return Ok();
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError(ex);
      }
    }

    /// <summary>
    /// Dissociate customer and asset
    /// </summary>
    /// <param name="customerProject">Customer - Project model</param>
    /// <param name="topic">(Optional)Topic to publish on. Used for test purposes.</param>
    /// <remarks>Dissociate customer and asset</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [Route("DissociateCustomer")]
    public IHttpActionResult DissociateCustomerProject([FromBody] DissociateProjectCustomer customerProject)
    {
      try
      {
        customerProject.ReceivedUTC = DateTime.UtcNow;
        var message = new JsonHelper().SerializeObjectToJson(new { DissociateCustomerAssetEvent = customerProject });
        _producer.Publish(message, customerProject.CustomerUID.ToString());
        Log.Debug(String.Format("DissociateProjectCustomer Project Event: {0}", message));
        return Ok();
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError(ex);
      }
    }

  }
}
