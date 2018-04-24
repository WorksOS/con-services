using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionCellDatum")]
  public sealed class CompactionCellDatumSteps
  {
    private string url;

    private Getter<CompactionCellDatumResult> compactionCellDatumRequester;

    [Given(@"the CompactionCellDatum service URI ""(.*)"" and result repo ""(.*)""")]
    public void GivenTheCompactionCellDatumServiceURIAndResultRepo(string url, string resultFileName)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
      compactionCellDatumRequester = new Getter<CompactionCellDatumResult>(url, resultFileName);
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      compactionCellDatumRequester.QueryString.Add("projectUid", projectUid);
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      if (!string.IsNullOrEmpty(filterUid))
        compactionCellDatumRequester.QueryString.Add("filterUid", filterUid);
    }

    [Given(@"cutfillDesignUid ""(.*)""")]
    public void GivenCutfillDesignUid(string cutfillDesignUid)
    {
      if (!string.IsNullOrEmpty(cutfillDesignUid))
        compactionCellDatumRequester.QueryString.Add("cutfillDesignUid", cutfillDesignUid);
    }

    [Given(@"displayMode ""(.*)""")]
    public void GivenDisplayMode(byte displayMode)
    {
      compactionCellDatumRequester.QueryString.Add("displayMode", displayMode.ToString());
    }

    [Given(@"lat ""(.*)""")]
    public void GivenLat(Decimal lat)
    {
      compactionCellDatumRequester.QueryString.Add("lat", lat.ToString(CultureInfo.CurrentCulture));
    }

    [Given(@"lon ""(.*)""")]
    public void GivenLon(Decimal lon)
    {
      compactionCellDatumRequester.QueryString.Add("lon", lon.ToString(CultureInfo.CurrentCulture));
    }

    [When(@"I request Compaction Cell Datum")]
    public void WhenIRequestCompactionCellDatum()
    {
      compactionCellDatumRequester.DoValidRequest(url);
    }

    [Then(@"the Compaction Cell Datum response should match ""(.*)"" result from the repository")]
    public void ThenTheCompactionCellDatumResponseShouldMatchResultFromTheRepository(string resultName)
    {
      Assert.AreEqual(compactionCellDatumRequester.ResponseRepo[resultName], compactionCellDatumRequester.CurrentResponse);
    }

    [When(@"I request Compaction Cell Datum I expect http error code (.*)")]
    public void WhenIRequestCompactionCellDatumIExpectHttpErrorCode(int httpCode)
    {
      compactionCellDatumRequester.DoInvalidRequest(url, (HttpStatusCode)httpCode);
    }

    [Then(@"the response should contain error code (.*)")]
    public void ThenTheResponseShouldContainErrorCode(int expectedCode)
    {
      Assert.AreEqual(expectedCode, compactionCellDatumRequester.CurrentResponse.Code);
    }

  }
}
