using System;
using System.Collections.Generic;
using System.Configuration;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using LandfillService.AcceptanceTests.Models;
using LandfillService.AcceptanceTests.Auth;
using LandfillService.AcceptanceTests.LandFillKafka;

namespace LandfillService.AcceptanceTests
{
    public class Config
    {
        #region Kafka
        public static string KafkaEndpoint = ConfigurationManager.AppSettings["KafkaEndpoint"];
        public static IKafkaDriver KafkaDriver
        {
            get
            {
                if(ConfigurationManager.AppSettings["KafkaDriver"] == "JAVA")
                {
                    return new KafkaResolver();
                }
                else
                {
                    return new KafkaDotNet();
                }
            }
        }

        public static string CustomerTopic = ConfigurationManager.AppSettings["CustomerTopic"];
        public static string ProjectTopic = ConfigurationManager.AppSettings["ProjectTopic"];
        public static string SubscriptionTopic = ConfigurationManager.AppSettings["SubscriptionTopic"];
        public static string GeofenceTopic = ConfigurationManager.AppSettings["GeofenceTopic"];
        #endregion

        #region Database
        public static string MySqlConnString = ConfigurationManager.ConnectionStrings["LandfillContext"].ConnectionString;
        public static string MySqlDbName = "`" + MySqlConnString.Split(';')[1].Split('=')[1] + "`";

        public static Guid LandfillUserUID = Guid.Parse("b80b542b-556f-4df7-9383-a630a7615536"); // "clay_anderson@trimble.com"
        public static Guid LandfillCustomerUID = Guid.Parse("b80b542b-556f-4df7-9383-a630a7615536"); // "Test customer"

        public static Guid MasterDataUserUID = Guid.Parse("2fa7e8f2-670e-4fa3-964b-7549c9cb196d"); // "acceptance_test@vss.com"
        public static Guid MasterDataCustomerUID = Guid.Parse("465a6189-9be3-48fc-a30b-1a525bd376b1"); // "AcceptTestGoldenCustomer" 
        #endregion

        #region Auth
        public static string JwtToken;
        #endregion

        #region Web API
        public static string LandfillBaseUri = ConfigurationManager.AppSettings["LandFillWebApiBaseUrl"];

        /// <summary>
        /// Returns the list of projects available to the user
        /// </summary>
        public static string ConstructGetProjectListUri()
        {
            return LandfillBaseUri + "/api/v2/projects";
        }
        /// <summary>
        /// Returns the project data for the given project. If geofenceUid is not specified, data for the entire project area 
        /// is returned otherwise data for the geofenced area is returned. If no date range specified, returns data for the last 
        /// 2 years to today in the project time zone otherwise returns data for the specified date range.
        /// </summary>
        public static string ConstructGetProjectDataUri(uint projectId, Guid geofenceUid = default(Guid), 
            DateTime startDate = default(DateTime), DateTime endDate = default(DateTime))
        {
            string geofenceUidStr = geofenceUid != default(Guid) ? geofenceUid.ToString() : "";
            string startDateStr = startDate != default(DateTime) ? startDate.ToString("yyyy-MM-dd") : "";
            string endDateStr = endDate != default(DateTime) ? endDate.ToString("yyyy-MM-dd") : "";

            return string.Format("{0}/api/v2/projects/{1}?geofenceUid={2}&startDate={3}&endDate={4}",
                LandfillBaseUri, projectId, geofenceUidStr, startDateStr, endDateStr);
        }
        /// <summary>
        /// Returns the weights for all geofences for the project for the date range of the last 2 years to today in the project time zone.
        /// </summary>
        public static string ConstructGetWeightsUri(uint projectId)
        {
            return string.Format("{0}/api/v2/projects/{1}/weights", LandfillBaseUri, projectId);
        }
        /// <summary>
        /// Saves weights submitted in the request.
        /// </summary>
        public static string ConstructSubmitWeightsUri(uint projectId, Guid geofenceUid)
        {
            return string.Format("{0}/api/v2/projects/{1}/weights?geofenceUid={2}", LandfillBaseUri, projectId, geofenceUid);
        }
        /// <summary>
        /// Gets volume and time summary for a landfill project.
        /// </summary>
        public static string ConstructGetVolumesUri(uint projectId)
        {
            return string.Format("{0}/api/v2/projects/{1}/volumeTime", LandfillBaseUri, projectId);
        }
        /// <summary>
        /// Returns a list of geofences for the project. A geofence is associated with a project if its boundary is 
        /// inside or intersects that of the project and it is of type 'Landfill'. The project geofence is also returned.
        /// </summary>
        public static string ConstructGetGeofencesUri(uint projectId)
        {
            return string.Format("{0}/api/v2/projects/{1}/geofences", LandfillBaseUri, projectId);
        }
        /// <summary>
        /// Returns a geofence boundary.
        /// </summary>
        public static string ConstructGetGeofencesBoundaryUri(uint projectId, Guid geofenceUid)
        {
            return string.Format("{0}/api/v2/projects/{1}/geofences/{2}", LandfillBaseUri, projectId, geofenceUid);
        }
        /// <summary>
        /// Gets CCA summary for a landfill project.
        /// </summary>
        public static string ConstructGetCcaSummaryUri(uint projectId, DateTime startDate, DateTime endDate)
        {
            return string.Format("{0}/api/v2/projects/{1}/cca?startDate={2}&endDate={3}",
                LandfillBaseUri, projectId, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
        } 
        #endregion
    }
}