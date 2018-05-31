using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests
{
  [TestClass]
  public class FilterWebTests : WebTestBase
  {
    private TestSupport ts;

    [TestInitialize]
    public void Initialize()
    {
      ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = CustomerUid
      };
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void CreateSimpleFilter(FilterType filterType)
    {
      ts.DeleteAllFiltersForProject(ProjectUid.ToString());

      const string filterName = "Filter Web test 1";
      Msg.Title(filterName, $"Create {filterType } filter with all the defaults in the filter json");

      var filterJson = CreateTestFilter();

      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson, filterType);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponse.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for PUT request");
      Assert.AreEqual(filterResponse.FilterDescriptor.Name, filterName, "Filter name doesn't match for PUT request");
      Assert.AreEqual(filterResponse.FilterDescriptor.FilterType, filterType, "Filter type doesn't match for PUT request");
      var filterUid = filterResponse.FilterDescriptor.FilterUid;
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
    public void CreateFilterThenGetListOfFilters(FilterType filterType)
    {
      ts.DeleteAllFiltersForProject(ProjectUid.ToString());

      const string filterName = "Filter Web test 2";
      Msg.Title(filterName, $"Create {filterType } filter then get a list of filters");

      var filterJson = CreateTestFilter(ElevationType.Last, null, null, 1, null, DateTime.Now.AddYears(-5).ToUniversalTime(), DateTime.Now.AddYears(-1).ToUniversalTime());
      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson, filterType);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filterUid = filterResponse.FilterDescriptor.FilterUid;

      var responseGet = ts.CallFilterWebApi($"api/v1/filters/{ProjectUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorListResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      if (filterType == FilterType.Persistent)
      {
        Assert.AreEqual(filterJson, filterResponseGet.FilterDescriptors[0].FilterJson,
          "JSON Filter doesn't match for GET request");
      }
      else
      {
        Assert.AreEqual(0, filterResponseGet.FilterDescriptors.Count, "Should not return transient or report filters in list");
      }
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void CreateThenUpdateFilter(FilterType filterType)
    {
      //Note: cannot update a transient filter - must create a new one

      ts.DeleteAllFiltersForProject(ProjectUid.ToString());

      const string filterName = "Filter Web test 3";
      Msg.Title(filterName, $"Create and update {filterType } filter");

      var filterJson = CreateTestFilter(ElevationType.Last, null, null, 1, null, DateTime.Now.AddYears(-5).ToUniversalTime(), DateTime.Now.AddYears(-1).ToUniversalTime());
      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson, filterType);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filterUid = filterResponse.FilterDescriptor.FilterUid;

      var filterJson2 = CreateTestFilter();
      var filterName2 = "Updated filter";
      var filterRequest2 = FilterRequest.Create(filterUid, filterName2, filterJson2, filterType);
      var filter2 = JsonConvert.SerializeObject(filterRequest2, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}", "PUT", filter2);

      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      //Can only update name, not JSON, therefore name should be second one but JSON the original
      Assert.AreEqual(filterJson, filterResponseGet.FilterDescriptor.FilterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterName2, filterResponseGet.FilterDescriptor.Name, "Filter name doesn't match for GET request");
      Assert.AreEqual(filterType, filterResponseGet.FilterDescriptor.FilterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void CreateFilterWithBoundary(FilterType filterType)
    {
      ts.DeleteAllBoundariesAndAssociations();

      const string filterName = "Filter Web test 4";
      Msg.Title(filterName, $"Create {filterType } filter with boundary in json string");

      var boundaryWkt = GenerateWKTPolygon();
      var boundaryName = filterName + " - boundary";
      var boundaryRequest = BoundaryRequest.Create(string.Empty, boundaryName, boundaryWkt);
      var boundary = JsonConvert.SerializeObject(boundaryRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}", "PUT", boundary);
      var boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var boundaryUid = boundaryResponse.GeofenceData.GeofenceUID;

      var filterJson = CreateTestFilter(polygonUid: boundaryUid.ToString());
      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson, filterType);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      responseCreate = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var hydratedFilterJson = CreateTestFilter(polygonUid: boundaryUid.ToString(), polygonName: boundaryName, polygonPoints: GetPointsFromWkt(boundaryWkt));
      Assert.AreEqual(filterRequest.Name, filterResponse.FilterDescriptor.Name, "Filter name doesn't match for PUT request");
      Assert.AreEqual(hydratedFilterJson, filterResponse.FilterDescriptor.FilterJson, "JSON Filter doesn't match for PUT request");
      var filterUid = filterResponse.FilterDescriptor.FilterUid;
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterRequest.Name, filterResponseGet.FilterDescriptor.Name, "Filter name doesn't match for GET request");
      Assert.AreEqual(hydratedFilterJson, filterResponseGet.FilterDescriptor.FilterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterRequest.FilterType, filterResponseGet.FilterDescriptor.FilterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void CreateFilterWithBoundaryThenDeleteBoundary(FilterType filterType)
    {
      ts.DeleteAllBoundariesAndAssociations();

      const string filterName = "Filter Web test 5";
      Msg.Title(filterName, $"Create {filterType} filter with boundary and delete boundary");

      var boundaryWkt = GenerateWKTPolygon();
      var boundaryName = filterName + " - boundary";
      var boundaryRequest = BoundaryRequest.Create(string.Empty, boundaryName, boundaryWkt);
      var boundary = JsonConvert.SerializeObject(boundaryRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}", "PUT", boundary);
      var boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var boundaryUid = boundaryResponse.GeofenceData.GeofenceUID;

      var filterJson = CreateTestFilter(polygonUid: boundaryUid.ToString());
      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson, filterType);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      responseCreate = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filterUid = filterResponse.FilterDescriptor.FilterUid;
      var hydratedFilterJson = CreateTestFilter(polygonUid: boundaryUid.ToString(), polygonName: boundaryName,
        polygonPoints: GetPointsFromWkt(boundaryWkt));

      var responseDelete = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}?boundaryUid={boundaryUid}", "DELETE");
      var filterResponse1 = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseDelete, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(0, filterResponse1.Code, "  Expecting a sucessful result but got " + filterResponse1.Message);

      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterRequest.Name, filterResponseGet.FilterDescriptor.Name, "Filter name doesn't match for GET request");
      Assert.AreEqual(hydratedFilterJson, filterResponseGet.FilterDescriptor.FilterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterRequest.FilterType, filterResponseGet.FilterDescriptor.FilterType, "Filter type doesn't match for GET request");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void CreateFilterWithInvalidDesignId(FilterType filterType)
    {
      const string filterName = "Filter Web test 6";
      Msg.Title(filterName, $"Create {filterType } filter with invalid DesignId");

      var filterRequest = FilterRequest.Create(string.Empty, filterName, string.Empty, filterType);
      filterRequest.FilterJson =
        "{\"startUTC\":null,\"designUid\":\"xxx\",\"contributingMachines\":[{\"assetID\":123456789,\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}],\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true,\"forwardDirection\":false,\"layerNumber\":null,\"layerType\":null}";
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponse.Message, "Invalid designUid.", "Expecting an Invalid designUid.");
    }


    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public void CreateThenDeleteFilter(FilterType filterType)
    {
      const string filterName = "Filter Web test 7";
      Msg.Title(filterName, "Create then delete {filterType } filter");

      var filterJson = CreateTestFilter(ElevationType.Lowest, true, false);
      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson, filterType);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filterUid = filterResponse.FilterDescriptor.FilterUid;
      var responseDelete = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "DELETE");
      var responseDel = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseDelete, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(responseDel.Message, "success", " delete call to wep api expects success");

      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filterUid}", "GET");
      var respGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(respGet.Message, "GetFilter By filterUid. The requested filter does not exist, or does not belong to the requesting customer; project or user.",
                      "Expecting an error message to say the filter does not exist.");
    }

    [TestMethod]
    public void PostMultipleFiltersThenGetFilters()
    {
      const string filterName = "Filter Web test 8";
      Msg.Title(filterName, "Post filters then get a list of filters");

      var filterJson1 = CreateTestFilter(ElevationType.Last, null, null, 1, null, DateTime.Now.AddYears(-5).ToUniversalTime(), DateTime.Now.AddYears(-1).ToUniversalTime());
      var filterRequest1 = FilterRequest.Create(string.Empty, filterName + "- one", filterJson1, FilterType.Transient);
      var filterJson2 = CreateTestFilter(ElevationType.First, true, true, 3, null, DateTime.Now.AddYears(-5).ToUniversalTime(), DateTime.Now.AddYears(-1).ToUniversalTime());
      var filterRequest2 = FilterRequest.Create(string.Empty, filterName + "- two", filterJson2, FilterType.Transient);
      var filterListRequest = new TestUtility.Model.WebApi.FilterListRequest();
      filterListRequest.FilterRequests.Add(filterRequest1);
      filterListRequest.FilterRequests.Add(filterRequest2);
      var filter = JsonConvert.SerializeObject(filterListRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallFilterWebApi($"api/v1/filters/{ProjectUid}", "POST", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorListResult>(response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(0, filterResponse.Code, "  Expecting a sucessful result but got " + filterResponse.Message);
      var filter1UID = filterResponse.FilterDescriptors[0].FilterUid;
      var filter2UID = filterResponse.FilterDescriptors[1].FilterUid;

      var filter1Response = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filter1UID}", "GET");
      var filter2Response = ts.CallFilterWebApi($"api/v1/filter/{ProjectUid}?filterUid={filter2UID}", "GET");

      var filter1 = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(filter1Response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filter2 = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(filter2Response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

      Assert.AreEqual(filterJson1, filter1.FilterDescriptor.FilterJson);
      Assert.AreEqual(filterJson2, filter2.FilterDescriptor.FilterJson);

    }

    /// <summary>
    /// Create the filter and convert it to json 
    /// </summary>
    /// <param name="elevation">ElevationType</param>
    /// <param name="vibestate">true or false</param>
    /// <param name="forward">true or false</param>
    /// <param name="layerNo">layer number</param>
    /// <param name="onMachineDesignId"></param>
    /// <param name="startUtc"></param>
    /// <param name="endUtc"></param>
    /// <param name="polygonUid"></param>
    /// <param name="polygonName"></param>
    /// <param name="polygonPoints"></param>
    /// <returns>complete filter in json format</returns>
    private static string CreateTestFilter(ElevationType? elevation = null, bool? vibestate = null, bool? forward = null,
                                    int? layerNo = null, int? onMachineDesignId = null, DateTime? startUtc = null,
                                    DateTime? endUtc = null, string polygonUid = null, string polygonName = null,
                                    List<WGSPoint> polygonPoints = null)
    {
      var listMachines = new List<MachineDetails>();
      var machine = MachineDetails.Create(123456789, "TheMachineName", false);
      listMachines.Add(machine);
      var filter = Filter.CreateFilter(startUtc, endUtc, null, listMachines, onMachineDesignId,
                                       elevation, vibestate, polygonPoints, forward, layerNo, polygonUid, polygonName);
      return filter.ToJsonString();
    }
  }
}