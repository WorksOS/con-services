using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Filter = VSS.MasterData.Models.Models.Filter;

namespace WebApiTests
{
  [TestClass]
  public class DbWebTests : WebTestBase
  {
    private TestSupport ts;
    private MySqlHelper mysql;

    [TestInitialize]
    public void Initialize()
    {
      ts = new TestSupport();
      mysql = new MySqlHelper();
    }

    #region Filters
    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void InsertFilterInDatabaseAndGetItFromWebApi(FilterType filterType)
    {
      const string filterName = "Filter DbWeb test 1";
      Msg.Title(filterName, "Insert Filter In Database And Get It From WebApi");
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var filterJson = CreateTestFilter();
      var eventsArray = new[] {
        "| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | fk_FilterTypeID   | FilterJson    | IsDeleted | LastActionedUTC |",
       $"| Filter    | {filterUid} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName} | {(int)filterType} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterType, filterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void InsertElevationTypeLastFilterInDatabaseAndGetItFromWebApi(FilterType filterType)
    {
      const string filterName = "Filter DbWeb test 2";
      Msg.Title(filterName, "Insert ElevationType.Last Filter In Database And Get It From WebApi");
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var filterJson = CreateTestFilter(ElevationType.Last);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | fk_FilterTypeID   | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName} | {(int)filterType} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterType, filterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void InsertVibeStateOnFilterInDatabaseAndGetItFromWebApi(FilterType filterType)
    {
      const string filterName = "Filter DbWeb test 3";
      Msg.Title(filterName, "Insert VibeStateOn Filter In Database And Get It From WebApi");
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var filterJson = CreateTestFilter(null, true);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | fk_FilterTypeID   | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName} | {(int)filterType} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterType, filterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void InsertForwardFilterInDatabaseAndGetItFromWebApi(FilterType filterType)
    {
      const string filterName = "Filter DbWeb test 4";
      Msg.Title(filterName, "Insert forward direction filter In Database And Get It From WebApi");
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var filterJson = CreateTestFilter(null, null, true);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | fk_FilterTypeID   | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName} | {(int)filterType} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterType, filterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void InsertLayerNoFilterInDatabaseAndGetItFromWebApi(FilterType filterType)
    {
      const string filterName = "Filter DbWeb test 5";
      Msg.Title(filterName, "Insert layer number filter In Database And Get It From WebApi");
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var filterJson = CreateTestFilter(null, null, null, 2);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | fk_FilterTypeID   | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName} | {(int)filterType} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterType, filterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void InsertFilterLayerMethodNoneFilterInDatabaseAndGetItFromWebApi(FilterType filterType)
    {
      const string filterName = "Filter DbWeb test 6";
      Msg.Title(filterName, "Insert FilterLayerMethod None filter In Database And Get It From WebApi");
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var filterJson = CreateTestFilter(null, null, null, null);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | fk_FilterTypeID   | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName} | {(int)filterType} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterType, filterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void InsertFilterLayerMethodMapResetFilterInDatabaseAndGetItFromWebApi(FilterType filterType)
    {
      const string filterName = "Filter DbWeb test 7";
      Msg.Title(filterName, "Insert FilterLayerMethod MapReset filter In Database And Get It From WebApi");
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var filterJson = CreateTestFilter();
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | fk_FilterTypeID   | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName} | {(int)filterType} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterType, filterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void InsertFilterLayerMethodTagfileLayerNumberFilterInDatabaseAndGetItFromWebApi(FilterType filterType)
    {
      const string filterName = "Filter DbWeb test 8";
      Msg.Title(filterName, "Insert FilterLayerMethod TagfileLayerNumber filter In Database And Get It From WebApi");
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var filterJson = CreateTestFilter();
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | fk_FilterTypeID   | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName} | {(int)filterType} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterType, filterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void InsertCombinationFilterInDatabaseAndGetItFromWebApi(FilterType filterType)
    {
      const string filterName = "Filter DbWeb test 9";
      Msg.Title(filterName, "Insert Combination filter In Database And Get It From WebApi");
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var filterJson = CreateTestFilter(ElevationType.Highest, true, true, 1);
      var eventsArray = new[] {
        $"| TableName | FilterUID   | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | fk_FilterTypeID   | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName} | {(int)filterType} | {filterJson}  | 0         | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid);
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterType, filterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void InsertMutipleFiltersInDatabaseAndGetItFromWebApi(FilterType filterType)
    {
      const string filterName = "Filter DbWeb test 10";
      //Persistent filters require a unique name
      string filterName1 = filterType == FilterType.Persistent ? $"{filterName}1" : $"{filterName}";
      string filterName2 = filterType == FilterType.Persistent ? $"{filterName}2" : $"{filterName}";
      string filterName3 = filterType == FilterType.Persistent ? $"{filterName}3" : $"{filterName}";
      Msg.Title(filterName, "Insert mutilple filters In Database And Get It From WebApi");
      ts.DeleteAllFiltersForProject(ProjectUid.ToString());

      var filterUid1 = Guid.NewGuid();
      var filterUid2 = Guid.NewGuid();
      var filterUid3 = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var filterJson1 = CreateTestFilter(ElevationType.Highest, true, true, 1);
      var filterJson2 = CreateTestFilter(ElevationType.Last, true, true, 1);
      var filterJson3 = CreateTestFilter(ElevationType.Lowest, true, true, 1);
      var eventsArray = new[] {
        $"| TableName | FilterUID    | fk_CustomerUID | fk_ProjectUID | UserID   | Name         | fk_FilterTypeID   | FilterJson    | IsDeleted | LastActionedUTC |",
        $"| Filter    | {filterUid1} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName1} | {(int)filterType} | {filterJson1}  | 0         | {ts.EventDate:yyyy-MM-dd} |",
        $"| Filter    | {filterUid2} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName2} | {(int)filterType} | {filterJson2}  | 0         | {ts.EventDate:yyyy-MM-dd} |",
        $"| Filter    | {filterUid3} | {CustomerUid}  | {ProjectUid}  | {UserId} | {filterName3} | {(int)filterType} | {filterJson3}  | 0         | {ts.EventDate:yyyy-MM-dd} |"

      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid1);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid2);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterUid3);
      var responseGet = ts.CallFilterWebApi($"api/v1/filters/{ProjectUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorListResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      if (filterType == FilterType.Persistent)
      { 
        Assert.AreEqual(filterResponseGet.FilterDescriptors.Count, 3, "Expecting 3 filters in response");

        for (var cnt = 0; cnt < 3; cnt++)
        {
          switch (cnt)
          {
            case 0:
              Assert.AreEqual(filterResponseGet.FilterDescriptors[cnt].FilterJson, filterJson1,
                "JSON Filter doesn't match for GET request");
              break;
            case 1:
              Assert.AreEqual(filterResponseGet.FilterDescriptors[cnt].FilterJson, filterJson2,
                "JSON Filter doesn't match for GET request");
              break;
            case 2:
              Assert.AreEqual(filterResponseGet.FilterDescriptors[cnt].FilterJson, filterJson3,
                "JSON Filter doesn't match for GET request");
              break;
          }
        }
      }
      else
      {
        //Get filters only returns persistent
        Assert.AreEqual(filterResponseGet.FilterDescriptors.Count, 0, "Expecting 0 filters in response");
      }
    }




    /// <summary>
    /// Create the filter and convert it to json 
    /// </summary>
    /// <param name="elevation">ElevationType</param>
    /// <param name="vibestate">true or false</param>
    /// <param name="forward">true or false</param>
    /// <param name="layerNo">layer number</param>
    /// <returns>complete filter in json format</returns>
    private static string CreateTestFilter(ElevationType? elevation = null, bool? vibestate = null, bool? forward = null,
      int? layerNo = null)
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
      var filter = Filter.CreateFilter(startUtc, endUtc, Guid.NewGuid().ToString(), listMachines, 123,
                                        elevation, vibestate, listPoints, forward, layerNo);
      return filter.ToJsonString();
    }

    #endregion

    #region Boundaries
    [TestMethod]
    public void InsertBoundaryInDatabaseAndGetItFromWebApi()
    {
      const string boundaryName = "Boundary DbWeb test 1";
      Msg.Title(boundaryName, "Insert Boundary In Database And Get It From WebApi");
      var boundaryUid = Guid.NewGuid();
      ts.CustomerUid = CustomerUid;
      var boundaryWKT = GenerateWKTPolygon();
      var geofenceType = (int)GeofenceType.Filter;

      var eventsArray = new[] {
        "| TableName | GeofenceUID   | Name           | fk_GeofenceTypeID | GeometryWKT   | FillColor | IsTransparent | IsDeleted | Description | fk_CustomerUID | UserUID   | LastActionedUTC           |",
       $"| Geofence  | {boundaryUid} | {boundaryName} | {geofenceType}    | {boundaryWKT} | 0         | 0             | 0         |             | {CustomerUid}  | {UserId} | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, boundaryUid);

      var eventsArray2 = new[] {
        "| TableName        | fk_GeofenceUID  | fk_ProjectUID | LastActionedUTC           |",
       $"| ProjectGeofence  | {boundaryUid}   | {ProjectUid}  | {ts.EventDate:yyyy-MM-dd} |"
      };
      ts.PublishEventCollection(eventsArray2);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, boundaryUid);

      var responseGet = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}?boundaryUid={boundaryUid}", "GET");
      var boundaryResponseGet = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(boundaryResponseGet.GeofenceData.GeometryWKT, boundaryWKT, "Boundary WKT doesn't match for GET request");
      Assert.AreEqual(boundaryResponseGet.GeofenceData.GeofenceName, boundaryName, "Boundary name doesn't match for GET request");
    }


    #endregion
  }
}