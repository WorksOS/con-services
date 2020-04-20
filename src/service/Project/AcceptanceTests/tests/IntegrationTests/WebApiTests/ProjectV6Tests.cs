using System;
using System.Net;
using System.Threading.Tasks;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.MasterData.Models.Internal;
using Xunit;

namespace IntegrationTests.WebApiTests
{
  public class ProjectV6Tests : WebApiTestsBase
  {
    [Fact]
    public async Task CreateStandardProject()
    {
      var testText = "Project v6 test 1";
      Msg.Title(testText, "Create standard project.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);
    }

    [Fact]
    public async Task CreateStandardProjectWithNoCustomerUid()
    {
      var testText = "Project v6 test 2";
      Msg.Title(testText, "Create standard project with no customerUid.");
      var ts = new TestSupport();
      ts.SetCustomerUid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | " };
      var response = await ts.PublishEventToWebApi(projectEventArray);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, ts.CustomerUid, projectEventArray, true);
    }

    [Fact]
    public async Task CreateStandardProjectWithCoordinateSystem()
    {
      var testText = "Project v6 test 3";
      Msg.Title(testText, "Create standard project with CoordinateSystem.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   | CoordinateSystem      | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);
    }

    [Fact]
    public async Task CreateStandardProjectThenUpdateCoordinateSystem()
    {
      var testText = "Project v6 test 4";
      Msg.Title(testText, "Create standard project then update, change name and add CoordinateSystem.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} | " };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);

      // note that customerUID in this list for the http header
      // note no boundary in update
      testText += "_Updated";
      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID                 | ProjectName | ProjectType | CoordinateSystem      | CustomerUID   | ",
      $"| UpdateProjectRequest | 0d+09:00:00 | {ts.ProjectUid.ToString()} | {testText}  | Standard    | BootCampDimensions.dc | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray2);

      var projectEventArrayCombined = new[] {
       "| EventType            | EventDate   | ProjectUID                 | ProjectName | ProjectType | CoordinateSystem      | CustomerUID   | ProjectTimezone           | ProjectBoundary          | ",
      $"| UpdateProjectRequest | 0d+09:00:00 | {ts.ProjectUid.ToString()} | {testText}  | Standard    | BootCampDimensions.dc | {customerUid} | New Zealand Standard Time | {Boundaries.Boundary1}   |" };

      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArrayCombined, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArrayCombined, true);
    }

    [Fact]
    public async Task CreateStandardProjectThenUpdateBoundary()
    {
      var testText = "Project v6 test 5";
      Msg.Title(testText, "Create standard project then update, change boundary.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} | " };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);

      testText += "_Updated";
      const string updatedGeometryWkt = "POLYGON((-122 39,-122.3 39.8,-122.3 39.8,-122.34 39.83,-122.8 39.4,-122 39))";
      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID      | ProjectName | ProjectType | ProjectBoundary          | CustomerUID   | ",
      $"| UpdateProjectRequest | 0d+09:00:00 | {ts.ProjectUid} | {testText}  | Standard    | {updatedGeometryWkt}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray2);

