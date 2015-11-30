using System;
using System.Configuration;
using System.Globalization;
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
            get 
            {
                if (instance == null)
                {
                    instance = new MessageFactory();
                }
                return instance;
            }
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
            
            switch (eventType)
            {
                #region CreateAssetEvent
                case MessageType.CreateProjectEvent:
                    var createProjectEvent = new CreateProjectEvent
                    {
                        ActionUTC = DateTime.UtcNow,
                        ProjectBoundaries = eventRow["Boundaries"],
                        ProjectEndDate = DateTime.Today.AddDays(Convert.ToInt32(eventRow["DaysToExpire"])),
                        ProjectStartDate = DateTime.Today.AddMonths(-3),
                        ProjectName = eventRow["ProjectName"] + uniqueNumber,
                        ProjectTimezone = eventRow["TimeZone"],
                        ProjectType = eventRow["Type"] == "LandFill" ? ProjectType.LandFill : ProjectType.Full3D,
                    //    ProjectID = Convert.ToInt32(uniqueNumber),
                        ProjectUID = new Guid(),
                        ReceivedUTC = DateTime.UtcNow
                    };
                    
                    messageStr = JsonConvert.SerializeObject(createProjectEvent,new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
                    topic = ConfigurationManager.AppSettings["CreateProjectTopic"];
                    break; 
                #endregion
            }
            return new KafkaMessage(Kafka.Instance.GetProducer(), topic, new Message(messageStr));
        }
    }
}
