using System;
using System.Collections.Generic;
using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerService;
using AutomationCore.API.Framework.Library;
using VSS.CustomerListService.AcceptanceTests.Utils.Config;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerService;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerUserService;
using AutomationCore.API.Framework.Common.Config.TPaaSServicesConfig;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Common;
using System.Net;
using Newtonsoft.Json;
using VSS.CustomerListService.AcceptanceTests.Utils.Features.Classes.CustomerListService;

namespace VSS.CustomerListService.AcceptanceTests.Scenarios.CustomerListService
{
    [Binding]
    public class CustomerListServiceSteps
    {
        //DB Configuration
        public string MySqlConnectionString;
        public string MySqlDBName;

        //Declarations
        public string customerUID = string.Empty;
        public string userUID = string.Empty;
        public string customerName = string.Empty;
        public string customerType = string.Empty;
        public DateTime actionUTC;
        public Guid deleteCustomerUID;
        public DateTime deleteActionUTC;
        public Guid[] multipleCustomerGUID = new Guid[3];
        public string[] multipleCustomerName = new string[3];

        public string MultipleCustomerUserUID = string.Empty;
        public string MultipleCustomerUserLoginId = "CustomerListMultipleUser" + InputGenerator.GenerateUniqueId() + "@Visionlink.com";
        public string MultipleCustomerUserPassword = "VisionLink$6";

        public string SingleCustomerUserUID = string.Empty;
        public string SingleCustomerUserLoginId = "CustomerListSingleUser" + InputGenerator.GenerateUniqueId() + "@Visionlink.com";
        public string SingleCustomerUserPassword = "VisionLink$6";

        public string ResponseString = string.Empty;

        //Initialize Logger
        private static Log4Net Logger = new Log4Net(typeof (CustomerListServiceSteps));
        private static CustomerServiceSupport customerServiceSupport = new CustomerServiceSupport(Logger);
        private static CustomerUserServiceSupport customerUserServiceSupport = new CustomerUserServiceSupport(Logger);

        public CustomerListServiceSteps()
        {
            CustomerListConfig.SetupEnvironment();
            MySqlDBName = CustomerListConfig.MySqlDBName;
            MySqlConnectionString = CustomerListConfig.MySqlConnection + MySqlDBName;
        }

        [Given(@"CustomerListConsumerService Is Ready To Verify '(.*)'")]
        public void GivenCustomerListConsumerServiceIsReadyToVerify(string testDescription)
        {
            LogResult.LogTestDescription(Logger, testDescription);
        }

        [Given(@"CustomerListConsumerService CreateCustomerEvent Request Is Setup With Default Values")]
        public void GivenCustomerListConsumerServiceCreateCustomerEventRequestIsSetupWithDefaultValues()
        {
            customerServiceSupport.CreateCustomerModel =
                CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();

            customerUID = customerServiceSupport.CreateCustomerModel.CustomerUID.ToString();
            customerName = customerServiceSupport.CreateCustomerModel.CustomerName;
            customerType = customerServiceSupport.CreateCustomerModel.CustomerType.ToString();
            actionUTC = customerServiceSupport.CreateCustomerModel.ActionUTC;
        }

        [When(@"I Post Valid CustomerListConsumerService CreateCustomerEvent Request")]
        public void WhenIPostValidCustomerListConsumerServiceCreateCustomerEventRequest()
        {
            customerServiceSupport.PostValidCreateRequestToService();
        }

        [When(@"Update The CustomerListConsumerService CreateCustomerEvent Request With Different Customer Name")]
        public void WhenUpdateTheCustomerListConsumerServiceCreateCustomerEventRequestWithDifferentCustomerName()
        {
            Guid oldCustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            string oldCustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;

            customerServiceSupport.CreateCustomerModel =
                CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
            customerServiceSupport.CreateCustomerModel.CustomerUID = oldCustomerUID;
            customerServiceSupport.CreateCustomerModel.CustomerType = oldCustomerType;
        }