      var projectEventArrayCombined = new[] {
       "| EventType            | EventDate   | ProjectUID      | ProjectName | ProjectType | ProjectBoundary        | CustomerUID   | ProjectTimezone           | ",
      $"| UpdateProjectRequest | 0d+09:00:00 | {ts.ProjectUid} | {testText}  | Standard    | {updatedGeometryWkt}   | {customerUid} | New Zealand Standard Time |" };

      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArrayCombined, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArrayCombined, true);
    }

    [Fact]
    public async Task CreateStandardProjectThenDelete()
    {
      var testText = "Project v6 test 6";
      Msg.Title(testText, "Create standard project then delete.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} | " };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);

      // project deletion sets the ProjectEndDate to now, in the projects timezone.
      //    this may cause the endDate to be a day earlier/later than 'NowUtc',
      //    depending on when this test is run.
      // note that only projectUID is passed from this array to the ProjectSvc endpoint,
      //    the others are simply used for comparison
      var endDateTime2Reset = DateTime.UtcNow.ToLocalDateTime("Pacific/Auckland")?.Date;
      ts.FirstEventDate = DateTime.UtcNow;

      var projectEventArray2 = new[] {
         "| EventType          | EventDate   | ProjectUID      | ProjectEndDate                                   | ",
        $"| DeleteProjectEvent | 1d+09:00:00 | {ts.ProjectUid} | {endDateTime2Reset:yyyy-MM-ddTHH:mm:ss.fffffff}  | " };
      var response = await ts.PublishEventToWebApi(projectEventArray2);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
    }

    [Fact]
    public async Task Create2StandardProjectsThenUpdateBoundary_Overlapping()
    {
      var testText = "Project v6 test 7";
      Msg.Title(testText, "Create 2 standard project2 then update with overlapping boundary.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      const string updatedGeometryWkt = "POLYGON((-12 3,-12.3 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary        | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {updatedGeometryWkt}   | {customerUid} | " };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);

      testText += "_2ndProject";
      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} | " };
      await ts.PublishEventCollection(projectEventArray2);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray2, true);

      testText += "_Updated";
      var projectEventArray3 = new[] {
       "| EventType            | EventDate   | ProjectUID                 | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary        | CustomerUID   | ",
      $"| UpdateProjectRequest | 0d+09:00:00 | {ts.ProjectUid.ToString()} | {testText}    | Standard    | New Zealand Standard Time | {updatedGeometryWkt}   | {customerUid} | " };
      var response = await ts.PublishEventToWebApi(projectEventArray3, HttpStatusCode.BadRequest);
      Assert.True(response == "Project boundary overlaps another project, for this customer and time span.", "Response is unexpected. Should fail with overlap. Response: " + response);
    }

    [Fact]
    public async Task CreateStandardProjectThenUpdateBoundary_OverlappingSelf_OK()
    {
      var testText = "Project v6 test 8";
      Msg.Title(testText, "Create standard project then update with overlapping boundary for self (OK).");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      const string geometryWkt =        "POLYGON((-12 3,-12.3 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      const string overlapGeometryWkt = "POLYGON((-12 3,-12.34 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone          | ProjectBoundary  | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time| {geometryWkt}    | {customerUid} | " };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);

      testText += "_Updated";
      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID                 | ProjectName | ProjectType | ProjectBoundary      | CustomerUID   | ",
      $"| UpdateProjectRequest | 0d+09:00:00 | {ts.ProjectUid.ToString()} | {testText}  | Standard    | {overlapGeometryWkt} | {customerUid} |" };
      var response = await ts.PublishEventToWebApi(projectEventArray2);
      
      var projectEventArrayCombined = new[] {
       "| EventType            | EventDate   | ProjectUID                 | ProjectName | ProjectType | ProjectBoundary        | CustomerUID   | ProjectTimezone           | ",
      $"| UpdateProjectRequest | 0d+09:00:00 | {ts.ProjectUid.ToString()} | {testText}  | Standard    | {overlapGeometryWkt}   | {customerUid} | New Zealand Standard Time |" };
      
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArrayCombined, true);
    }

    [Fact]
    public async Task Create2StandardProjectsWithAdjacentBoundarys()
    {
      var testText = "Project v6 test 9";
      Msg.Title(testText, "Create 2 standard projects with adjacent boundaries.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary3}   | {customerUid} | " };
      var response1 = await ts.PublishEventToWebApi(projectEventArray);
      Assert.True(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);
 
      testText += "_Updated";
      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary        | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary4} | {customerUid} | " };
      var response2 = await ts.PublishEventToWebApi(projectEventArray2);
      Assert.True(response2 == "success", "Response is unexpected. Should be a success. Response: " + response2);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray2, true);
    }

    [Fact]
    public async Task Create2StandardProjectsWithOverlappingBoundarys()
    {
      var testText = "Project v6 test 10";
      Msg.Title(testText, "Create 2 standard projects with overlapping boundaries.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary3}   | {customerUid} | " };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);
      var projectUid_firstCreate = ts.ProjectUid;

      testText += "_Updated";
      const string geometryWkt2 = "POLYGON((172.595071 -43.542112,172.595562 -43.543218,172.59766 -43.542353,172.595071 -43.542112))";
      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary  | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {geometryWkt2}   | {customerUid} | " };
      var response2 = await ts.PublishEventToWebApi(projectEventArray2, HttpStatusCode.BadRequest);
      Assert.True(response2 == "Project boundary overlaps another project, for this customer and time span.", "Response is unexpected. Should be a success. Response: " + response2);

      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid_firstCreate.ToString(), projectEventArray, true);
    }

    [Fact]
    public async Task Create2StandardProjectsWithSameNames()
    {
      var testText = "Project v6 test 11";
      Msg.Title(testText, "Create 2 standard projects with same names.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary3}   | {customerUid} | " };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);
      var projectUid_firstCreate = ts.ProjectUid;

      const string geometryWkt2 = "POLYGON((172.595071 -43.542112,172.595562 -43.543218,172.59766 -43.542353,172.595071 -43.542112))";
      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary  | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {geometryWkt2}   | {customerUid} | " };
      var response2 = await ts.PublishEventToWebApi(projectEventArray2, HttpStatusCode.BadRequest);
      Assert.True(response2 == $"UpsertProject Not allowed duplicate, active projectnames: Count:1 projectUid: {projectUid_firstCreate.ToString()}.", "Response is unexpected. Should be a success. Response: " + response2);

      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid_firstCreate.ToString(), projectEventArray, true);
    }

    [Fact]
    public async Task Create2StandardProjectsWithOverlappingBoundarysButNotTimes()
    {
      var testText = "Project v6 test 12";
      Msg.Title(testText, "Create 2 standard projects with overlapping boundaries but not times.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary3}   | {customerUid} | " };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray, true);

      testText += "_Updated";
      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary3}   | {customerUid} | " };
      var response2 = await ts.PublishEventToWebApi(projectEventArray2);
      Assert.True(response2 == "success", "Response is unexpected. Should be a success. Response: " + response2);
      
      await ts.GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, ts.ProjectUid.ToString(), projectEventArray2, true);
    }
  }
}
