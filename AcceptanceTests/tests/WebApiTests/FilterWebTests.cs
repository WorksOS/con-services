using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace WebApiTests
{
  [TestClass]
  public class FilterWebTests
  {
    private readonly Msg msg = new Msg();
    private readonly Guid projectUid  = new Guid("7925f179-013d-4aaf-aff4-7b9833bb06d6");
    private readonly Guid customerUid = new Guid("48003241-851d-4145-8c2a-7b099bbfd117");
    private readonly Guid userUid = new Guid("98cdb619-b06b-4084-b7c5-5dcccc82af3b");

    [TestMethod]
    public void InsertFilterInDatabaseAndGetItFromWebApi()
    {
      const string filterName = "Filter test 1";
      msg.Title(filterName, "Insert Filter In Database And Get It From WebApi");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString());
      var eventsArray = new[] {
       "| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
      $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter","FilterUID",1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void InsertElevationTypeLastFilterInDatabaseAndGetItFromWebApi()
    {
      const string filterName = "Filter test 2";
      msg.Title(filterName, "Insert ElevationType.Last Filter In Database And Get It From WebApi");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString(),ElevationType.Last);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void InsertVibeStateOnFilterInDatabaseAndGetItFromWebApi()
    {
      const string filterName = "Filter test 3";
      msg.Title(filterName, "Insert VibeStateOn Filter In Database And Get It From WebApi");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString(),null,true);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void InsertForwardFilterInDatabaseAndGetItFromWebApi()
    {
      const string filterName = "Filter test 4";
      msg.Title(filterName, "Insert forward direction filter In Database And Get It From WebApi");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString(),null,null,true);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void InsertLayerNoFilterInDatabaseAndGetItFromWebApi()
    {
      const string filterName = "Filter test 5";
      msg.Title(filterName, "Insert layer number filter In Database And Get It From WebApi");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString(), null, null, null, 2);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void InsertFilterLayerMethodNoneFilterInDatabaseAndGetItFromWebApi()
    {
      const string filterName = "Filter test 6";
      msg.Title(filterName, "Insert FilterLayerMethod None filter In Database And Get It From WebApi");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString(), null, null, null, null, FilterLayerMethod.None);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void InsertFilterLayerMethodMapResetFilterInDatabaseAndGetItFromWebApi()
    {
      const string filterName = "Filter test 7";
      msg.Title(filterName, "Insert FilterLayerMethod MapReset filter In Database And Get It From WebApi");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString(), null, null, null, null, FilterLayerMethod.MapReset);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void InsertFilterLayerMethodTagfileLayerNumberFilterInDatabaseAndGetItFromWebApi()
    {
      const string filterName = "Filter test 8";
      msg.Title(filterName, "Insert FilterLayerMethod TagfileLayerNumber filter In Database And Get It From WebApi");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString(), null, null, null, null, FilterLayerMethod.TagfileLayerNumber);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void InsertCombinationFilterInDatabaseAndGetItFromWebApi()
    {
      const string filterName = "Filter test 9";
      msg.Title(filterName, "Insert Combination filter In Database And Get It From WebApi");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString(),ElevationType.Highest,true,true,1, FilterLayerMethod.None);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void InsertMutipleFiltersInDatabaseAndGetItFromWebApi()
    {
      const string filterName = "Filter test 10";
      msg.Title(filterName, "Insert mutilple filters In Database And Get It From WebApi");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var filterUid1 = Guid.NewGuid();
      var filterUid2 = Guid.NewGuid();
      var filterUid3 = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson1 = CreateTestFilter(filterUid1.ToString(), ElevationType.Highest, true, true, 1, FilterLayerMethod.None);
      var filterJson2 = CreateTestFilter(filterUid2.ToString(), ElevationType.Last, true, true, 1, FilterLayerMethod.MapReset);
      var filterJson3 = CreateTestFilter(filterUid3.ToString(), ElevationType.Lowest, true, true, 1, FilterLayerMethod.None);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | fk_UserUID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid1} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson1}  | 0         | {ts.EventDate:yyyy-MM-dd} |",
        $"| Filter    | {filterUid2} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson2}  | 0         | {ts.EventDate:yyyy-MM-dd} |",
        $"| Filter    | {filterUid3} | {customerUid}  | {projectUid}  | {userUid}  | {filterName} | {filterJson3}  | 0         | {ts.EventDate:yyyy-MM-dd} |"

      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid1);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid2);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid3);
      var responseGet = ts.CallFilterWebApi($"api/v1/filters/{projectUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorListResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptors.Count, 3, "Expecting 3 filters in response");

      for (var cnt = 0; cnt < 3; cnt++)
      {
        switch (cnt)
        {
          case 0:
            Assert.AreEqual(filterResponseGet.filterDescriptors[cnt].FilterJson, filterJson1, "JSON Filter doesn't match for GET request");
            break;
          case 1:
            Assert.AreEqual(filterResponseGet.filterDescriptors[cnt].FilterJson, filterJson2, "JSON Filter doesn't match for GET request");
            break;
          case 2:
            Assert.AreEqual(filterResponseGet.filterDescriptors[cnt].FilterJson, filterJson3, "JSON Filter doesn't match for GET request");
            break;
        }
      }
    }




    [TestMethod]
    public void CreateFilterToWebApiAndGetItFromWebApi()
    {
      const string filterName = "Filter test 20";
      msg.Title(filterName, "Create filter then request it from WebApi");
      var ts = new TestSupport { IsPublishToWebApi = true};
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString());
      
      var filterRequest = FilterRequest.CreateFilterRequest(filterUid.ToString(), filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter);
      try
      {
        var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        Assert.AreEqual(filterResponse.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for PUT request");
        Assert.AreEqual(filterResponse.filterDescriptor,filterName, "Filter name doesn't match for PUT request");
        var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
        var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
        Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      }
      catch (Exception)
      {
        Assert.Fail("Failed to deserialize the json response" + responseCreate);
      }
    }

    /// <summary>
    /// Create the filter and convert it to json 
    /// </summary>
    /// <param name="filterUid">filter uid</param>
    /// <param name="elevation">ElevationType</param>
    /// <param name="vibestate">true or false</param>
    /// <param name="forward">true or false</param>
    /// <param name="layerNo">layer number</param>
    /// <param name="fltlayer">FilterLayerMethod</param>
    /// <returns>complete filter in json format</returns>
    private string CreateTestFilter(string filterUid, ElevationType? elevation = null,bool? vibestate = null, bool? forward = null,
                                    int? layerNo = null, FilterLayerMethod? fltlayer = null)
    {
      var startUtc = DateTime.Now.AddMonths(-6).ToUniversalTime();
      var endUtc = DateTime.Now.AddMonths(+6).ToUniversalTime();
      var listMachines = new List<MachineDetails>();
      var machine = MachineDetails.CreateMachineDetails(123456789,"TheMachineName", false);
      listMachines.Add(machine);
      var listPoints = new List<WGSPoint>
      {
        WGSPoint.CreatePoint(38.8361907402694, -121.349260032177),
        WGSPoint.CreatePoint(38.8361656688414, -121.349217116833),
        WGSPoint.CreatePoint(38.8387897637231, -121.347275197506),
        WGSPoint.CreatePoint(38.8387145521594, -121.347189366818)
      };
      var filter = Filter.CreateFilter(filterUid, startUtc, endUtc,Guid.NewGuid().ToString(), listMachines, 
                                       "machine Design Name", elevation,vibestate,listPoints,forward, layerNo, fltlayer);
      return filter.ToJsonString();
    }


    //"{\"" +
    //"filterUid\":\"" + filterUid.ToString() + "\",\"" +  
    //"startUTC\":\"2017-08-13T23:37:51.3153239Z\",\"" +
    //"endUTC\":\"2017-08-23T23:37:51.3153239Z\",\"" +
    //"designUid\":\"220e12e5-ce92-4645-8f01-1942a2d5a57f\",\"" +
    //"contributingMachines\":[{\"assetID\":1137642418461469,\"" +
    //"machineName\":\"VOLVO G946B\",\"" +
    //"isJohnDoe\":false}],\"" +
    //"onMachineDesignID\":123,\"" +
    //"elevationType\":3,\"" +
    //"vibeStateOn\":true,\"" +
    //"polygonLL\":[" +
    //"{\"Lat\":0.6127702476232747,\"Lon\":-1.8605921222462667}," +
    //"{\"Lat\":0.64777024762327473,\"Lon\":-1.8605921222462667}," +
    //"{\"Lat\":0.6127702476232747,\"Lon\":-1.8255921222462668}],\"" +
    //"forwardDirection\":true,\"" +
    //"layerNumber\":1,\"" +
    //"layerType\":3}";


  }
}