        [When(@"Update The CustomerListConsumerService CreateCustomerEvent Request With Different Customer TypeId")]
        public void WhenUpdateTheCustomerListConsumerServiceCreateCustomerEventRequestWithDifferentCustomerTypeId()
        {
            Guid oldCustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            string oldCustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;

            customerServiceSupport.CreateCustomerModel =
                CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
            customerServiceSupport.CreateCustomerModel.CustomerUID = oldCustomerUID;
            customerServiceSupport.CreateCustomerModel.CustomerName = oldCustomerName;
            customerServiceSupport.CreateCustomerModel.CustomerType = "Customer";
        }

        [Then(@"The CreateCustomerEvent Details Are Stored In VSS DB")]
        [Then(@"The CreateCustomerEvent Details Are Updated In VSS DB")]
        public void ThenTheCreateCustomerEventDetailsAreStoredInVSSDB()
        {
            customerUID = customerServiceSupport.CreateCustomerModel.CustomerUID.ToString();
            customerName = customerServiceSupport.CreateCustomerModel.CustomerName;
            customerType = customerServiceSupport.CreateCustomerModel.CustomerType.ToString();

            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold);

            List<string> columnList = new List<string>() {"CustomerUID", "CustomerName", "CustomerType"};
            List<string> createCustomerDetails = new List<string>();
            createCustomerDetails.Add(customerUID);
            createCustomerDetails.Add(customerName);
            createCustomerDetails.Add(GetCustomerTypeId(customerType));

