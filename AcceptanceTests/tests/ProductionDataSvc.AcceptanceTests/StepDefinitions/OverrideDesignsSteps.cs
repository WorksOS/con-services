using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding]
    public class OverrideDesignsSteps
    {
        private string editDataUri;
        private string getEditDataUri;
        private ServiceValidatorPOST<EditDataRequest, EditDataResult> submitOverrideDesignValidator;
        private EditDataResult submitOverrideResult;
        private ServiceValidatorPOST<GetEditDataRequest, GetEditDataResult> getOverrideDesignValidator;
        private GetEditDataResult getOverrideResultAfterOverride;
        private ServiceValidatorPOST<EditDataRequest, EditDataResult> submitUndoOverrideDesignValidator;
        private EditDataResult getUndoResult;
        private GetEditDataResult getOverrideResultUndoOverride;

        [Given(@"the edit data service URI ""(.*)"", the get data service URI ""(.*)"", request parameter repository file ""(.*)"", result repository file ""(.*)""")]
        public void GivenTheEditDataServiceURITheGetDataServiceURIRequestParameterRepositoryFileResultRepositoryFile(string dataEditServiceURI, string dataGetEditServiceURI, string requestRepository, string responseRepository)
        {
            this.editDataUri = RaptorClientConfig.ProdSvcBaseUri + dataEditServiceURI;
            this.getEditDataUri = RaptorClientConfig.ProdSvcBaseUri + dataGetEditServiceURI;
            submitOverrideDesignValidator = new ServiceValidatorPOST<EditDataRequest, EditDataResult>(editDataUri, requestRepository, responseRepository);
            getOverrideDesignValidator = new ServiceValidatorPOST<GetEditDataRequest, GetEditDataResult>(getEditDataUri, requestRepository, responseRepository);
            submitUndoOverrideDesignValidator = new ServiceValidatorPOST<EditDataRequest, EditDataResult>(editDataUri, requestRepository, responseRepository);
        }



        [When(@"I override design with ""(.*)"" supplying a request from a repository")]
        public void WhenIOverrideDesignWithSupplyingARequestFromARepository(string jsonRequest)
        {
            this.submitOverrideResult = submitOverrideDesignValidator.DoValidRequest(jsonRequest);
        }


        [When(@"I get the list of overriden designs supplying a request ""(.*)"" from a repository and save it to ""(.*)""")]
        public void WhenIGetTheListOfOverridenDesignsSupplyingARequestFromARepositoryAndSaveItTo(string jsonRequest, string toSave)
        {
            if (toSave == "<AfterOverride>")
                this.getOverrideResultAfterOverride = getOverrideDesignValidator.DoValidRequest(jsonRequest);
            if (toSave == "<AfterUndo>")
                this.getOverrideResultUndoOverride = getOverrideDesignValidator.DoValidRequest(jsonRequest);
        }


        [When(@"I undo design ""(.*)"" supplying a request from a repository")]
        public void WhenIUndoDesignSupplyingARequestFromARepository(string jsonRequest)
        {
            this.getUndoResult = submitUndoOverrideDesignValidator.DoValidRequest(jsonRequest);
        }

        [Then(@"The override design list before undo should include design ""(.*)""")]
        public void ThenTheOverrideDesignListBeforeUndoShouldIncludeDesign(string resultName)
        {
            Assert.AreEqual(getOverrideDesignValidator.GetExpectedResult(resultName), this.getOverrideResultAfterOverride);
        }

        [Then(@"The override design list should include only initial designs in ""(.*)""")]
        public void ThenTheOverrideDesignShouldNotIncludeDesign(string resultName)
        {
            Assert.AreEqual(getOverrideDesignValidator.GetExpectedResult(resultName), getOverrideResultUndoOverride);
        }

    }
}