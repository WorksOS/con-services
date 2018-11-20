using System;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using TestUtility.Model.WebApi;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace WebApiTests
{
  [TestClass]
  public class ExportJobTests
  {
    private TestSupport ts;
    private readonly Msg Msg = new Msg();
    private static readonly Guid CustomerUid = new Guid("48003241-851d-4145-8c2a-7b099bbfd117");

    private const string GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1 = "7925f179-013d-4aaf-aff4-7b9833bb06d6";
    private const string SUCCESS_JOB_ID = "Test_Job_1";
    private const string FAILURE_JOB_ID = "Test_Job_2";
    private const string TIMEOUT_JOB_ID = "Test_Job_4";

    [TestInitialize]
    public void Initialize()
    {
      ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = CustomerUid
      };
    }

    [TestMethod]
    public void CanDoExportSuccess()
    {
      Msg.Title("Scheduler web test 1", "Schedule export job happy path");

      //Schedule the export job...
      var filterUid = "81422acc-9b0c-401c-9987-0aedbf153f1d";
      var jobId = GetScheduledJobId(filterUid, SUCCESS_JOB_ID);

      //Get the job status...
      var statusResult = WaitForExpectedStatus(jobId, "SUCCEEDED");
      Assert.IsTrue(!string.IsNullOrEmpty(statusResult.DownloadLink), "Should get a download link on success");

      //Download the zip file...
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(statusResult.DownloadLink, "GET", null, HttpStatusCode.OK, string.Empty, CustomerUid.ToString());
      var actualSorted = Common.SortCsvFileIntoString(response);
      var expectedBytes = JsonConvert.DeserializeObject<byte[]>(
        "\"UEsDBBQAAAgIAEsQe0tesHMI2AEAANUIAAAIAAAAVGVzdC5jc3bNlM1um0AQgO+V+g4op1aabPeH5Sc3Sn4q1SDLRpZ6stawjVcB1gXiqn21HvpIfYUu1CZSAlVRKsVcZhdmhk/fDvz68TNRhYRQ5nm8Lrp4ZeJVLveiUbo067mo6/i+2MgKZqJuFiJTetaU6Te4lLW6LWNhGkQi3apSwnInZba+K95tu+Sb+TLSmQQTgzRNdG76JaK6bXuG+r5sYCVylc11DTP1uelqwmjV5bSx3UeX827fxg7gcP+6kl/WH75366DYrYs/rZOtSu9KWdfmVQeqGykqWKmNXDaikV1BIk1F+voVxYSex3p/jrmF/QtuXzAHMQ8DoZgjwjBQj1LECQbue4i75glgiIK5hQmchUFihUvuvD8DijDMdNpZsxbJRwi1qGppvcGIYq94Cww+ydpUA8cmlTAX+UC6tbnaJAwLuZemBmLdWMFul6tUbHIJg5juA6btPsJ0qDuGySZi2m3BMzD5gE2PH2xye9ymPc0mRu5jzGtdfRVV9k+YQ4feY7Z6T+PQ+cCh95hs9NB9RKbatJ9jc2g2j5iOwRyx+fKzyRDxj7Ppj39C7jSbBNH/ZPMp5knOZo/J2QHTYadss8c8bZuee7T5l9/7y9vsMV06bpNPw/Qn2PwNUEsBAhQAFAAACAgASxB7S16wcwjYAQAA1QgAAAgAAAAAAAAAAAAgAAAAAAAAAFRlc3QuY3N2UEsFBgAAAAABAAEANgAAAP4BAAAAAA==\"");
      var expected = Encoding.Default.GetString(Common.Decompress(expectedBytes));
      var expectedSorted = Common.SortCsvFileIntoString(expected);
      Assert.AreEqual(expectedSorted, actualSorted, "Export data does not match");
    }

    [TestMethod]
    public void CanGetExportJobStatusMissingJob()
    {
      Msg.Title("Scheduler web test 2", "Get Scheduled export job status for missing job");

      const string jobId = "999999";
      var responseJson = ts.CallSchedulerWebApi($"api/v1/export/{jobId}", "GET", null, HttpStatusCode.BadRequest);
      var result = JsonConvert.DeserializeObject<ContractExecutionResult>(responseJson,
        new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.IsNotNull(result, "Should be a error message");
      Assert.AreEqual($"Missing job details for {jobId}", result.Message, "Wrong error message");
    }

    [TestMethod]
    public void CanDoExportFailure()
    {
      Msg.Title("Scheduler web test 3", "Schedule export job with 3dpm failure code");

      //Schedule the export job...
      var filterUid = "1cf81668-1739-42d5-b068-ea025588796a";
      var jobId = GetScheduledJobId(filterUid, FAILURE_JOB_ID);

      //Get the job status...
      var statusResult = WaitForExpectedStatus(jobId, "FAILED");
      Assert.IsNotNull(statusResult.FailureDetails, "Should get details on failure");
      Assert.AreEqual(HttpStatusCode.BadRequest, statusResult.FailureDetails.Code, "Wrong http status code");
      Assert.AreEqual(2002, statusResult.FailureDetails.Result.Code, "Wrong failure code");
      Assert.AreEqual("Failed to get requested export data with error: No data for export", statusResult.FailureDetails.Result.Message, "Wrong failure message");
    }

    [TestMethod]
    public void CanDoLongRunningExport()
    {
      Msg.Title("Scheduler web test 4", "Schedule long running export job");

      //Schedule the export job...
      var filterUid = "81422acc-9b0c-401c-9987-0aedbf153f1d";
      var jobId = GetScheduledJobId(filterUid, TIMEOUT_JOB_ID);

      //Get the job status...
      var statusResult = WaitForExpectedStatus(jobId, "SUCCEEDED", 180);
      Assert.IsTrue(!string.IsNullOrEmpty(statusResult.DownloadLink), "Should get a download link on success");
    }

    [TestMethod]
    public void CanDoExportTimeout()
    {
      Msg.Title("Scheduler web test 5", "Schedule export job with timeout");

      //Schedule the export job...
      var filterUid = "81422acc-9b0c-401c-9987-0aedbf153f1d";
      var jobId = GetScheduledJobId(filterUid, TIMEOUT_JOB_ID, 100000);

      //Get the job status...
      var statusResult = WaitForExpectedStatus(jobId, "FAILED", 150);
      Assert.IsNotNull(statusResult.FailureDetails, "Should get details on failure");
      Assert.AreEqual(HttpStatusCode.InternalServerError, statusResult.FailureDetails.Code, "Wrong http status code");
      Assert.AreEqual(-3, statusResult.FailureDetails.Result.Code, "Wrong failure code");
      Assert.AreEqual("The operation has timed out.", statusResult.FailureDetails.Result.Message, "Wrong failure message");

    }

    private string GetScheduledJobId(string filterUid, string filename, int timeoutMillisecs= 300000)//5 mins
    {
      var url = $"{ts.tsCfg.vetaExportUrl}?projectUid={GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1}&fileName={filename}&filterUid={filterUid}";
      var request = new ScheduleJobRequest { Url = url, Filename = filename, Timeout= timeoutMillisecs };
      Console.WriteLine($"Uri is {url}");
      var requestJson = JsonConvert.SerializeObject(request);
      var responseJson = ts.CallSchedulerWebApi("internal/v1/export", "POST", requestJson);
      Console.WriteLine($"Response from the mockApi is {responseJson}");
      var scheduleResult = JsonConvert.DeserializeObject<ScheduleJobResult>(responseJson,
        new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.IsNotNull(scheduleResult, "Should get a schedule job response");
      Assert.IsTrue(!string.IsNullOrEmpty(scheduleResult.JobId), "Should get a job id");
      return scheduleResult.JobId;
    }

    private JobStatusResult WaitForExpectedStatus(string jobId, string expectedStatus, int maxSeconds=60)
    {
      //Get the job status...
      JobStatusResult statusResult = new JobStatusResult { Status = string.Empty };
      //Avoid infinite loop if something goes wrong
      var timeout = DateTime.Now.AddSeconds(maxSeconds);
      while (!statusResult.Status.Equals(expectedStatus, StringComparison.OrdinalIgnoreCase) && DateTime.Now < timeout)
      {
        var responseJson = ts.CallSchedulerWebApi($"api/v1/export/{jobId}", "GET");
        statusResult = JsonConvert.DeserializeObject<JobStatusResult>(responseJson,
          new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

        Assert.IsNotNull(statusResult, "Should get a job status response");
        Console.WriteLine($"Scheduled Job Status: {statusResult.Status}");
        Thread.Sleep(5000);
      }
      if (!statusResult.Status.Equals(expectedStatus, StringComparison.OrdinalIgnoreCase))
        Assert.Fail("Test timed out");
      return statusResult;
    }
  }
}
