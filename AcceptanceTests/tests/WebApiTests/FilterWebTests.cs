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
    private readonly string userId = new Guid("98cdb619-b06b-4084-b7c5-5dcccc82af3b").ToString();

    [TestMethod]
    public void CreateFilterToWebApiAndGetItFromWebApi()
    {
      const string filterName = "Filter Web test 1";
      msg.Title(filterName, "Create filter then request it from WebApi");
      var ts = new TestSupport { IsPublishToWebApi = true};
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString());
      
      var filterRequest = FilterRequest.CreateFilterRequest(filterName, filterJson);
     // filterRequest.filterUid = filterUid.ToString();
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter);
      var filterResponse = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponse.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for PUT request");
      Assert.AreEqual(filterResponse.filterDescriptor,filterName, "Filter name doesn't match for PUT request");
      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
    }

    [TestMethod]
    public void CreateTheUpdateFilterToWebApiAndGetItFromWebApi()
    {
      const string filterName = "Filter Web test 2";
      msg.Title(filterName, "Create and update filter then request it from WebApi");
      var ts = new TestSupport { IsPublishToWebApi = true };
      var filterUid = Guid.NewGuid();
      ts.CustomerUid = customerUid;
      var filterJson = CreateTestFilter(filterUid.ToString());     
      var filterRequest = FilterRequest.CreateFilterRequest(filterName, filterJson);
      var filter = JsonConvert.SerializeObject(filterRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter);

      var filterJson2 = CreateTestFilter(filterUid.ToString(),ElevationType.Lowest,true,false);
      var filterRequest2 = FilterRequest.CreateFilterRequest(filterName, filterJson2);
      filterRequest2.filterUid = filterUid.ToString();
      var filter2 = JsonConvert.SerializeObject(filterRequest2, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate2 = ts.CallFilterWebApi($"api/v1/filter/{projectUid}", "PUT", filter2);

      var responseGet = ts.CallFilterWebApi($"api/v1/filter/{projectUid}?filterUid={filterUid}", "GET");
      var filterResponseGet = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(filterResponseGet.filterDescriptor.FilterJson, filterJson2, "JSON Filter doesn't match for GET request");
      Assert.AreEqual(filterResponseGet.filterDescriptor.Name, filterName, "Filter name doesn't match for GET request");
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
      var filter = Filter.CreateFilter(startUtc, endUtc, Guid.NewGuid().ToString(), listMachines, 123,
                                       elevation, vibestate, listPoints, forward, layerNo, fltlayer);
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
