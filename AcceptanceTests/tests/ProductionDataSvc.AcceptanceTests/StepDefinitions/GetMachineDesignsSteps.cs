using System;
using TechTalk.SpecFlow;
using System.Net;
using System.Collections.Generic;
using RaptorSvcAcceptTestsCommon.Utils;
using ProductionDataSvc.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "GetMachineDesigns")]
  public class GetMachineDesignsSteps
  {
    private Getter<GetMachineDesignResult> machineDesignRequester;
    private Getter<GetMachineDesignDetailsResult> machineDesignDetailsRequester;
    private string operation;

    [Given(@"the Machine Design service URI ""(.*)""")]
    public void GivenTheMachineDesignServiceURI(string uri)
    {
      uri = RaptorClientConfig.ProdSvcBaseUri + uri;
      machineDesignRequester = new Getter<GetMachineDesignResult>(uri);
    }

    [Given(@"a project Id (.*)")]
    public void GivenAProjectId(int projectId)
    {
      machineDesignRequester.Uri = String.Format(machineDesignRequester.Uri, projectId);
    }

    [When(@"I request machine designs")]
    public void WhenIRequestMachineDesigns()
    {
      machineDesignRequester.DoValidRequest();
    }

    [When(@"I request machine designs expecting Bad Request")]
    public void WhenIRequestMachineDesignsExpectingBadRequest()
    {
      machineDesignRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
    }

    [Then(@"the following machine designs should be returned")]
    public void ThenTheFollowingMachineDesignsShouldBeReturned(Table designs)
    {
      GetMachineDesignResult expectedResult = new GetMachineDesignResult();

      // Get expected machine designs from feature file
      List<DesignName> expectedDesigns = new List<DesignName>();
      foreach (var design in designs.Rows)
      {
        expectedDesigns.Add(new DesignName()
        {
          designId = Convert.ToInt64(design["designId"]),
          designName = design["designName"]
        });
      }

      expectedResult.designs = expectedDesigns;

      Assert.AreEqual(expectedResult, machineDesignRequester.CurrentResponse);
    }

    [Then(@"the response should contain Code (.*) and Message ""(.*)""")]
    public void ThenTheResponseShouldContainCodeAndMessage(int code, string message)
    {
      Assert.IsTrue(machineDesignRequester.CurrentResponse.Code == code &&
                    machineDesignRequester.CurrentResponse.Message == message);
    }

    [Given(@"the Machine Design Details service URI ""(.*)"" for operation ""(.*)"" and the result file ""(.*)""")]
    public void GivenTheMachineDesignDetailsServiceURIForOperationAndTheResultFile(string uri, string operation, string resultFileName)
    {
      uri = RaptorClientConfig.ProdSvcBaseUri + uri;
      this.operation = operation;
      switch (operation)
      {
        case "machinedesigns":
          machineDesignRequester = new Getter<GetMachineDesignResult>(uri, resultFileName);
          break;
        case "machinedesigndetails":
          machineDesignDetailsRequester = new Getter<GetMachineDesignDetailsResult>(uri, resultFileName);
          break;
      }
    }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      switch (operation)
      {
        case "machinedesigns":
          machineDesignRequester.Uri = String.Format(machineDesignRequester.Uri, projectUid);
          break;
        case "machinedesigndetails":
          machineDesignDetailsRequester.Uri = String.Format(machineDesignDetailsRequester.Uri, projectUid);
          break;
      }
    }

    [Given(@"startUTC ""(.*)""")]
    public void GivenStartUTC(string startUTC)
    {
      switch (operation)
      {
        case "machinedesigns":
          //Dates don't apply to machine designs end point only to details
          break;
        case "machinedesigndetails":
          machineDesignDetailsRequester.QueryString.Add("startUtc", startUTC);
          break;
      }
    }

    [Given(@"endUTC ""(.*)""")]
    public void GivenEndUTC(string endUTC)
    {
      switch (operation)
      {
        case "machinedesigns":
          //Dates don't apply to machine designs end point only to details
          break;
        case "machinedesigndetails":
          machineDesignDetailsRequester.QueryString.Add("endUtc", endUTC);
          break;
      }
    }

    [When(@"I request machine design details")]
    public void WhenIRequestMachineDesignDetails()
    {
      switch (operation)
      {
        case "machinedesigns":
          machineDesignRequester.DoValidRequest();
          break;
        case "machinedesigndetails":
          machineDesignDetailsRequester.DoValidRequest();
          break;
      }
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      switch (operation)
      {
        case "machinedesigns":
          Assert.AreEqual(machineDesignRequester.ResponseRepo[resultName], machineDesignRequester.CurrentResponse);
          break;
        case "machinedesigndetails":
          Assert.AreEqual(machineDesignDetailsRequester.ResponseRepo[resultName], machineDesignDetailsRequester.CurrentResponse);
          break;
      }
    }

  }
}
