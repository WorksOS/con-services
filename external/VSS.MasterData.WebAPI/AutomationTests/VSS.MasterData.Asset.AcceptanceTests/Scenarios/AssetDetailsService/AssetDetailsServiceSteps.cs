using AutomationCore.Shared.Library;
using Newtonsoft.Json;
using System;
using TechTalk.SpecFlow;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerService;
using VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSearchService;
using VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSubscription;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceService;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerAssetService;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerRelationship;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerService;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetDetailsService
{
    [Binding]
    public class AssetDetailsServiceSteps
    {
        #region Variables
        public string TestName;

        private static Log4Net Log = new Log4Net(typeof(AssetDetailsServiceSteps));
        public static AssetDetailsServiceSupport assetDetailsServiceSupport = new AssetDetailsServiceSupport(Log);
        private static CustomerServiceSupport customerServiceSupport = new CustomerServiceSupport(Log);
        private static CustomerRelationshipServiceSupport customerRelationshipServiceSupport = new CustomerRelationshipServiceSupport(Log);
        private static CustomerAssetServiceSupport customerAssetServiceSupport = new CustomerAssetServiceSupport(Log);
        private static AssetDeviceSearchServiceSupport assetDeviceSearchServiceSupport = new AssetDeviceSearchServiceSupport(Log);
        public static AssetSubscriptionModel defaultValidAssetSubscriptionModel = new AssetSubscriptionModel();

        public static Guid oldCustomerUID = Guid.Empty;
        public static Guid dealerUID = Guid.Empty;
        public static Guid customerUID = Guid.Empty;

        public static Guid oldAssetUID = Guid.Empty;
        public static Guid assetUID = Guid.Empty;
        public static Guid deviceUID = Guid.Empty;

        #endregion

        [Given(@"AssetDetailService Is Ready To Verify '(.*)'")]
        public void GivenAssetDetailServiceIsReadyToVerify(string testDescription)
        {
            //log the scenario info
            TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + testDescription;
            //TestName = TestDescription;
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
        }

        [Given(@"CustomerServiceCreate Request Is Setup With Default Values")]
        public void GivenCustomerServiceCreateRequestIsSetupWithDefaultValues()
        {
            customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
            //string requestCustomerString = JsonConvert.SerializeObject(customerServiceSupport.CreateCustomerModel);

            customerServiceSupport.PostValidCreateRequestToService();
            dealerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;

            assetDetailsServiceSupport.DealerModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            assetDetailsServiceSupport.DealerModel.CustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;
            assetDetailsServiceSupport.DealerModel.CustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;

            customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
            customerServiceSupport.CreateCustomerModel.CustomerType = "Customer";
        }

        [Given(@"CustomerServiceCreate Request Is Setup For AssetAndDevice Verification")]
        public void GivenCustomerServiceCreateRequestIsSetupForAssetAndDeviceVerification()
        {
            //first dealer
            customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
            customerServiceSupport.PostValidCreateRequestToService();
            dealerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;

            assetDetailsServiceSupport.FirstDealerModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            assetDetailsServiceSupport.FirstDealerModel.CustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;
            assetDetailsServiceSupport.FirstDealerModel.CustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;

            //first customer
            customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
            customerServiceSupport.CreateCustomerModel.CustomerType = "Customer";
            
            customerServiceSupport.PostValidCreateRequestToService();
            customerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;

            assetDetailsServiceSupport.FirstCustomerModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            assetDetailsServiceSupport.FirstCustomerModel.CustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;
            assetDetailsServiceSupport.FirstCustomerModel.CustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;

            //first dealer customer relationship
            customerRelationshipServiceSupport.CreateCustomerRelationshipModel = CustomerRelationshipServiceSteps.GetDefaultValidCustomerRelationshipServiceCreateRequest();
            customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ParentCustomerUID = dealerUID;
            customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ChildCustomerUID = customerUID;
            customerRelationshipServiceSupport.PostValidCreateCustomerRelationshipRequestToService();

            //second dealer
            customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
            customerServiceSupport.PostValidCreateRequestToService();
            dealerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;

            assetDetailsServiceSupport.DealerModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            assetDetailsServiceSupport.DealerModel.CustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;
            assetDetailsServiceSupport.DealerModel.CustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;

            //second customer
            customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
            customerServiceSupport.CreateCustomerModel.CustomerType = "Customer";

        }

        [When(@"I Post Valid CustomerServiceCreate Request")]
        public void WhenIPostValidCustomerServiceCreateRequest()
        {
            customerServiceSupport.PostValidCreateRequestToService();
            customerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;

            assetDetailsServiceSupport.CustomerModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            assetDetailsServiceSupport.CustomerModel.CustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;
            assetDetailsServiceSupport.CustomerModel.CustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;

            customerRelationshipServiceSupport.CreateCustomerRelationshipModel = CustomerRelationshipServiceSteps.GetDefaultValidCustomerRelationshipServiceCreateRequest();
            customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ParentCustomerUID = dealerUID;
            customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ChildCustomerUID = customerUID;
            customerRelationshipServiceSupport.PostValidCreateCustomerRelationshipRequestToService();
        }

        [Given(@"CustomerServiceCreate Request Is Setup for '(.*)'")]
        public void GivenCustomerServiceCreateRequestIsSetupFor(string associationType)
        {
            if (associationType == "CustomerWithSameDealer")
            {
                customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
                customerServiceSupport.PostValidCreateRequestToService();
                dealerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;

                assetDetailsServiceSupport.DealerModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
                assetDetailsServiceSupport.DealerModel.CustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;
                assetDetailsServiceSupport.DealerModel.CustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;

                customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
                customerServiceSupport.CreateCustomerModel.CustomerType = "Customer";

                customerServiceSupport.PostValidCreateRequestToService();
                customerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
                oldCustomerUID = customerUID;

                assetDetailsServiceSupport.FirstCustomerModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
                assetDetailsServiceSupport.FirstCustomerModel.CustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;
                assetDetailsServiceSupport.FirstCustomerModel.CustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;

                customerRelationshipServiceSupport.CreateCustomerRelationshipModel = CustomerRelationshipServiceSteps.GetDefaultValidCustomerRelationshipServiceCreateRequest();
                customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ParentCustomerUID = dealerUID;
                customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ChildCustomerUID = customerUID;
                customerRelationshipServiceSupport.PostValidCreateCustomerRelationshipRequestToService();

                customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
                customerServiceSupport.CreateCustomerModel.CustomerType = "Customer";
            }
            else
            {
                customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
                customerServiceSupport.PostValidCreateRequestToService();
                dealerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;

                assetDetailsServiceSupport.DealerModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
                assetDetailsServiceSupport.DealerModel.CustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;
                assetDetailsServiceSupport.DealerModel.CustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;

                customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
                customerServiceSupport.CreateCustomerModel.CustomerType = "Customer";
            }
        }

        [When(@"DeviceAssetAssociation Request Is Setup With Default Values")]
        public void WhenDeviceAssetAssociationRequestIsSetupWithDefaultValues()
        {
            AssetServiceSteps.assetServiceSupport.CreateAssetModel = AssetServiceSteps.GetDefaultValidAssetServiceCreateRequest();
            if (customerUID != Guid.Empty)
            {
                AssetServiceSteps.assetServiceSupport.CreateAssetModel.OwningCustomerUID = customerUID;
            }
            AssetServiceSteps.assetServiceSupport.PostValidCreateRequestToService();
            assetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceServiceCreateRequest();
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.PostValidCreateRequestToService();
            deviceUID = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID;
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.DeviceAssetAssociationModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceAssetAssociationServiceRequest();
        }

        [Given(@"DeviceAssetAssociation Request Is Setup With Default Values")]
        public void GivenDeviceAssetAssociationRequestIsSetupWithDefaultValues()
        {
            AssetServiceSteps.assetServiceSupport.CreateAssetModel = AssetServiceSteps.GetDefaultValidAssetServiceCreateRequest();
            if (customerUID != Guid.Empty)
            {
                AssetServiceSteps.assetServiceSupport.CreateAssetModel.OwningCustomerUID = customerUID;
            }
            AssetServiceSteps.assetServiceSupport.PostValidCreateRequestToService();
            assetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceServiceCreateRequest();
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.PostValidCreateRequestToService();
            deviceUID = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID;
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.DeviceAssetAssociationModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceAssetAssociationServiceRequest();
        }

        [When(@"DeviceAssetAssociation Request Is Setup for '(.*)'")]
        public void WhenDeviceAssetAssociationRequestIsSetupFor(string associationType)
        {
            if (associationType == "CustomerWithSameDealer")
            {
                AssetServiceSteps.assetServiceSupport.CreateAssetModel = AssetServiceSteps.GetDefaultValidAssetServiceCreateRequest();
                if (oldCustomerUID != Guid.Empty)
                {
                    AssetServiceSteps.assetServiceSupport.CreateAssetModel.OwningCustomerUID = oldCustomerUID;
                }
                AssetServiceSteps.assetServiceSupport.PostValidCreateRequestToService();
                assetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
                oldAssetUID = assetUID;
                AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceServiceCreateRequest();
                AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.PostValidCreateRequestToService();
                deviceUID = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID;
                AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.DeviceAssetAssociationModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceAssetAssociationServiceRequest();

                AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.PostValidDeviceAssetAssociationRequestToService();

                assetDetailsServiceSupport.FirstAssetModel.AssetName = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetName;
                assetDetailsServiceSupport.FirstAssetModel.SerialNumber = AssetServiceSteps.assetServiceSupport.CreateAssetModel.SerialNumber;
                assetDetailsServiceSupport.FirstAssetModel.MakeCode = AssetServiceSteps.assetServiceSupport.CreateAssetModel.MakeCode;
                assetDetailsServiceSupport.FirstAssetModel.Model = AssetServiceSteps.assetServiceSupport.CreateAssetModel.Model;
                assetDetailsServiceSupport.FirstAssetModel.AssetType = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetType;
                assetDetailsServiceSupport.FirstAssetModel.ModelYear = AssetServiceSteps.assetServiceSupport.CreateAssetModel.ModelYear;
                assetDetailsServiceSupport.FirstAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;


                assetDetailsServiceSupport.FirstDeviceModel.DeviceSerialNumber = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceSerialNumber;
                assetDetailsServiceSupport.FirstDeviceModel.DeviceType = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceType;
                assetDetailsServiceSupport.FirstDeviceModel.DeviceState = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceState;
                assetDetailsServiceSupport.FirstDeviceModel.DeviceUID = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID;

                AssetServiceSteps.assetServiceSupport.CreateAssetModel = AssetServiceSteps.GetDefaultValidAssetServiceCreateRequest();
                if (customerUID != Guid.Empty)
                {
                    AssetServiceSteps.assetServiceSupport.CreateAssetModel.OwningCustomerUID = customerUID;
                }
                AssetServiceSteps.assetServiceSupport.PostValidCreateRequestToService();
                assetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
                AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceServiceCreateRequest();
                AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.PostValidCreateRequestToService();
                deviceUID = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID;
                AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.DeviceAssetAssociationModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceAssetAssociationServiceRequest();
            }
            else
            {
                AssetServiceSteps.assetServiceSupport.CreateAssetModel = AssetServiceSteps.GetDefaultValidAssetServiceCreateRequest();
                if (customerUID != Guid.Empty)
                {
                    AssetServiceSteps.assetServiceSupport.CreateAssetModel.OwningCustomerUID = customerUID;
                }
                AssetServiceSteps.assetServiceSupport.PostValidCreateRequestToService();
                assetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
                AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceServiceCreateRequest();
                AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.PostValidCreateRequestToService();
                deviceUID = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID;
                AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.DeviceAssetAssociationModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceAssetAssociationServiceRequest();
            }
        }



        [When(@"DeviceAssetAssociation Request Is Setup For AssetAndDevice Verification")]
        public void WhenDeviceAssetAssociationRequestIsSetupForAssetAndDeviceVerification()
        {
            //first asset
            AssetServiceSteps.assetServiceSupport.CreateAssetModel = AssetServiceSteps.GetDefaultValidAssetServiceCreateRequest();
            if (assetDetailsServiceSupport.FirstCustomerModel.CustomerUID != Guid.Empty)
            {
                AssetServiceSteps.assetServiceSupport.CreateAssetModel.OwningCustomerUID = assetDetailsServiceSupport.FirstCustomerModel.CustomerUID;
            }
            AssetServiceSteps.assetServiceSupport.PostValidCreateRequestToService();
            assetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            oldAssetUID = assetUID;
            //first device
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceServiceCreateRequest();
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.PostValidCreateRequestToService();
            deviceUID = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID;
            //first device asset association
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.DeviceAssetAssociationModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceAssetAssociationServiceRequest();
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.PostValidDeviceAssetAssociationRequestToService();

            assetDetailsServiceSupport.FirstAssetModel.AssetName = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetName;
            assetDetailsServiceSupport.FirstAssetModel.SerialNumber = AssetServiceSteps.assetServiceSupport.CreateAssetModel.SerialNumber;
            assetDetailsServiceSupport.FirstAssetModel.MakeCode = AssetServiceSteps.assetServiceSupport.CreateAssetModel.MakeCode;
            assetDetailsServiceSupport.FirstAssetModel.Model = AssetServiceSteps.assetServiceSupport.CreateAssetModel.Model;
            assetDetailsServiceSupport.FirstAssetModel.AssetType = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetType;
            assetDetailsServiceSupport.FirstAssetModel.ModelYear = AssetServiceSteps.assetServiceSupport.CreateAssetModel.ModelYear;
            assetDetailsServiceSupport.FirstAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;


            assetDetailsServiceSupport.FirstDeviceModel.DeviceSerialNumber = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceSerialNumber;
            assetDetailsServiceSupport.FirstDeviceModel.DeviceType = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceType;
            assetDetailsServiceSupport.FirstDeviceModel.DeviceState = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceState;
            assetDetailsServiceSupport.FirstDeviceModel.DeviceUID = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID;

            //second asset
            AssetServiceSteps.assetServiceSupport.CreateAssetModel = AssetServiceSteps.GetDefaultValidAssetServiceCreateRequest();
            if (customerUID != Guid.Empty)
            {
                AssetServiceSteps.assetServiceSupport.CreateAssetModel.OwningCustomerUID = customerUID;
            }
            AssetServiceSteps.assetServiceSupport.PostValidCreateRequestToService();
            assetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            //second device
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceServiceCreateRequest();
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.PostValidCreateRequestToService();
            deviceUID = AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID;
            //second device asset association
            AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.DeviceAssetAssociationModel = AssetDeviceSearchServiceSteps.GetDefaultValidDeviceAssetAssociationServiceRequest();
        }

        [When(@"AssetCustomerAssociation Request Is Setup For AssetAndDevice Verification")]
        public void WhenAssetCustomerAssociationRequestIsSetupForAssetAndDeviceVerification()
        {
            //first Dealer first Asset association
            customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
            customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = assetDetailsServiceSupport.FirstDealerModel.CustomerUID;    
            customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = assetDetailsServiceSupport.FirstAssetModel.AssetUID;
            customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();
            
            //first Customer first Asset association
            customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
            customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = assetDetailsServiceSupport.FirstCustomerModel.CustomerUID; ;
            customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = assetDetailsServiceSupport.FirstAssetModel.AssetUID;
            customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();

            //second Dealer second Asset association
            customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
            customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = dealerUID;
            customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();
            //second customer second Asset association
            customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
            customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = customerUID;
            customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
        }

        [When(@"AssetCustomerAssociation Request Is Setup With Default Values")]
        public void WhenAssetCustomerAssociationRequestIsSetupWithDefaultValues()
        {
            customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
            customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = dealerUID;
            customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();

            customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
            customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = customerUID;
            customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
        }

        [When(@"I Post Valid AssetCustomerAssociation Request")]
        public void WhenIPostValidAssetCustomerAssociationRequest()
        {
            customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();
        }

        [When(@"AssetSubscription Request Is Setup With Default Values")]
        public void WhenAssetSubscriptionRequestIsSetupWithDefaultValues()
        {
            assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
        }

        [When(@"AssetSubscription Request Is Setup For '(.*)', '(.*)', '(.*)' And '(.*)'")]
        public void WhenAssetSubscriptionRequestIsSetupForActiveAnd(string subscriptionScenarioType, string subscription, string activeState, string customerType)
        {
            
            switch(subscriptionScenarioType)
            {
                case "OneActive_Customer":
                    assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
                    assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType = subscription;
                    assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = customerUID;
                    break;

                case "OneActive_Dealer":
                    assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
                    assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType = subscription;
                    assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = dealerUID;
                    break;

                case "OneInactive_Customer":
                    assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
                    assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType = subscription;
                    assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = customerUID;
                    defaultValidAssetSubscriptionModel.StartDate = DateTime.UtcNow.AddDays(-3);
                    defaultValidAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddDays(-2);
                    break;

                case "OneInactive_Dealer":
                    assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
                    assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType = subscription;
                    assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = dealerUID;
                    defaultValidAssetSubscriptionModel.StartDate = DateTime.UtcNow.AddDays(-3);
                    defaultValidAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddDays(-2);
                    break;

                case "TwoActive":
                    string[] activeSubscriptions = subscription.Split(',');
                    string[] activeCustomerTypes = customerType.Split(',');

                    assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
                    assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType = activeSubscriptions[0];
                    if (activeCustomerTypes[0] == "Customer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = customerUID;
                    }
                    else if (activeCustomerTypes[0] == "Dealer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = dealerUID;
                    }
                    assetDetailsServiceSupport.PostValidSubscriptionRequestToService();

                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.SubscriptionUID = assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.CustomerUID = assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.AssetUID = assetDetailsServiceSupport.AssetSubscriptionModel.AssetUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.DeviceUID = assetDetailsServiceSupport.AssetSubscriptionModel.DeviceUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.SubscriptionType = assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.StartDate = assetDetailsServiceSupport.AssetSubscriptionModel.StartDate;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.EndDate = assetDetailsServiceSupport.AssetSubscriptionModel.EndDate;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.ActionUTC = assetDetailsServiceSupport.AssetSubscriptionModel.ActionUTC;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.ReceivedUTC = assetDetailsServiceSupport.AssetSubscriptionModel.ReceivedUTC;

                    assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
                    assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType = activeSubscriptions[1];
                    if (activeCustomerTypes[1] == "Customer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = customerUID;
                    }
                    else if (activeCustomerTypes[1] == "Dealer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = dealerUID;
                    }

                    break;

                case "TwoInactive":
                    string[] inactiveSubscriptions = subscription.Split(',');
                    string[] inactiveCustomerTypes = customerType.Split(',');

                    assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
                    assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType = inactiveSubscriptions[0];
                    if (inactiveCustomerTypes[0] == "Customer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = customerUID;
                    }
                    else if (inactiveCustomerTypes[0] == "Dealer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = dealerUID;
                    }
                    defaultValidAssetSubscriptionModel.StartDate = DateTime.UtcNow.AddDays(-3);
                    defaultValidAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddDays(-2);
                    assetDetailsServiceSupport.PostValidSubscriptionRequestToService();

                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.SubscriptionUID = assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.CustomerUID = assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.AssetUID = assetDetailsServiceSupport.AssetSubscriptionModel.AssetUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.DeviceUID = assetDetailsServiceSupport.AssetSubscriptionModel.DeviceUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.SubscriptionType = assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.StartDate = assetDetailsServiceSupport.AssetSubscriptionModel.StartDate;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.EndDate = assetDetailsServiceSupport.AssetSubscriptionModel.EndDate;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.ActionUTC = assetDetailsServiceSupport.AssetSubscriptionModel.ActionUTC;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.ReceivedUTC = assetDetailsServiceSupport.AssetSubscriptionModel.ReceivedUTC;

                    assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
                    assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType = inactiveSubscriptions[1];
                    if (inactiveCustomerTypes[1] == "Customer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = customerUID;
                    }
                    else if (inactiveCustomerTypes[1] == "Dealer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = dealerUID;
                    }
                    defaultValidAssetSubscriptionModel.StartDate = DateTime.UtcNow.AddDays(-3);
                    defaultValidAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddDays(-2);
                    break;

                case "OneActiveOneInactive":
                    string[] inactive_Active_Subscriptions = subscription.Split(',');
                    string[] inactive_Active_CustomerTypes = customerType.Split(',');
                    string[] activeStates = activeState.Split(',');

                    assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
                    assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType = inactive_Active_Subscriptions[0];
                    if (inactive_Active_CustomerTypes[0] == "Customer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = customerUID;
                    }
                    else if (inactive_Active_CustomerTypes[0] == "Dealer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = dealerUID;
                    }
                    if (activeStates[0] == "Inactive")
                    {
                        defaultValidAssetSubscriptionModel.StartDate = DateTime.UtcNow.AddDays(-3);
                        defaultValidAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddDays(-2);
                    }
                    assetDetailsServiceSupport.PostValidSubscriptionRequestToService();

                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.SubscriptionUID = assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.CustomerUID = assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.AssetUID = assetDetailsServiceSupport.AssetSubscriptionModel.AssetUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.DeviceUID = assetDetailsServiceSupport.AssetSubscriptionModel.DeviceUID;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.SubscriptionType = assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.StartDate = assetDetailsServiceSupport.AssetSubscriptionModel.StartDate;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.EndDate = assetDetailsServiceSupport.AssetSubscriptionModel.EndDate;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.ActionUTC = assetDetailsServiceSupport.AssetSubscriptionModel.ActionUTC;
                    assetDetailsServiceSupport.FirstAssetSubscriptionModel.ReceivedUTC = assetDetailsServiceSupport.AssetSubscriptionModel.ReceivedUTC;

                    assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
                    assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType = inactive_Active_Subscriptions[1];
                    if (inactive_Active_CustomerTypes[1] == "Customer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = customerUID;
                    }
                    else if (inactive_Active_CustomerTypes[1] == "Dealer")
                    {
                        assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = dealerUID;
                    }
                    if (activeStates[1] == "Inactive")
                    {
                        defaultValidAssetSubscriptionModel.StartDate = DateTime.UtcNow.AddDays(-3);
                        defaultValidAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddDays(-2);
                    }
                    break;

                default:
                    break;
            }
        }

        [When(@"AssetSubscription Request Is Setup For AssetAndDevice Verification")]
        public void WhenAssetSubscriptionRequestIsSetupForAssetAndDeviceVerification()
        {
            assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
            assetDetailsServiceSupport.AssetSubscriptionModel.AssetUID = assetDetailsServiceSupport.FirstAssetModel.AssetUID;
            assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID = assetDetailsServiceSupport.FirstCustomerModel.CustomerUID;
            assetDetailsServiceSupport.AssetSubscriptionModel.DeviceUID = assetDetailsServiceSupport.FirstDeviceModel.DeviceUID;
            assetDetailsServiceSupport.PostValidSubscriptionRequestToService();

            assetDetailsServiceSupport.FirstAssetSubscriptionModel.SubscriptionUID = assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionUID;
            assetDetailsServiceSupport.FirstAssetSubscriptionModel.CustomerUID = assetDetailsServiceSupport.AssetSubscriptionModel.CustomerUID;
            assetDetailsServiceSupport.FirstAssetSubscriptionModel.AssetUID = assetDetailsServiceSupport.AssetSubscriptionModel.AssetUID;
            assetDetailsServiceSupport.FirstAssetSubscriptionModel.DeviceUID = assetDetailsServiceSupport.AssetSubscriptionModel.DeviceUID;
            assetDetailsServiceSupport.FirstAssetSubscriptionModel.SubscriptionType = assetDetailsServiceSupport.AssetSubscriptionModel.SubscriptionType;
            assetDetailsServiceSupport.FirstAssetSubscriptionModel.StartDate = assetDetailsServiceSupport.AssetSubscriptionModel.StartDate;
            assetDetailsServiceSupport.FirstAssetSubscriptionModel.EndDate = assetDetailsServiceSupport.AssetSubscriptionModel.EndDate;
            assetDetailsServiceSupport.FirstAssetSubscriptionModel.ActionUTC = assetDetailsServiceSupport.AssetSubscriptionModel.ActionUTC;
            assetDetailsServiceSupport.FirstAssetSubscriptionModel.ReceivedUTC = assetDetailsServiceSupport.AssetSubscriptionModel.ReceivedUTC;

            assetDetailsServiceSupport.AssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceRequest();
        }

        [When(@"AssetCustomerAssociation Request Is Setup for Asset With '(.*)'")]
        public void WhenAssetCustomerAssociationRequestIsSetupForAssetWith(string associationType)
        {
            if (associationType == "Dealer")
            {
                customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
                customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = dealerUID;
                customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            }
            else if (associationType == "Customer")
            {
                customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
                customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = customerUID;
                customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            }
            else if (associationType == "CustomerDealer")
            {
                customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
                customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = dealerUID;
                customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
                customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();

                customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
                customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = customerUID;
                customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            }
            else if (associationType == "CustomerWithSameDealer")
            {
                customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
                customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = dealerUID;
                customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = oldAssetUID;
                customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();

                customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
                customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = oldCustomerUID;
                customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = oldAssetUID;
                customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();

                customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
                customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = dealerUID;
                customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
                customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();

                customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
                customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = customerUID;
                customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID = AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID;
            }
        }

        [When(@"I Post Valid AssetSubscription Request")]
        public void WhenIPostValidAssetSubscriptionRequest()
        {
            assetDetailsServiceSupport.PostValidSubscriptionRequestToService();
        }

        [When(@"I Get the AssetDetails For AssetUID")]
        public void WhenIGetTheAssetDetailsForAssetUID()
        {
            assetDetailsServiceSupport.GetAssetDetails(assetUID.ToString(), null);
        }

        [Given(@"I Get the AssetDetails For '(.*)' And '(.*)'")]
        public void GivenIGetTheAssetDetailsForAnd(string assetUID, string deviceUID)
        {
            assetDetailsServiceSupport.GetAssetDetails(assetUID, deviceUID);
        }

        [When(@"I Get the AssetDetails For '(.*)'")]
        public void WhenIGetTheAssetDetailsFor(string parameterType)
        {
            if (parameterType == "CustomerWithSameDealer")
            {
                assetDetailsServiceSupport.GetAssetDetails_Association(oldAssetUID.ToString(), assetUID.ToString());
            }
            else if (parameterType == "AssetAndDevice")
            {
                assetDetailsServiceSupport.GetAssetDetails(oldAssetUID.ToString(), deviceUID.ToString());
            }
            else
            {
                assetDetailsServiceSupport.GetAssetDetails(assetUID.ToString(), null);
            }
        }

        [When(@"I Get the AssetDetails For DeviceUID")]
        public void WhenIGetTheAssetDetailsForDeviceUID()
        {
            assetDetailsServiceSupport.GetAssetDetails(null, deviceUID.ToString());
        }

        [When(@"I Get the AssetDetails For AssetUID And DeviceUID")]
        public void WhenIGetTheAssetDetailsForAssetUIDAndDeviceUID()
        {
            assetDetailsServiceSupport.GetAssetDetails(assetUID.ToString(), deviceUID.ToString());
        }

        [Then(@"AssetDetails Response With All information Should Be Returned")]
        public void ThenAssetDetailsResponseWithAllInformationShouldBeReturned()
        {
            assetDetailsServiceSupport.VerifyResponse("HappyPath");
        }

        [Then(@"AssetDetails Response With Only Asset And Device Information Should Be Returned")]
        public void ThenAssetDetailsResponseWithOnlyAssetAndDeviceInformationShouldBeReturned()
        {
            assetDetailsServiceSupport.VerifyResponse("WithoutAccountAndSubscription");
        }

        [Then(@"AssetDetails Response With Asset, Device And Account Information Should Be Returned")]
        public void ThenAssetDetailsResponseWithAssetDeviceAndAccountInformationShouldBeReturned()
        {
            assetDetailsServiceSupport.VerifyResponse("WithoutSubscription");
        }

        [Then(@"AssetDetails Response With Account information '(.*)' Should Be Returned")]
        public void ThenAssetDetailsResponseWithAccountInformationShouldBeReturned(string verifyParameter)
        {
            assetDetailsServiceSupport.VerifyResponse_Association(verifyParameter);
        }

        [Then(@"AssetDetails Response With All Information '(.*)' Should Be Returned")]
        public void ThenAssetDetailsResponseWithAllInformationShouldBeReturned(string subscriptionType)
        {
            assetDetailsServiceSupport.VerifyResponse_Subscription(subscriptionType);
        }

        [Then(@"AssetDetails Response With All Information With AssetAndDevice Should Be Returned")]
        public void ThenAssetDetailsResponseWithAllInformationWithAssetAndDeviceShouldBeReturned()
        {
            assetDetailsServiceSupport.VerifyResponse_AssetAndDevice();
        }

        [Then(@"AssetDetails Response With '(.*)' Should Be Returned")]
        public void ThenAssetDetailsResponseWithShouldBeReturned(string errorMessage)
        {
            assetDetailsServiceSupport.VerifyErrorResponse(errorMessage);
        }

        public static AssetSubscriptionModel GetDefaultValidAssetSubscriptionServiceRequest()
        {
            defaultValidAssetSubscriptionModel.SubscriptionUID = Guid.NewGuid();
            defaultValidAssetSubscriptionModel.CustomerUID = dealerUID;
            defaultValidAssetSubscriptionModel.AssetUID = assetUID;
            defaultValidAssetSubscriptionModel.DeviceUID = deviceUID;
            defaultValidAssetSubscriptionModel.SubscriptionType = "Essentials";
            defaultValidAssetSubscriptionModel.StartDate = DateTime.UtcNow.AddDays(-1);
            defaultValidAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddDays(1);
            defaultValidAssetSubscriptionModel.ActionUTC = DateTime.UtcNow;
            defaultValidAssetSubscriptionModel.ReceivedUTC = null;

            return defaultValidAssetSubscriptionModel;
        }
    }
}
