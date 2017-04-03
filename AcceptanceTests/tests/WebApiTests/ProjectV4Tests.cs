using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace WebApiTests
{
  [TestClass]
  public class ProjectV4Tests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void CreateProjectAndGetProjectListV4()
    {
      msg.Title("Project v4 tests", "Create project and customer then read the project list");
      var ts = new TestSupport { };
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary | CustomerUID   | IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 1 | Standard    | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWKT}   | {customerUid} | false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray);
    }
  }
}
