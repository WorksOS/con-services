using System.Collections.Generic;
using System.Configuration;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using LandfillService.AcceptanceTests.Models;

namespace LandfillService.AcceptanceTests
{
    public class Config
    {
        public static string LandfillBaseUri = ConfigurationManager.AppSettings["LandFillWebApiBaseUrl"];
        public static string PMBaseUri = ConfigurationManager.AppSettings["ProjectMonitoringWebApiBaseUrl"];

        public static string KafkaDriver = ConfigurationManager.AppSettings["KafkaDriver"];
        public static string KafkaEndpoint =  ConfigurationManager.AppSettings["KafkaEndpoint"];

        public static string CustomerMasterDataTopic = ConfigurationManager.AppSettings["CustomerMasterDataTopic"];
        public static string ProjectMasterDataTopic = ConfigurationManager.AppSettings["ProjectMasterDataTopic"];
        public static string SubscriptionTopic = ConfigurationManager.AppSettings["SubscriptionTopic"];
        public static string CustomerUserMasterDataTopic = ConfigurationManager.AppSettings["CustomerUserMasterDataTopic"];
    }
}