using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Filter.Common.Models;

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
      MySqlHelper mysql = new MySqlHelper();
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
      mysql.VerifyTestResultDatabaseRecordCount("Filter","FilterUID",1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET"); 

    }

    [TestMethod]
    public void CreateFilterToWebApiAndGetItFromWebApi()
    {
      msg.Title("Filter test 2", "Create filter then request it from WebApi");
      var ts = new TestSupport { IsPublishToWebApi = true};
      var filterUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var projectUid = Guid.NewGuid();
      var filterRequest = FilterRequest.CreateFilterRequest("", ""); //"Filter test 2","{ designUid: xxx, elevationType: First,layerType: 2,machineDesignName: test,polygonLL: 123 }");
      filterRequest.filterUid = filterUid.ToString();
      //filterRequest.filterJson =  "{ designUid: xxx, elevationType: First,layerType: 2,machineDesignName: test,polygonLL: 123 }";
      filterRequest.name = "Filter test 2";
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v4/filter/{projectUid}", "PUT", filter);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");

    }

    //private string CreateFilter()
    //{
    //  var startUtc = DateTime.Now.AddMonths(-6).ToUniversalTime();
    //  var endUtc = DateTime.Now.AddMonths(-6).ToUniversalTime();
    //  List<MachineDetails> listMachines =
    //  var filter = VSS.MasterData.Models.Models.Filter.CreateFilter(Guid.NewGuid().ToString(),startUtc,endUtc, 
    //    Guid.NewGuid().ToString(),,"machineName",ElevationType.First, true,,true,3, FilterLayerMethod.Automatic);
    //  return filter.ToJsonString();
    //}

  }
}
