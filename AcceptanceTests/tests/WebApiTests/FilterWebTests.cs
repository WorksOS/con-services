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

    [TestMethod]
    public void CreateSimpleFilter()
    {
      const string filterName = "Filter Web test 1";
      msg.Title(filterName, "Create filter with all the defaults in the filter json");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = customerUid
      };
      ts.DeleteAllFiltersForProject(projectUid.ToString());
      var filterJson = CreateTestFilter();
      
      var filterRequest = FilterRequest.Create("", filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponse.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for PUT request");
      Assert.AreEqual(filterResponse.filterDescriptor.Name,filterName, "Filter name doesn't match for PUT request");
      var filterUid = filterResponse.filterDescriptor.FilterUid;
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod, Ignore("(Aaron) I have no idea how this test ever passed. It doesn't set polygonName or polygonUID so must always fail Filter::Validate().")]
    public void CreateFilterWgsPointsValues()
    {
      const string filterName = "Filter Web test 2";
      msg.Title(filterName, "Create filter with WGSPoints and dates");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = customerUid
      };
      ts.DeleteAllFiltersForProject(projectUid.ToString());
      var listWgsPoints = new List<WGSPoint>
      {
        WGSPoint.CreatePoint(38.8361907402694, -121.349260032177),
        WGSPoint.CreatePoint(38.8361656688414, -121.349217116833),
        WGSPoint.CreatePoint(38.8387897637231, -121.347275197506),
        WGSPoint.CreatePoint(38.8387145521594, -121.347189366818)
      };
      var filterJson = CreateTestFilter(ElevationType.Last, null, null, 1, listWgsPoints,null,DateTime.Now.AddYears(-5).ToUniversalTime(), DateTime.Now.AddYears(-1).ToUniversalTime());
      var filterRequest = FilterRequest.Create("", filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filterUid = filterResponse.filterDescriptor.FilterUid;
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    [Ignore]
    public void CreateThenUpdateSomeValuesToNulls()
    {
      const string filterName = "Filter Web test 3";
      msg.Title(filterName, "Create and update filter with null feilds");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = customerUid
      };
      ts.DeleteAllFiltersForProject(projectUid.ToString());
      var listWgsPoints = new List<WGSPoint>
      {
        WGSPoint.CreatePoint(38.8361907402694, -121.349260032177),
        WGSPoint.CreatePoint(38.8361656688414, -121.349217116833),
        WGSPoint.CreatePoint(38.8387897637231, -121.347275197506),
        WGSPoint.CreatePoint(38.8387145521594, -121.347189366818)
      };
      var filterJson = CreateTestFilter(ElevationType.Last, null, null, 1, listWgsPoints, null, DateTime.Now.AddYears(-5).ToUniversalTime(), DateTime.Now.AddYears(-1).ToUniversalTime());
      var filterRequest = FilterRequest.Create("", filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filterUid = filterResponse.filterDescriptor.FilterUid;

      var filterJson2 = CreateTestFilter();
      var filterRequest2 = FilterRequest.Create(filterUid, filterName, filterJson2);
      var filter2 = JsonConvert.SerializeObject(filterRequest2, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter2);

      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson2, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod, Ignore("(Aaron) I have no idea how this test ever passed. It doesn't set polygonName or polygonUID so must always fail Filter::Validate().")]
    public void CreateFilterWithValidJsonString()
    {
      const string filterName = "Filter Web test 4";
      msg.Title(filterName, "Create filter with json string");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = customerUid
      };
      ts.DeleteAllFiltersForProject(projectUid.ToString());
      var filterRequest = FilterRequest.Create("", filterName, "");
      filterRequest.FilterJson = "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":123456789,\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}],\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true,\"polygonLL\":[{\"Lat\":38.8361907402694,\"Lon\":-121.349260032177},{\"Lat\":38.8361656688414,\"Lon\":-121.349217116833},{\"Lat\":38.8387897637231,\"Lon\":-121.347275197506},{\"Lat\":38.8387145521594,\"Lon\":-121.347189366818}],\"forwardDirection\":false,\"layerNumber\":null,\"layerType\":null}";
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponse.filterDescriptor.FilterJson, filterRequest.FilterJson, "JSON Filter doesn't match for PUT request");
      Assert.AreEqual(filterResponse.filterDescriptor.Name, filterRequest.Name, "Filter name doesn't match for PUT request");
      var filterUid = filterResponse.filterDescriptor.FilterUid;
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterRequest.FilterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterRequest.Name, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void CreateFilterWithInValidDesignId()
    {
      const string filterName = "Filter Web test 5";
      msg.Title(filterName, "Create filter with invalid DesignId");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = customerUid
      };
      ts.DeleteAllFiltersForProject(projectUid.ToString());
      var filterRequest = FilterRequest.Create("", filterName, "");
      filterRequest.FilterJson =
        "{\"startUTC\":null,\"designUid\":\"xxx\",\"contributingMachines\":[{\"assetID\":123456789,\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}],\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true,\"forwardDirection\":false,\"layerNumber\":null,\"layerType\":null}";
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponse.Message, "Invalid designUid.", "Expecting a Invalid designUid.");
    }


    [TestMethod]
    public void CreateThenDeleteFilter()
    {
      const string filterName = "Filter Web test 6";
      msg.Title(filterName, "Create then delete filter");
      var ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = customerUid
      };
      ts.DeleteAllFiltersForProject(projectUid.ToString());
      var filterJson = CreateTestFilter(ElevationType.Lowest, true, false);
      var filterRequest = FilterRequest.Create("", filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var filterUid = filterResponse.filterDescriptor.FilterUid;
      var responseDelete = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "DELETE");
      var responseDel = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseDelete, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(responseDel.Message, "success", " delete call to wep api expects success" );

      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var respGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(respGet.Message, "GetFilter By filterUid. The requested filter does exist, or does not belong to the requesting customer; project or user.",
                      "Expecting an error message to say the filter does not exist.");
    }


    /// <summary>
    /// Create the filter and convert it to json 
    /// </summary>
    /// <param name="elevation">ElevationType</param>
    /// <param name="vibestate">true or false</param>
    /// <param name="forward">true or false</param>
    /// <param name="layerNo">layer number</param>
    /// <param name="listWgsPoints"></param>
    /// <param name="onMachineDesignId"></param>
    /// <param name="startUtc"></param>
    /// <param name="endUtc"></param>
    /// <returns>complete filter in json format</returns>
    private string CreateTestFilter(ElevationType? elevation = null,bool? vibestate = null, bool? forward = null,
                                    int? layerNo = null, List<WGSPoint> listWgsPoints = null , int? onMachineDesignId = null,
                                    DateTime? startUtc = null, DateTime? endUtc = null)
    {
      var listMachines = new List<MachineDetails>();
      var machine = MachineDetails.CreateMachineDetails(123456789,"TheMachineName", false);
      listMachines.Add(machine);
      var filter = Filter.CreateFilter(startUtc, endUtc, Guid.NewGuid().ToString(), listMachines, onMachineDesignId,
                                       elevation, vibestate, listWgsPoints, forward, layerNo);
      return filter.ToJsonString();
    }
  }
}