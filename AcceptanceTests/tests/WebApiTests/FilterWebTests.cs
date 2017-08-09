using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using TestUtility;

namespace WebApiTests
{
  [TestClass]
  public class FilterWebTests
  {

    private readonly Msg msg = new Msg();

    [TestMethod]
    public void InsertFilterInDatabaseAndGetItFromWebApi()
    {
      msg.Title("Filter test 1", "Insert Filter In Database And Get It From WebApi");
      var ts = new TestSupport();
      var filterUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var projectUid = Guid.NewGuid();
      var userUid = Guid.NewGuid();
      var eventsArray = new[] {
       "| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name          | FilterJson    | IsDeleted | LastActionedUTC |",
      $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userUid}  | Filter test 1 | Filter test 1 | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      var reponse = ts.CallFilterWebApi($"api/v1/filters/{projectUid}", "GET"); //?filterUid={filterUid}"

    }
  }
}
