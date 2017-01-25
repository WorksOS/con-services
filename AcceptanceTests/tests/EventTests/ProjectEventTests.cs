using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestUtility;

namespace EventTests
{
  [TestClass]
  public class ProjectEventTests
  {

    [TestMethod]
    public void CreateProjectEvent()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
             "| EventType          | EventDate   | ProjectID | ProjectGUID     | ProjectName   | ProjectType       | ProjectTimezone           | ProjectStartDate | ProjectEndDate |" ,
            $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject1  | ProjectMonitoring | New Zealand Standard Time | -1d+12:00:00     | 2d+01:00:00    |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer", "CustomerUID", "Name,fk_CustomerTypeID,IsDeleted", "CustName,1,0", projectGuid);
    }

  }
}
