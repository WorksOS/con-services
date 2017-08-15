using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace WebApiTests
{
    public class DbWebTests
    {
      private readonly Msg msg = new Msg();
      private readonly Guid projectUid = new Guid("7925f179-013d-4aaf-aff4-7b9833bb06d6");
      private readonly Guid customerUid = new Guid("48003241-851d-4145-8c2a-7b099bbfd117");
      private readonly string userId = new Guid("98cdb619-b06b-4084-b7c5-5dcccc82af3b").ToString();

    [TestMethod]
      public void InsertFilterInDatabaseAndGetItFromWebApi()
      {
        const string filterName = "Filter DbWeb test 1";
        msg.Title(filterName, "Insert Filter In Database And Get It From WebApi");
        var ts = new TestSupport();
        var mysql = new MySqlHelper();
        var filterUid = Guid.NewGuid();
        ts.CustomerUid = customerUid;
        var filterJson = CreateTestFilter();
        var eventsArray = new[] {
          "| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
          $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
        };
        ts.PublishEventCollection(eventsArray);
        mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
        var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
        var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
        Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      }

      [TestMethod]
      public void InsertElevationTypeLastFilterInDatabaseAndGetItFromWebApi()
      {
        const string filterName = "Filter DbWeb test 2";
        msg.Title(filterName, "Insert ElevationType.Last Filter In Database And Get It From WebApi");
        var ts = new TestSupport();
        var mysql = new MySqlHelper();
        var filterUid = Guid.NewGuid();
        ts.CustomerUid = customerUid;
        var filterJson = CreateTestFilter( ElevationType.Last);
        var eventsArray = new[] {
          $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
          $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
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
        const string filterName = "Filter DbWeb test 3";
        msg.Title(filterName, "Insert VibeStateOn Filter In Database And Get It From WebApi");
        var ts = new TestSupport();
        var mysql = new MySqlHelper();
        var filterUid = Guid.NewGuid();
        ts.CustomerUid = customerUid;
        var filterJson = CreateTestFilter( null, true);
        var eventsArray = new[] {
          $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
          $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
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
        const string filterName = "Filter DbWeb test 4";
        msg.Title(filterName, "Insert forward direction filter In Database And Get It From WebApi");
        var ts = new TestSupport();
        var mysql = new MySqlHelper();
        var filterUid = Guid.NewGuid();
        ts.CustomerUid = customerUid;
        var filterJson = CreateTestFilter( null, null, true);
        var eventsArray = new[] {
          $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
          $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
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
        const string filterName = "Filter DbWeb test 5";
        msg.Title(filterName, "Insert layer number filter In Database And Get It From WebApi");
        var ts = new TestSupport();
        var mysql = new MySqlHelper();
        var filterUid = Guid.NewGuid();
        ts.CustomerUid = customerUid;
        var filterJson = CreateTestFilter( null, null, null, 2);
        var eventsArray = new[] {
          $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
          $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
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
        const string filterName = "Filter DbWeb test 6";
        msg.Title(filterName, "Insert FilterLayerMethod None filter In Database And Get It From WebApi");
        var ts = new TestSupport();
        var mysql = new MySqlHelper();
        var filterUid = Guid.NewGuid();
        ts.CustomerUid = customerUid;
        var filterJson = CreateTestFilter( null, null, null, null, FilterLayerMethod.None);
        var eventsArray = new[] {
          $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
          $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
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
        const string filterName = "Filter DbWeb test 7";
        msg.Title(filterName, "Insert FilterLayerMethod MapReset filter In Database And Get It From WebApi");
        var ts = new TestSupport();
        var mysql = new MySqlHelper();
        var filterUid = Guid.NewGuid();
        ts.CustomerUid = customerUid;
        var filterJson = CreateTestFilter( null, null, null, null, FilterLayerMethod.MapReset);
        var eventsArray = new[] {
          $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
          $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userId}  | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
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
        const string filterName = "Filter DbWeb test 8";
        msg.Title(filterName, "Insert FilterLayerMethod TagfileLayerNumber filter In Database And Get It From WebApi");
        var ts = new TestSupport();
        var mysql = new MySqlHelper();
        var filterUid = Guid.NewGuid();
        ts.CustomerUid = customerUid;
        var filterJson = CreateTestFilter( null, null, null, null, FilterLayerMethod.TagfileLayerNumber);
        var eventsArray = new[] {
          $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
          $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
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
        const string filterName = "Filter DbWeb test 9";
        msg.Title(filterName, "Insert Combination filter In Database And Get It From WebApi");
        var ts = new TestSupport();
        var mysql = new MySqlHelper();
        var filterUid = Guid.NewGuid();
        ts.CustomerUid = customerUid;
        var filterJson = CreateTestFilter( ElevationType.Highest, true, true, 1, FilterLayerMethod.None);
        var eventsArray = new[] {
          $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
          $"| Filter    | {filterUid} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
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
        const string filterName = "Filter DbWeb test 10";
        msg.Title(filterName, "Insert mutilple filters In Database And Get It From WebApi");
        var ts = new TestSupport();
        var mysql = new MySqlHelper();
        ts.DeleteAllFiltersForProject(projectUid.ToString());

        var filterUid1 = Guid.NewGuid();
        var filterUid2 = Guid.NewGuid();
        var filterUid3 = Guid.NewGuid();
        ts.CustomerUid = customerUid;
        var filterJson1 = CreateTestFilter( ElevationType.Highest, true, true, 1, FilterLayerMethod.None);
        var filterJson2 = CreateTestFilter( ElevationType.Last, true, true, 1, FilterLayerMethod.MapReset);
        var filterJson3 = CreateTestFilter( ElevationType.Lowest, true, true, 1, FilterLayerMethod.None);
        var eventsArray = new[] {
          $"| TableName | FilterUID    | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | FilterJson    | IsDeleted | LastActionedUTC |",
          $"| Filter    | {filterUid1} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson1}  | 0         | {ts.EventDate:yyyy-MM-dd} |",
          $"| Filter    | {filterUid2} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson2}  | 0         | {ts.EventDate:yyyy-MM-dd} |",
          $"| Filter    | {filterUid3} | {customerUid}  | {projectUid}  | {userId} | {filterName} | {filterJson3}  | 0         | {ts.EventDate:yyyy-MM-dd} |"

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
      private string CreateTestFilter(ElevationType? elevation = null, bool? vibestate = null, bool? forward = null,
        int? layerNo = null, FilterLayerMethod? fltlayer = null)
      {
        var startUtc = DateTime.Now.AddMonths(-6).ToUniversalTime();
        var endUtc = DateTime.Now.AddMonths(+6).ToUniversalTime();
        var listMachines = new List<MachineDetails>();
        var machine = MachineDetails.CreateMachineDetails(123456789, "TheMachineName", false);
        listMachines.Add(machine);
        var listPoints = new List<WGSPoint>
        {
          WGSPoint.CreatePoint(38.8361907402694, -121.349260032177),
          WGSPoint.CreatePoint(38.8361656688414, -121.349217116833),
          WGSPoint.CreatePoint(38.8387897637231, -121.347275197506),
          WGSPoint.CreatePoint(38.8387145521594, -121.347189366818)
        };
        var filter = Filter.CreateFilter(startUtc, endUtc, Guid.NewGuid().ToString(), listMachines,123,
                                          elevation, vibestate, listPoints, forward, layerNo, fltlayer);
        return filter.ToJsonString();
      }
  }
}
