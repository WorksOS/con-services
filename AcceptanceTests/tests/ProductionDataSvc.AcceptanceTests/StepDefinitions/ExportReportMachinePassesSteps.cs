using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System.Net;
using System.Text;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ExportReportMachinePasses")]
  public sealed class ExportReportMachinePassesSteps
  {
    private string url;

    private Getter<ExportReportResult> exportReportRequester;

    [Given(@"the Machine Passes Export Report service URI ""(.*)"" and the result file ""(.*)""")]
    public void GivenTheMachinePassesExportReportServiceURIAndTheResultFile(string url, string resultFileName)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
      exportReportRequester = new Getter<ExportReportResult>(url, resultFileName);
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      exportReportRequester.QueryString.Add("ProjectUid", projectUid);
    }

    [Given(@"coordType ""(.*)""")]
    public void GivenCoordType(int coordType)
    {
      exportReportRequester.QueryString.Add("coordType", coordType.ToString());
    }

    [Given(@"outputType ""(.*)""")]
    public void GivenOutputType(int outputType)
    {
      exportReportRequester.QueryString.Add("outputType", outputType.ToString());
    }

    [Given(@"restrictOutput ""(.*)""")]
    public void GivenRestrictOutput(string restrictOutput)
    {
      exportReportRequester.QueryString.Add("restrictOutput", restrictOutput);
    }

    [Given(@"rawDataOutput ""(.*)""")]
    public void GivenRawDataOutput(string rawDataOutput)
    {
      exportReportRequester.QueryString.Add("rawDataOutput", rawDataOutput);
    }

    [Given(@"fileName is ""(.*)""")]
    public void GivenFileNameIs(string fileName)
    {
      exportReportRequester.QueryString.Add("fileName", fileName);
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      exportReportRequester.QueryString.Add("filterUid", filterUid);
    }

    [When(@"I request an Export Report Machine Passes")]
    public void WhenIRequestAnExportReportMachinePasses()
    {
      exportReportRequester.DoValidRequest(url);
    }

    [When(@"I request an Export Report Machine Passes expecting BadRequest")]
    public void WhenIRequestAnExportReportMachinePassesExpectingBadRequest()
    {
      exportReportRequester.DoInvalidRequest(url);
    }
    [When(@"I request an Export Report Machine Passes expecting Unauthorized")]
    public void WhenIRequestAnExportReportMachinePassesExpectingUnauthorized()
    {
      exportReportRequester.DoInvalidRequest(url, HttpStatusCode.Unauthorized);
    }

    [When(@"I request an Export Report Machine Passes expecting NoContent")]
    public void WhenIRequestAnExportReportMachinePassesExpectingNoContent()
    {
      exportReportRequester.DoInvalidRequest(url, HttpStatusCode.NoContent);
    }

    [Then(@"the report result should match the ""(.*)"" from the repository")]
    public void ThenTheReportResultShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(exportReportRequester.ResponseRepo[resultName], exportReportRequester.CurrentResponse);
    }

    /// <summary>
    /// This method unzips the csv file. It then sorts it by date time and lat/long or northing then compares to expected 
    /// </summary>
    /// <param name="resultName"></param>
    [Then(@"the report result csv should match the ""(.*)"" from the repository")]
    public void ThenTheReportResultCsvShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(0, exportReportRequester.CurrentResponse.Code);
      Assert.AreEqual("success", exportReportRequester.CurrentResponse.Message);

      var actualResult = Encoding.Default.GetString(Common.Decompress(exportReportRequester.CurrentResponse.ExportData));
      var expectedResult = Encoding.Default.GetString(Common.Decompress(exportReportRequester.ResponseRepo[resultName].ExportData));
      var expSorted = Common.SortCsvFileIntoString(expectedResult.Substring(3));
      var actSorted = Common.SortCsvFileIntoString(actualResult.Substring(3));

      Assert.IsTrue(expSorted == actSorted, "Expected CSV file does not match actual");
    }


    [Then(@"the report result should contain error code (.*) and error message ""(.*)""")]
    public void ThenTheReportResultShouldContainErrorCodeAndErrorMessage(int errorCode, string errorMessage)
    {
      Assert.IsTrue(exportReportRequester.CurrentResponse.Code == errorCode && (exportReportRequester.CurrentResponse.Message == errorMessage || exportReportRequester.CurrentResponse.Message.Contains(errorMessage)),
        $"Expected to see code {errorCode} and message {errorMessage}, but got {exportReportRequester.CurrentResponse.Code} and {exportReportRequester.CurrentResponse.Message} instead.");
    }
  }
}