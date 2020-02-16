using log4net;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM
{
  public class GroupService : ServiceBase, IGroupService
  {
    private readonly ILog _log;
    private readonly HttpClient _httpClient;
    private static readonly string GroupApiBaseUri = ConfigurationManager.AppSettings["GroupService.WebAPIURI"];


    public GroupService()
    {
      _log = base.Logger;
      _httpClient = new HttpClient
      {
        //Timeout = new TimeSpan(0, 0, AppConfigSettings.TimeOutValue)
      };
    }


    public bool CreateGroup(object createGroupDetails)
    {
      try
      {
          var groupDetails = JsonConvert.SerializeObject(createGroupDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
          var status = DispatchRequest(GroupApiBaseUri, HttpMethod.Post, groupDetails);
          _log.IfDebugFormat("Create Group Event Payload Data : {0} Posted Status : {1}", groupDetails, status);
          return status;
      }
      catch (Exception ex)
      {
        _log.IfWarnFormat("Error occurred while creating Group in VSP stack. Error message :{0}",
              ex.Message);
        return false;
      }
    }

    public bool UpdateGroup(object groupDetails)
    {
      try
      {
        var updateEvent = JsonConvert.SerializeObject(groupDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
        var status = DispatchRequest(GroupApiBaseUri, HttpMethod.Put, updateEvent);
        _log.IfDebugFormat("Update Group Event Payload Data : {0} Posted Status : {1}", updateEvent, status);
        return status;
      }
      catch (Exception ex)
      {
        _log.IfWarnFormat("Error occurred while updating Group in VSP stack. Error message :{0}",
            ex.Message);
        return false;
      }
    }

    public bool DeleteGroup(DeleteGroupEvent deleteGroupEvent, string url)
    {
      try
      {
        var groupDetails = JsonConvert.SerializeObject(deleteGroupEvent, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        string groupUrl = string.Concat(GroupApiBaseUri, url);
        var status = DispatchRequest(groupUrl, HttpMethod.Delete, groupDetails);
        _log.IfDebugFormat("Delete Group Event Payload Data : {0} Posted Status : {1}", groupDetails, status);
        return status;
      }
      catch (Exception ex)
      {
        _log.IfWarnFormat("Error occurred while deleting Group in VSP stack. Error message :{0}",
    ex.Message);
        return false;
      }
    }
  }
}
