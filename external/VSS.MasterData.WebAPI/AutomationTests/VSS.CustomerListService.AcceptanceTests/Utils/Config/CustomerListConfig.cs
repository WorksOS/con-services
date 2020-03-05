using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.CustomerListService.AcceptanceTests.Utils.Config
{
    public class CustomerListConfig
    {
        public static string VSSTestEnv;

        //MySQL Settings
        public static string MySqlDBServer;
        public static string MySqlDBUsername;
        public static string MySqlDBPassword;
        public static string MySqlDBName;

        //VSS Test Environment Endpoints
        public static string MySqlConnection;
        public static string KafkaTimeoutThreshold;
        public static string CustomerListWebAPI;
        public static string BaseWebAPIUri;
        public static string WebAPIUri;
        public static string WebAPIVersion;
        public static string WebAPICustomer;
        public static string WebAPICustomerList;
        public static string WebAPIConsumerKey;
        public static string WebAPIConsumerSecret;
        
        public static void SetupEnvironment()
        {
            string Protocol = "https://";
            VSSTestEnv = ConfigurationManager.AppSettings["VSSTestEnv"];
            KafkaTimeoutThreshold = ConfigurationManager.AppSettings["KafkaTimeoutThreshold"];
            
            switch (VSSTestEnv)
            {
                case "DEV":
                    MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBServer"];
                    MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBUsername"];
                    MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBPassword"];
                    MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBName"];

                    BaseWebAPIUri = System.Configuration.ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
                    WebAPIUri = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIUri"];
                    WebAPIVersion = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIVersion"];
                    WebAPICustomer = System.Configuration.ConfigurationManager.AppSettings["DevWebAPICustomer"];
                    WebAPICustomerList = System.Configuration.ConfigurationManager.AppSettings["DevWebAPICustomerList"];

                    WebAPIConsumerKey = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
                    WebAPIConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];
                    
                    break;

                case "LOCAL":
                    MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBServer"];
                    MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBUsername"];
                    MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBPassword"];
                    MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBName"];

                    BaseWebAPIUri = System.Configuration.ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
                    WebAPIUri = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIUri"];
                    WebAPIVersion = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIVersion"];
                    WebAPICustomer = System.Configuration.ConfigurationManager.AppSettings["DevWebAPICustomer"];
                    WebAPICustomerList = System.Configuration.ConfigurationManager.AppSettings["DevWebAPICustomerList"];
                    
                    WebAPIConsumerKey = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
                    WebAPIConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];
                    
                    break;

                default: //Default is Dev Environment
                    MySqlDBServer = System.Configuration.ConfigurationManager.AppSettings["MySqlDBServer"];
                    MySqlDBUsername = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBUsername"];
                    MySqlDBPassword = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBPassword"];
                    MySqlDBName = System.Configuration.ConfigurationManager.AppSettings["DevMySqlDBName"];

                    BaseWebAPIUri = System.Configuration.ConfigurationManager.AppSettings["DevBaseWebAPIUri"];
                    WebAPIUri = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIUri"];
                    WebAPIVersion = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIVersion"];
                    WebAPICustomer = System.Configuration.ConfigurationManager.AppSettings["DevWebAPICustomer"];
                    WebAPICustomerList = System.Configuration.ConfigurationManager.AppSettings["DevWebAPICustomerList"];
                    
                    WebAPIConsumerKey = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIConsumerKey"];
                    WebAPIConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["DevWebAPIConsumerSecret"];
                    
                    break;
            }

            MySqlConnection = "server=" + MySqlDBServer + ";user id=" + MySqlDBUsername + ";password=" + MySqlDBPassword + ";database=";

            CustomerListWebAPI = Protocol + BaseWebAPIUri + "/t/trimble.com/" + WebAPIUri + "/" 
                + WebAPIVersion + "/" + WebAPICustomer + "/" + WebAPICustomerList;
        }
    }
}
