using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace WebApiTests
{
  [TestClass]
  public class FilterWebTests : WebTestBase
  {
    [TestMethod]
    public void CreateSimpleFilter()
    {
      const string filterName = "Filter Web test 1";
      this.Msg.Title(filterName, "Create filter with all the defaults in the filter json");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = this.CustomerUid
      };
      ts.DeleteAllFiltersForProject(this.ProjectUid.ToString());
      var filterJson = CreateTestFilter();

      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponse.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for PUT request");
      Assert.AreEqual(filterResponse.FilterDescriptor.Name, filterName, "Filter name doesn't match for PUT request");
      var filterUid = filterResponse.FilterDescriptor.FilterUid;
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.FilterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.FilterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void CreateThenUpdateFilter()
    {
      const string filterName = "Filter Web test 3";
      this.Msg.Title(filterName, "Create and update filter");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = this.CustomerUid
      };
      ts.DeleteAllFiltersForProject(this.ProjectUid.ToString());
      var filterJson = CreateTestFilter(ElevationType.Last, null, null, 1, null, DateTime.Now.AddYears(-5).ToUniversalTime(), DateTime.Now.AddYears(-1).ToUniversalTime());
      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filterUid = filterResponse.FilterDescriptor.FilterUid;

      var filterJson2 = CreateTestFilter();
      var filterName2 = "Updated filter";
      var filterRequest2 = FilterRequest.Create(filterUid, filterName2, filterJson2);
      var filter2 = JsonConvert.SerializeObject(filterRequest2, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}", "PUT", filter2);

      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      //Can only update name, not JSON, therefore name should be second one but JSON the original
      Assert.AreEqual(filterJson, filterResponseGet.FilterDescriptor.FilterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterName2, filterResponseGet.FilterDescriptor.Name, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void CreateFilterWithBoundary()
    {
      const string filterName = "Filter Web test 4";
      this.Msg.Title(filterName, "Create filter with boundary in json string");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = this.CustomerUid
      };

      ts.DeleteAllBoundariesAndAssociations();
      var boundaryWKT = GenerateWKTPolygon();
      var boundaryName = filterName + " - boundary";
      var boundaryRequest = BoundaryRequest.Create(string.Empty, boundaryName, boundaryWKT);
      var boundary = JsonConvert.SerializeObject(boundaryRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{this.ProjectUid}", "PUT", boundary);
      var boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var boundaryUid = boundaryResponse.GeofenceData.GeofenceUID;

      ts.DeleteAllFiltersForProject(this.ProjectUid.ToString());
      var filterJson = CreateTestFilter(polygonUid:boundaryUid.ToString());
      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      responseCreate = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var hydratedFilterJson = CreateTestFilter(polygonUid: boundaryUid.ToString(), polygonName: boundaryName,
        polygonPoints: GetPointsFromWkt(boundaryWKT));
      Assert.AreEqual(filterRequest.Name, filterResponse.FilterDescriptor.Name, "Filter name doesn't match for PUT request");
      Assert.AreEqual(hydratedFilterJson, filterResponse.FilterDescriptor.FilterJson, "JSON Filter doesn't match for PUT request");
      var filterUid = filterResponse.FilterDescriptor.FilterUid;
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterRequest.Name, filterResponseGet.FilterDescriptor.Name, "Filter name doesn't match for GET request");
      Assert.AreEqual(hydratedFilterJson, filterResponseGet.FilterDescriptor.FilterJson, "JSON Filter doesn't match for GET request");
    }

    [TestMethod]
    public void CreateFilterWithBoundaryThenDeleteBoundary()
    {
      const string filterName = "Filter Web test 5";
      this.Msg.Title(filterName, "Create filter with boundary and delete boundary");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = this.CustomerUid
      };

      ts.DeleteAllBoundariesAndAssociations();
      var boundaryWKT = GenerateWKTPolygon();
      var boundaryName = filterName + " - boundary";
      var boundaryRequest = BoundaryRequest.Create(string.Empty, boundaryName, boundaryWKT);
      var boundary = JsonConvert.SerializeObject(boundaryRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{this.ProjectUid}", "PUT", boundary);
      var boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var boundaryUid = boundaryResponse.GeofenceData.GeofenceUID;

      ts.DeleteAllFiltersForProject(this.ProjectUid.ToString());
      var filterJson = CreateTestFilter(polygonUid: boundaryUid.ToString());
      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      responseCreate = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filterUid = filterResponse.FilterDescriptor.FilterUid;
      var hydratedFilterJson = CreateTestFilter(polygonUid: boundaryUid.ToString(), polygonName: boundaryName,
        polygonPoints: GetPointsFromWkt(boundaryWKT));

      var responseDelete = ts.CallFilterWebApi($"api/v1/boundary/{this.ProjectUid}?boundaryUid={boundaryUid}", "DELETE");

      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterRequest.Name, filterResponseGet.FilterDescriptor.Name, "Filter name doesn't match for GET request");
      Assert.AreEqual(hydratedFilterJson, filterResponseGet.FilterDescriptor.FilterJson, "JSON Filter doesn't match for GET request");
    }

    [TestMethod]
    public void CreateFilterWithInValidDesignId()
    {
      const string filterName = "Filter Web test 6";
      this.Msg.Title(filterName, "Create filter with invalid DesignId");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = this.CustomerUid
      };
      ts.DeleteAllFiltersForProject(this.ProjectUid.ToString());
      var filterRequest = FilterRequest.Create(string.Empty, filterName, string.Empty);
      filterRequest.FilterJson =
        "{\"startUTC\":null,\"designUid\":\"xxx\",\"contributingMachines\":[{\"assetID\":123456789,\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}],\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true,\"forwardDirection\":false,\"layerNumber\":null,\"layerType\":null}";
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponse.Message, "Invalid designUid.", "Expecting a Invalid designUid.");
    }


    [TestMethod]
    public void CreateThenDeleteFilter()
    {
      const string filterName = "Filter Web test 7";
      this.Msg.Title(filterName, "Create then delete filter");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = this.CustomerUid
      };
      ts.DeleteAllFiltersForProject(this.ProjectUid.ToString());
      var filterJson = CreateTestFilter(ElevationType.Lowest, true, false);
      var filterRequest = FilterRequest.Create(string.Empty, filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filterUid = filterResponse.FilterDescriptor.FilterUid;
      var responseDelete = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}?filterUid={filterUid}", "DELETE");
      var responseDel = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseDelete, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(responseDel.Message, "success", " delete call to wep api expects success");

      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{this.ProjectUid}?filterUid={filterUid}", "GET");
      var respGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(respGet.Message, "GetFilter By filterUid. The requested filter does not exist, or does not belong to the requesting customer; project or user.",
                      "Expecting an error message to say the filter does not exist.");
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
    private string CreateTestFilter(ElevationType? elevation = null, bool? vibestate = null, bool? forward = null,
                                    int? layerNo = null, int? onMachineDesignId = null, DateTime? startUtc = null, 
                                    DateTime? endUtc = null,string polygonUid = null, string polygonName=null, 
                                    List<WGSPoint> polygonPoints=null)
    {
      var listMachines = new List<MachineDetails>();
      var machine = MachineDetails.CreateMachineDetails(123456789, "TheMachineName", false);
      listMachines.Add(machine);
      var filter = Filter.CreateFilter(startUtc, endUtc, null, listMachines, onMachineDesignId,
                                       elevation, vibestate, polygonPoints, forward, layerNo, polygonUid, polygonName);
      return filter.ToJsonString();
    }
  }
}
