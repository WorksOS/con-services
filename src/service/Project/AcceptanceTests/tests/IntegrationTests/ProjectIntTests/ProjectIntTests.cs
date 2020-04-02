using System;
using System.Net;
using System.Threading.Tasks;
using TestUtility;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Repositories.DBModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace IntegrationTests.ProjectIntTests
{
  public class ProjectIntTests
  {
    private const string _polygon1 = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
     

    [Fact]
    public async Task CreateStandardProjectThenUpdateProjectTypeToLandFillAndCheckDatabase()
    {
      Msg.Title("Project Integration test", "Create standard project then update project type to landfill and check project-only database");
      var ts = new TestSupport();
      var legacyProjectId = TestSupport.GenerateLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = _polygon1;
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      await ts.PublishEventCollection(eventsArray);
      MySqlHelper.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);
      ts.IsPublishToWebApi = true;
      var projectName = "Standard To LandFill test";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);
      MySqlHelper.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid.ToString(), projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | {projectName} | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | dummy description |" };
      var response = await ts.PublishEventToWebApi(projectEventArray2);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid.ToString(), projectEventArray2, true);

      // Now check the project type has changed and the kafka message consumed properly.
      MySqlHelper.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "fk_ProjectTypeID", "1", projectUid);
    }   
  }
}
