using AutomationCore.Shared.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.GetAssetSettings;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSettings.GetDeviceType
{
    [Binding]
    public class GetDeviceTypeSteps
    {
        #region Variables
        private static Log4Net Log = new Log4Net(typeof(GetDeviceTypeSteps));
        private static GetDeviceTypeSupport DeviceTypeSupport = new GetDeviceTypeSupport(Log);

        #endregion

        [Given(@"I add a device to a customer")]
        public void GivenIAddADeviceToACustomer()
        {
            DeviceTypeSupport.SetDefaultValuesToDevice();
            DeviceTypeSupport.CreateDevice();
            DeviceTypeSupport.AssociateAssetDevice();
        }
        [Given(@"I change Customer details")]
        public void GivenIChangeCustomerDetails()
        {
            DeviceTypeSupport.ChangeCustomerUID();
        }

        [When(@"I try to get Device details")]
        public void WhenITryToGetDeviceDetails()
        {
            DeviceTypeSupport.GetDeviceTypeDetails();
        }


        [Then(@"Valid Device Details response should be returned")]
        public void ThenValidDeviceDetailsResponseShouldBeReturned()
        {
            DeviceTypeSupport.VerifyGetDeviceTypeDetails();
            DeviceTypeSupport.VerifyGetDeviceTypeDetailsCount();
        }

        [Given(@"I add same devices to a customer")]
        public void GivenIAddSameDevicesToACustomer()
        {
            DeviceTypeSupport.SetDefaultValuesToDevice();
            DeviceTypeSupport.CreateDevice();
            DeviceTypeSupport.CreateDevice();
            DeviceTypeSupport.AssociateAssetDevice();

        }
        [Then(@"No Details will be displayed")]
        public void ThenNoDetailsWillBeDisplayed()
        {
            DeviceTypeSupport.VerifyDeviceTypeDifferentCustomer();
        }
        [When(@"I try to get Device details with customerUID null")]
        public void WhenITryToGetDeviceDetailsWithCustomerUIDNull()
        {
            DeviceTypeSupport.GetDeviceTypeCustomerNull();
        }

        [Then(@"Valid Error Response should be thrown")]
        public void ThenValidErrorResponseShouldBeThrown()
        {
            DeviceTypeSupport.VerifyErrorResponse();
        }

    }
}
