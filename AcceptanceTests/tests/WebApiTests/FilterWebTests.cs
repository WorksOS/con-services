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
      TestUtility.MySqlHelper mysql = new TestUtility.MySqlHelper();
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
      var projectUid = new Guid("7925f179-013d-4aaf-aff4-7b9833bb06d6");
      var FilterJson = "{\"" +
                       "filterUid\":\"" + filterUid.ToString() + "\",\"" +  
                       "startUTC\":\"2017-08-13T23:37:51.3153239Z\",\"" +
                       "endUTC\":\"2017-08-23T23:37:51.3153239Z\",\"" +
                       "designUid\":\"220e12e5-ce92-4645-8f01-1942a2d5a57f\",\"" +
                       "contributingMachines\":[{\"assetID\":1137642418461469,\"" +
                       "machineName\":\"VOLVO G946B\",\"" +
                       "isJohnDoe\":false}],\"" +
                       "onMachineDesignID\":123,\"" +
                       "elevationType\":3,\"" +
                       "vibeStateOn\":true,\"" +
                       "polygonLL\":[" +
                       "{\"Lat\":0.6127702476232747,\"Lon\":-1.8605921222462667}," +
                       "{\"Lat\":0.64777024762327473,\"Lon\":-1.8605921222462667}," +
                       "{\"Lat\":0.6127702476232747,\"Lon\":-1.8255921222462668}],\"" +
                       "forwardDirection\":true,\"" +
                       "layerNumber\":1,\"" +
                       "layerType\":3}";
      var filterRequest = FilterRequest.CreateFilterRequest("Filter test 2", FilterJson);
      //filterRequest.filterUid = filterUid.ToString();
      //filterRequest.filterJson =  "{ designUid: xxx, elevationType: First,layerType: 2,machineDesignName: test,polygonLL: 123 }";
      filterRequest.name = "Filter test 2";
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter);
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
