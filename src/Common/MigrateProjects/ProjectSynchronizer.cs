using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;
using VSS.Hosted.VLCommon;
using MigrateProjects.Properties;
using Newtonsoft.Json;
using RdKafka;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType;

namespace ThreeDAPIs.ProjectMasterData
{
  public class ProjectSynchronizer
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    private Producer producer;
    public List<Task> tasks = new List<Task>();

    public ProjectSynchronizer()
    {
      InitKafka();
    }

    private void InitKafka()
    {
      Config config = new Config();
      TopicConfig topicConfig = new TopicConfig();
      config.DefaultTopicConfig=topicConfig;
      producer = new Producer(config, Settings.Default.KAFKA_URI);
    }


    private async Task Send(string topic, KeyValuePair<string, string> messageToSendWithKey)
    {
      using (Topic myTopic = producer.Topic(topic))
      {
        byte[] data = Encoding.UTF8.GetBytes(messageToSendWithKey.Value);
        byte[] key = Encoding.UTF8.GetBytes(messageToSendWithKey.Key);
        tasks.Add(myTopic.Produce(data, key));
      }
    }



    public void SyncCreateProject(int id, Guid uid, int startKeyDate, int endKeyDate, string name, string timezone, 
      ProjectTypeEnum projectType, string boundary, Guid customerUid, long customerId, string coordfilename, DateTime actionUTC)
    {
      //string boundary = points.Aggregate(string.Empty, (current, point) => string.Format("{0}{1},{2};", current, point.Latitude, point.Longitude));
      //Remove trailing ;
      //boundary = boundary.Substring(0, boundary.Length - 1);
      CreateProjectEvent evt = new CreateProjectEvent
                          {
                            ProjectEndDate = endKeyDate.FromKeyDate(),
                            ProjectStartDate = startKeyDate.FromKeyDate(),
                            ProjectTimezone = timezone,
                            ProjectName = name,
                            Description = "",
                            ProjectType = (ProjectType) projectType,
                            ProjectBoundary = boundary,
                            ProjectUID = uid,
                            CustomerUID = customerUid,
                            ProjectID = id,
                            CustomerID = customerId,
                            CoordinateSystemFileName = coordfilename,
                            CoordinateSystemFileContent = null,
                            ActionUTC = actionUTC,
                            ReceivedUTC = actionUTC
                          };

      var messagePayload = JsonConvert.SerializeObject(new { CreateProjectEvent = evt });
      Send("VSS.Interfaces.Events.MasterData.IProjectEvent.V2",
        new KeyValuePair<string, string>(evt.ProjectUID.ToString(), messagePayload)).Wait();
    }

    public void SyncAssignProjectToCustomer(Guid projectUid, Guid customerUid, long customerID, DateTime actionUTC)
    {
      AssociateProjectCustomer evt = new AssociateProjectCustomer
                                {
                                    ProjectUID = projectUid,
                                    CustomerUID = customerUid,
                                    LegacyCustomerID = customerID,
                                    RelationType = (int)RelationType.Owner,
                                    ActionUTC = actionUTC,
                                    ReceivedUTC = actionUTC
                                };
      var messagePayload = JsonConvert.SerializeObject(new { AssociateProjectCustomer = evt });
      Send("VSS.Interfaces.Events.MasterData.IProjectEvent.V2",
        new KeyValuePair<string, string>(evt.ProjectUID.ToString(), messagePayload)).Wait();
    }


    public void SyncDeleteProject(Guid projectUid, DateTime actionUTC)
    {
      //aka Archive Project
      DeleteProjectEvent evt = new DeleteProjectEvent
                               {
                                   ProjectUID = projectUid,
                                   ActionUTC = actionUTC,
                                   ReceivedUTC = actionUTC
                               };
      var messagePayload = JsonConvert.SerializeObject(new { DeleteProjectEvent = evt });
      Send("VSS.Interfaces.Events.MasterData.IProjectEvent.V2",
        new KeyValuePair<string, string>(evt.ProjectUID.ToString(), messagePayload)).Wait();
    }


    public void SyncAssignSiteToProject(Guid projectUid, Guid siteUid, DateTime actionUTC)
    {
      AssociateProjectGeofence evt = new AssociateProjectGeofence
      {
        ProjectUID = projectUid,
        GeofenceUID = siteUid,
        ActionUTC = actionUTC,
        ReceivedUTC = actionUTC
      };
      var messagePayload = JsonConvert.SerializeObject(new { AssociateProjectGeofence = evt });
      Send("VSS.Interfaces.Events.MasterData.IProjectEvent.V2",
        new KeyValuePair<string, string>(evt.ProjectUID.ToString(), messagePayload)).Wait();
    }

  }
}
