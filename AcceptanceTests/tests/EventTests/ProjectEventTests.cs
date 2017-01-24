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
      var customerUid = Guid.NewGuid();
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
             "| EventType          | ProjectID | ActionUTC   | ProjectName | ProjectTimezone | ProjectBoundary | ProjectStartDate | ProjectEndDate",
            $"| CreateProjectEvent | 0         | 09:00:00    | CustName     | Customer     | {customerUid} |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer", "CustomerUID", "Name,fk_CustomerTypeID,IsDeleted", "CustName,1,0", customerUid);
    }

  }
}
