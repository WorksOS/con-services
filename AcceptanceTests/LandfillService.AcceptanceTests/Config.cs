using System;
using System.Collections.Generic;
using System.Configuration;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using LandfillService.AcceptanceTests.Models;

namespace LandfillService.AcceptanceTests
{
    public class Config
    {
        public static string MySqlConnString = ConfigurationManager.ConnectionStrings["LandfillContext"].ConnectionString;
        public static string MySqlDbName = ConfigurationManager.AppSettings["MySqlDBName"];

        public static string LandfillBaseUri = ConfigurationManager.AppSettings["LandFillWebApiBaseUrl"];
        public static string PMBaseUri = ConfigurationManager.AppSettings["ProjectMonitoringWebApiBaseUrl"];

        public static string KafkaDriver = ConfigurationManager.AppSettings["KafkaDriver"];
        public static string KafkaEndpoint =  ConfigurationManager.AppSettings["KafkaEndpoint"];

        public static string CustomerMasterDataTopic = ConfigurationManager.AppSettings["CustomerMasterDataTopic"];
        public static string ProjectMasterDataTopic = ConfigurationManager.AppSettings["ProjectMasterDataTopic"];
        public static string SubscriptionTopic = ConfigurationManager.AppSettings["SubscriptionTopic"];
        public static string CustomerUserMasterDataTopic = ConfigurationManager.AppSettings["CustomerUserMasterDataTopic"];

        public static string GoldenUserName = "acceptance_test@vss.com";
        public static string GoldenUserPassword = "Password@123";
        public static Guid GoldenUserUID = Guid.Parse("2fa7e8f2-670e-4fa3-964b-7549c9cb196d");
        public static Guid GoldenCustomerUID = Guid.Parse("465a6189-9be3-48fc-a30b-1a525bd376b1");
    }
}