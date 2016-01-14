using System;
using System.Configuration;
using KafkaNet.Protocol;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using LandfillService.AcceptanceTests.Models;
using LandfillService.AcceptanceTests.Models.KafkaTopics;

namespace LandfillService.AcceptanceTests.Helpers
{
    /// <summary>
    /// MessageFactory is Singleton (just in case it needs be derived)
    /// </summary>
    public class MessageFactory
    {
       // private static readonly ILog log = LogManager.GetLogger(typeof(MessageFactory));

        private static MessageFactory instance;
        private MessageFactory() { }
        public static MessageFactory Instance
        {
            get { return instance ?? (instance = new MessageFactory()); }
        }

        /// <summary>
        /// Generates IMessage that can be Send() to a Kafka queue. Data source comes from SpecFlow feature file Table.
        /// </summary>
        /// <param name="eventRow"></param>
        /// <param name="uniqueNumber"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public IMessage CreateMessage(TableRow eventRow, string uniqueNumber, MessageType eventType)
        {
            string messageStr = "";
            string topic = "";
            Guid guid = Guid.NewGuid();

            switch (eventType)
            {
                #region CreateProjectEvent
                case MessageType.CreateProjectEvent:
                    var createProjectEvent = new ProjectEvent
                    {
                        ActionUTC = DateTime.UtcNow,
                        ProjectBoundaries = eventRow.Keys.Contains("Boundaries") ? eventRow["Boundaries"] : " ",
                        ProjectEndDate = DateTime.Today.AddDays(Convert.ToInt32(eventRow["DaysToExpire"])),
                        ProjectStartDate = DateTime.Today.AddMonths(-3),
                        ProjectName = eventRow["ProjectName"] + uniqueNumber,
                        ProjectTimezone = eventRow.Keys.Contains("TimeZone") ? eventRow["TimeZone"] : " ",
                        ProjectType = eventRow["Type"] == "LandFill" ? ProjectType.LandFill : ProjectType.Full3D,
                        ProjectID = LandFillMySqlDb.GetTheHighestProjectId() + 1,
                        ProjectUID = guid,
                        ReceivedUTC = DateTime.UtcNow
                    };

                    messageStr = JsonConvert.SerializeObject(new { CreateProjectEvent = createProjectEvent }, 
                                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });                   
                    topic = ConfigurationManager.AppSettings["CreateProjectTopic"];
                    break; 
                #endregion

                #region UpdateProjectEvent
                case MessageType.UpdateProjectEvent:
                    var updateProjectEvent = new ProjectEvent
                    {
                        ActionUTC = DateTime.UtcNow,
                        ProjectBoundaries = eventRow.Keys.Contains("Boundaries") ? eventRow["Boundaries"] : " ",
                        ProjectEndDate = DateTime.Today.AddDays(Convert.ToInt32(eventRow["DaysToExpire"])),
                        ProjectStartDate = DateTime.Today.AddMonths(-3),
                        ProjectName = eventRow["ProjectName"] + uniqueNumber,
                        ProjectTimezone = eventRow.Keys.Contains("TimeZone") ? eventRow["TimeZone"] : " ",
                        ProjectType = eventRow["Type"] == "LandFill" ? ProjectType.LandFill : ProjectType.Full3D,
                        ProjectID = LandFillMySqlDb.GetTheHighestProjectId() + 1,
                        ProjectUID = guid,
                        ReceivedUTC = DateTime.UtcNow
                    };

                    messageStr = JsonConvert.SerializeObject(new { UpdateProjectEvent = updateProjectEvent },
                                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

                    topic = ConfigurationManager.AppSettings["UpdateProjectTopic"];
                    break;
                #endregion

                #region DeleteProjectEvent
                case MessageType.DeleteProjectEvent:
                    var deleteProjectEvent = new ProjectEvent
                    {
                        ActionUTC = DateTime.UtcNow,
                        ProjectBoundaries = eventRow.Keys.Contains("Boundaries") ? eventRow["Boundaries"] : " ",
                        ProjectEndDate = DateTime.Today.AddDays(Convert.ToInt32(eventRow["DaysToExpire"])),
                        ProjectStartDate = DateTime.Today.AddMonths(-3),
                        ProjectName = eventRow["ProjectName"] + uniqueNumber,
                        ProjectTimezone = eventRow.Keys.Contains("TimeZone") ? eventRow["TimeZone"] : " ",
                        ProjectType = eventRow["Type"] == "LandFill" ? ProjectType.LandFill : ProjectType.Full3D,
                        ProjectID = LandFillMySqlDb.GetTheHighestProjectId() + 1,
                        ProjectUID = guid,
                        ReceivedUTC = DateTime.UtcNow
                    };

                    messageStr = JsonConvert.SerializeObject(new { DeleteProjectEvent = deleteProjectEvent },
                                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

                    topic = ConfigurationManager.AppSettings["DeleteProjectTopic"];
                    break;
                #endregion
            }
            return new KafkaMessage(Kafka.Instance.GetProducer(), topic, new Message(messageStr));
        }
    }
}
