using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;
using VSS.MasterData.Models.Models;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ProjectSettingsPost")]
  public class ProjectSettingsPostSteps
  {
    private Poster<ProjectSettingsRequest, RequestResult> projectSettingsPoster;

    [Given(@"the Project Settings service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
    public void GivenTheProjectSettingsServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
    {
      uri = RaptorClientConfig.CompactionSvcBaseUri + uri;
      projectSettingsPoster = new Poster<ProjectSettingsRequest, RequestResult>(uri, requestFile, resultFile);
    }

    [When(@"I Post ProjectSettingsValidation supplying ""(.*)"" paramters from the repository")]
    public void WhenIPostProjectSettingsValidationSupplyingParamtersFromTheRepository(string paramName)
    {
      projectSettingsPoster.DoValidRequest(paramName);
    }

    [Then(@"the ProjectSettingsValidation response should match ""(.*)"" result from the repository")]
    public void ThenTheProjectSettingsValidationResponseShouldMatchResultFromTheRepository(string resultName)
    {
      Assert.AreEqual(projectSettingsPoster.ResponseRepo[resultName], projectSettingsPoster.CurrentResponse);
    }
  }
}