            string validateQuery = CustomerListSqlQueries.CustomerDetailsByCustomerUID + customerUID + "'";
            string validateDateQuery = CustomerListSqlQueries.CustomerUpdateUTCByCustomerUID + customerUID + "'";

            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, createCustomerDetails);
            MySqlUtil.ValidateMySQLDateValueQuery(MySqlConnectionString, validateDateQuery, DateTime.UtcNow.AddMinutes(-2), "LESS_THAN_DB");
        }

        [Then(@"The CreateCustomerEvent Details Are NOT Updated In VSS DB")]
        public void ThenTheCreateCustomerEventDetailsAreNOTUpdatedInVSSDB()
        {
            string validateQuery = CustomerListSqlQueries.CustomerTypeIdByCustomerUID + customerUID + "'";
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, "1"); //Not Updated As Customer
        }

        [When(@"CustomerListConsumerService UpdateCustomerEvent Request Is Setup With Default Values")]
        public void WhenCustomerListConsumerServiceUpdateCustomerEventRequestIsSetupWithDefaultValues()
        {
            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold); 

            Guid oldCustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            //string oldCustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;

            customerServiceSupport.UpdateCustomerModel =
                CustomerServiceSteps.GetDefaultValidCustomerServiceUpdateRequest();
            customerServiceSupport.UpdateCustomerModel.CustomerUID = oldCustomerUID;
            //customerServiceSupport.UpdateCustomerModel.CustomerName = oldCustomerName;
        }

        [When(@"I Post Valid CustomerListConsumerService UpdateCustomerEvent Request")]
        public void WhenIPostValidCustomerListConsumerServiceUpdateCustomerEventRequest()
        {
            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold);

            customerServiceSupport.PostValidUpdateRequestToService();
        }

        [Then(@"The UpdateCustomerEvent Details Are Updated In VSS DB")]
        public void ThenTheUpdateCustomerEventDetailsAreUpdatedInVSSDB()
        {
            customerUID = customerServiceSupport.UpdateCustomerModel.CustomerUID.ToString();
            customerName = customerServiceSupport.UpdateCustomerModel.CustomerName;
            
            string validateQuery = CustomerListSqlQueries.CustomerNameUpdateByCustomerUID + customerUID + "'";
            string validateDateQuery = CustomerListSqlQueries.CustomerUpdateUTCByCustomerUID + customerUID + "'";

            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, customerName.ToString());
            MySqlUtil.ValidateMySQLDateValueQuery(MySqlConnectionString, validateDateQuery, actionUTC, "LESS_THAN_DB");
        }

        [When(@"CustomerListConsumerService DeleteCustomerEvent Request Is Setup With Default Values")]
        public void WhenCustomerListConsumerServiceDeleteCustomerEventRequestIsSetupWithDefaultValues()
        {
            deleteCustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            deleteActionUTC = DateTime.UtcNow;

            customerServiceSupport.DeleteCustomerModel =
                CustomerServiceSteps.GetDefaultValidCustomerServiceDeleteRequest();
            customerServiceSupport.DeleteCustomerModel.CustomerUID = deleteCustomerUID;
            customerServiceSupport.DeleteCustomerModel.ActionUTC = deleteActionUTC;
        }

        [Given(@"CustomerListConsumerService AssociateCustomerUserEvent Request Is Setup With Default Values")]
        public void GivenCustomerListConsumerServiceAssociateCustomerUserEventRequestIsSetupWithDefaultValues()
        {
            customerServiceSupport.CreateCustomerModel =
                CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();

            customerServiceSupport.PostValidCreateRequestToService();
            
            multipleCustomerGUID[0] = customerServiceSupport.CreateCustomerModel.CustomerUID;

            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold); 

            customerUserServiceSupport.AssociateCustomerUserModel = CustomerUserServiceSteps.GetDefaultValidAssociateCustomerUserServiceRequest();
            customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID = multipleCustomerGUID[0];
            Guid userUID = new Guid(CreateValueSingleCustomerUserWithUID());
            customerUserServiceSupport.AssociateCustomerUserModel.UserUID = userUID;
        }
        
        [When(@"I Post Valid CustomerListConsumerService AssociateCustomerUserEvent Request")]
        public void WhenIPostValidCustomerListConsumerServiceAssociateCustomerUserEventRequest()
        {
            customerUserServiceSupport.PostValidCustomerUserAssociateRequestToService();
        }
        
        [When(@"I Post Valid CustomerListConsumerService AssociateCustomerUserEvent Request For Multiple Customers")]
        public void WhenIPostValidCustomerListConsumerServiceAssociateCustomerUserEventRequestForMultipleCustomers()
        {
            customerUserServiceSupport.AssociateCustomerUserModel = CustomerUserServiceSteps.GetDefaultValidAssociateCustomerUserServiceRequest();
            customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID = multipleCustomerGUID[0];
            Guid userUID = new Guid(CreateValidMultipeCustomerUserWithUID());
            customerUserServiceSupport.AssociateCustomerUserModel.UserUID = userUID;
            customerUserServiceSupport.PostValidCustomerUserAssociateRequestToService();
            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold); 

            customerUserServiceSupport.AssociateCustomerUserModel = CustomerUserServiceSteps.GetDefaultValidAssociateCustomerUserServiceRequest();
            customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID = multipleCustomerGUID[1];
            customerUserServiceSupport.AssociateCustomerUserModel.UserUID = userUID;
            customerUserServiceSupport.PostValidCustomerUserAssociateRequestToService();
            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold);

            customerUserServiceSupport.AssociateCustomerUserModel = CustomerUserServiceSteps.GetDefaultValidAssociateCustomerUserServiceRequest();
            customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID = multipleCustomerGUID[2];
            customerUserServiceSupport.AssociateCustomerUserModel.UserUID = userUID;
            customerUserServiceSupport.PostValidCustomerUserAssociateRequestToService();
            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold); 
        }
        
        [Given(@"CustomerListConsumerService DissociateCustomerUserEvent Request Is Setup With Default Values")]
        public void GivenCustomerListConsumerServiceDissociateCustomerUserEventRequestIsSetupWithDefaultValues()
        {
            customerServiceSupport.CreateCustomerModel =
                CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();

            customerServiceSupport.PostValidCreateRequestToService();

            Guid oldCustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;

            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold); 

            customerUserServiceSupport.AssociateCustomerUserModel = CustomerUserServiceSteps.GetDefaultValidAssociateCustomerUserServiceRequest();
            customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID = oldCustomerUID;

            customerUserServiceSupport.PostValidCustomerUserAssociateRequestToService();

            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold); 

            customerUserServiceSupport.DissociateCustomerUserModel = CustomerUserServiceSteps.GetDefaultValidDissociateCustomerUserServiceRequest();
            customerUserServiceSupport.DissociateCustomerUserModel.CustomerUID = oldCustomerUID;
            customerUserServiceSupport.DissociateCustomerUserModel.UserUID = customerUserServiceSupport.AssociateCustomerUserModel.UserUID;
        }

        [Given(@"CustomerListWebApi Is Ready To Verify '(.*)'")]
        public void GivenCustomerListWebApiIsReadyToVerify(string testDescription)
        {
            LogResult.LogTestDescription(Logger, testDescription);
        }

        [Given(@"User Created And Associated With Single Customer")]
        public void GivenUserCreatedAndAssociatedWithSingleCustomer()
        {
            if (SingleCustomerUserUID == string.Empty)
            {
                GivenCustomerListConsumerServiceAssociateCustomerUserEventRequestIsSetupWithDefaultValues();
                WhenIPostValidCustomerListConsumerServiceAssociateCustomerUserEventRequest();
            }
        }
        
        [Given(@"User Created And Associated With Multiple Customers")]
        public void GivenUserCreatedAndAssociatedWithMultipleCustomers()
        {
            if (MultipleCustomerUserUID == string.Empty)
            {
                GivenCustomerListConsumerServiceAssociateCustomerUserEventRequestIsSetupWithMultipleCustomerDefaultValues();
                WhenIPostValidCustomerListConsumerServiceAssociateCustomerUserEventRequestForMultipleCustomers();
            }
        }
        
        [When(@"I Post Valid CustomerListConsumerService DeleteCustomerEvent Request")]
        public void WhenIPostValidCustomerListConsumerServiceDeleteCustomerEventRequest()
        {
            customerServiceSupport.PostValidDeleteRequestToService(deleteCustomerUID, deleteActionUTC);
        }
        
        [When(@"I Set CustomerListConsumerService AssociateCustomerUserEvent CustomerUID Which DoesNotExist")]
        public void WhenISetCustomerListConsumerServiceAssociateCustomerUserEventCustomerUIDTo()
        {
            customerUserServiceSupport.AssociateCustomerUserModel = CustomerUserServiceSteps.GetDefaultValidAssociateCustomerUserServiceRequest();
        }

        [When(@"I Post Valid CustomerListConsumerService DissociateCustomerUserEvent Request")]
        public void WhenIPostValidCustomerListConsumerServiceDissociateCustomerUserEventRequest()
        {
            customerUserServiceSupport.PostValidCustomerUserDissociateRequestToService();
        }

        [When(@"I Set CustomerListConsumerService DissociateCustomerUserEvent CustomerUID Which DoesNotExist")]
        public void WhenISetCustomerListConsumerServiceDissociateCustomerUserEventCustomerUIDTo()
        {
            customerUID = customerUserServiceSupport.DissociateCustomerUserModel.CustomerUID.ToString();
            userUID = customerUserServiceSupport.DissociateCustomerUserModel.UserUID.ToString();

            customerUserServiceSupport.DissociateCustomerUserModel.CustomerUID = new Guid();
        }
        
        [When(@"I Set CustomerListConsumerService DissociateCustomerUserEvent UserUID Which DoesNotExist")]
        public void WhenISetCustomerListConsumerServiceDissociateCustomerUserEventUserUIDTo()
        {
            customerUID = customerUserServiceSupport.DissociateCustomerUserModel.CustomerUID.ToString();
            userUID = customerUserServiceSupport.DissociateCustomerUserModel.UserUID.ToString();

            customerUserServiceSupport.DissociateCustomerUserModel.UserUID = new Guid();
        }

        [When(@"I Post Valid CustomerListWebApi GetUserCustomerList Request")]
        public void WhenIPostValidCustomerListWebApiGetUserCustomerListRequest()
        {
            string accessToken = GetValidUserAccessToken(SingleCustomerUserLoginId);

            ResponseString = RestClientUtil.DoHttpRequest(CustomerListConfig.CustomerListWebAPI, HeaderSettings.GetMethod, accessToken,
                  HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
        }

        [When(@"I Post Valid CustomerListWebApi GetUserMultipleCustomerList Request")]
        public void WhenIPostValidCustomerListWebApiGetUserMultipleCustomerListRequest()
        {
            string accessToken = GetValidUserAccessToken(MultipleCustomerUserLoginId);

            ResponseString = RestClientUtil.DoHttpRequest(CustomerListConfig.CustomerListWebAPI, HeaderSettings.GetMethod, accessToken,
                  HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
        }
        
        [When(@"I Post InValid CustomerListWebApi GetUserCustomerList Request With '(.*)'")]
        public void WhenIPostInValidCustomerListWebApiGetUserCustomerListRequest(string invalidAccessToken)
        {
            string accessToken = InputGenerator.GetValue(invalidAccessToken);

            ResponseString = RestClientUtil.DoInvalidHttpRequest(CustomerListConfig.CustomerListWebAPI, HeaderSettings.GetMethod, accessToken,
                  HeaderSettings.JsonMediaType, null, HttpStatusCode.Unauthorized, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
        }

        [Then(@"The DeleteCustomerEvent Details Are Removed In VSS DB")]
        public void ThenTheDeleteCustomerEventDetailsAreRemovedInVSSDB()
        {
            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold);

            customerUID = customerServiceSupport.DeleteCustomerModel.CustomerUID.ToString();
            string validateQuery = CustomerListSqlQueries.CustomerDetailsUpdateByCustomerUID + customerUID + "'";

            MySqlUtil.ValidateMySQLQueryCount(MySqlConnectionString, validateQuery, 0); // There should be no matching rows.
        }

        [Then(@"The AssociateCustomerUserEvent Details Are Stored In VSS DB")]
        public void ThenTheAssociateCustomerUserEventDetailsAreStoredInVSSDB()
        {
            customerUID = customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID.ToString();
            userUID = customerUserServiceSupport.AssociateCustomerUserModel.UserUID.ToString();

            string validateQuery = CustomerListSqlQueries.CustomerUserDetailsByCustomerUID + userUID + "'";

            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, customerUID);
        }

        [Then(@"The AssociateCustomerUserEvent Details For All Customers Are Stored In VSS DB")]
        public void ThenTheAssociateCustomerUserEventDetailsForAllCustomersAreStoredInVSSDB()
        {
            userUID = customerUserServiceSupport.AssociateCustomerUserModel.UserUID.ToString();

            for (int index = 0; index <= 2; index++)
            {
                string validateQuery = CustomerListSqlQueries.CustomerUserDetailsByUserUID +
                                       multipleCustomerGUID[index].ToString() + "'";
                MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, userUID);
            }
        }

        [Then(@"The AssociateCustomerUserEvent Details Are NOT Stored In VSS DB")]
        public void ThenTheAssociateCustomerUserEventDetailsAreNOTStoredInVSSDB()
        {
            string userUID = customerUserServiceSupport.AssociateCustomerUserModel.UserUID.ToString();

            string validateQuery = CustomerListSqlQueries.CustomerUserDetailsByCustomerUID + userUID + "'";
            MySqlUtil.ValidateMySQLQueryCount(MySqlConnectionString, validateQuery, 0); // There should be no matching rows.
        }

        [Then(@"The DissociateCustomerUserEvent Details Are Removed In VSS DB")]
        public void ThenTheDissociateCustomerUserEventDetailsAreRemovedInVSSDB()
        {
            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold); 
            
            customerUID = customerUserServiceSupport.DissociateCustomerUserModel.CustomerUID.ToString();
            userUID = customerUserServiceSupport.DissociateCustomerUserModel.UserUID.ToString();
            string validateQuery = CustomerListSqlQueries.CustomerUserDetails + customerUID + "' AND fk_UserUID='" + userUID + "'";

            MySqlUtil.ValidateMySQLQueryCount(MySqlConnectionString, validateQuery, 0); // There should be no matching rows.
        }

        [Then(@"The DissociateCustomerUserEvent Details For Invalid Customer NOT Removed In VSS DB")]
        public void ThenTheDissociateCustomerUserEventDetailsAreNOTUpdatedInVSSDB()
        {
            string validateQuery = CustomerListSqlQueries.CustomerUserDetailsByCustomerUID + userUID + "'";
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, customerUID); // NOT Removed In VSS DB
        }

        [Then(@"The DissociateCustomerUserEvent Details Are Invalid User NOT Removed In VSS DB")]
        public void ThenTheDissociateCustomerUserEventDetailsAreInvalidUserNOTRemovedInVSSDB()
        {
            string validateQuery = CustomerListSqlQueries.CustomerUserDetailsByUserUID + customerUID + "'";
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, userUID); // NOT Removed In VSS DB
        }

        [Then(@"The GetUserCustomerList Response Should Return Customer Details")]
        public void ThenTheGetUserCustomerListResponseShouldReturnCustomerDetails()
        {
            CustomerListSuccessResponse responseObject = JsonConvert.DeserializeObject<CustomerListSuccessResponse>(ResponseString);

            Assert.AreEqual(responseObject.status, HttpStatusCode.OK);
            StringAssert.Contains(responseObject.metadata.msg, "Customers retrieved successfully");

            Assert.AreEqual(responseObject.customer.Count, 1); //Only One Customer Given In Response
            Assert.AreEqual(responseObject.customer[0].uid, customerServiceSupport.CreateCustomerModel.CustomerUID);
            Assert.AreEqual(responseObject.customer[0].name, customerServiceSupport.CreateCustomerModel.CustomerName);
            Assert.AreEqual(responseObject.customer[0].type, "Dealer");
        }

        [Then(@"The GetUserCustomerList Response Should Return Multiple Customers Details")]
        public void ThenTheGetUserCustomerListResponseShouldReturnMultipleCustomersDetails()
        {
            CustomerListSuccessResponse responseObject = JsonConvert.DeserializeObject<CustomerListSuccessResponse>(ResponseString);

            Assert.AreEqual(responseObject.status, HttpStatusCode.OK);
            StringAssert.Contains(responseObject.metadata.msg, "Customers retrieved successfully");

            Assert.AreEqual(responseObject.customer.Count, 3); //Three Customers Given In Response
            
            for (int responseIndex = 0; responseIndex < 3; responseIndex++)
            {
                bool matchingUID = false;
                for (int customerIndex = 0; customerIndex < 3; customerIndex++)
                {
                    if (responseObject.customer[responseIndex].uid == multipleCustomerGUID[customerIndex])
                    {
                        matchingUID = true;
                        Assert.AreEqual(responseObject.customer[responseIndex].name, multipleCustomerName[customerIndex]);
                        break;
                    }
                }
                Assert.IsTrue(matchingUID);
                Assert.AreEqual(responseObject.customer[responseIndex].type, "Dealer");
            }
        }
        
        [Then(@"The GetUserCustomerList Response Should Return '(.*)'")]
        public void ThenTheGetUserCustomerListResponseShouldReturn(string errorMessage)
        {
            if (errorMessage == "ERR_EmptyToken")
            {
                StringAssert.Contains(ResponseString, "Required OAuth credentials not provided");
            }
            else
            {
                StringAssert.Contains(ResponseString, "Access failure for API");
            }
        }

        [Given(@"CustomerListConsumerService AssociateCustomerUserEvent Request Is Setup With Multiple Customer Default Values")]
        public void GivenCustomerListConsumerServiceAssociateCustomerUserEventRequestIsSetupWithMultipleCustomerDefaultValues()
        {
            //Posting 1st Customer
            customerServiceSupport.CreateCustomerModel =
                CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();

            customerServiceSupport.PostValidCreateRequestToService();
            multipleCustomerName[0] = customerServiceSupport.CreateCustomerModel.CustomerName;
            multipleCustomerGUID[0] = customerServiceSupport.CreateCustomerModel.CustomerUID;

            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold); 

            //Posting 2nd Customer
            customerServiceSupport.CreateCustomerModel =
                CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();

            customerServiceSupport.PostValidCreateRequestToService();
            multipleCustomerName[1] = customerServiceSupport.CreateCustomerModel.CustomerName;
            multipleCustomerGUID[1] = customerServiceSupport.CreateCustomerModel.CustomerUID;

            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold); 
            
            //Posting 3rd Customer
            customerServiceSupport.CreateCustomerModel =
                CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();

            customerServiceSupport.PostValidCreateRequestToService();
            multipleCustomerName[2] = customerServiceSupport.CreateCustomerModel.CustomerName;
            multipleCustomerGUID[2] = customerServiceSupport.CreateCustomerModel.CustomerUID;

            CommonUtil.WaitToProcess(CustomerListConfig.KafkaTimeoutThreshold); 
        }

        public string GetCustomerTypeId(string customerTypeName)
        {
            CustomerType customerTypeId;
            CustomerType.TryParse(customerTypeName, out customerTypeId);

            return (((int)customerTypeId).ToString());
        }

        public string CreateValidMultipeCustomerUserWithUID()
        {
            if (MultipleCustomerUserUID == string.Empty)
            {
                TPaaSServicesConfig.SetupEnvironment();
                MultipleCustomerUserUID = UserIdentityService.CreateUser(MultipleCustomerUserLoginId, MultipleCustomerUserPassword);
            }

            return MultipleCustomerUserUID; 
        }

        public string CreateValueSingleCustomerUserWithUID()
        {
            if (SingleCustomerUserUID == string.Empty)
            {
                TPaaSServicesConfig.SetupEnvironment();
                SingleCustomerUserUID = UserIdentityService.CreateUser(SingleCustomerUserLoginId, SingleCustomerUserPassword);
            }

            return SingleCustomerUserUID;
        }

        public string GetValidUserAccessToken(string userLoginId)
        {
            string accessToken = string.Empty;
            string customerUserLoginId = string.Empty;
            string customerUserPassword = string.Empty;

            if (userLoginId.Contains("CustomerListMultipleUser"))
            {
                CreateValidMultipeCustomerUserWithUID();
                customerUserLoginId = MultipleCustomerUserLoginId;
                customerUserPassword = MultipleCustomerUserPassword;
            }
            else
            {
                CreateValueSingleCustomerUserWithUID();
                customerUserLoginId = SingleCustomerUserLoginId;
                customerUserPassword = SingleCustomerUserPassword;
            }

            string userTokenEndpoint = TokenService.GetTokenAPIEndpointUpdated(TPaaSServicesConfig.TPaaSTokenEndpoint, customerUserLoginId, customerUserPassword);
            accessToken = TokenService.GetAccessToken(userTokenEndpoint, CustomerListConfig.WebAPIConsumerKey, CustomerListConfig.WebAPIConsumerSecret);

            return accessToken;
        }
    }
}
