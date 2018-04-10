using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using TestUtility.Model.WebApi;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace WebApiTests
{
  [TestClass]

  public class BoundaryWebTests : WebTestBase
  {
    private TestSupport ts;
    private string boundaryWKT;

    [TestInitialize]
    public void Initialize()
    {
      ts = new TestSupport
      {
        IsPublishToWebApi = true,
        CustomerUid = CustomerUid
      };

      ts.DeleteAllBoundariesAndAssociations();

      boundaryWKT = GenerateWKTPolygon();
    }

    [TestMethod]
    public void CreateBoundary_DuplicateNameNotValid()
    {
      const string boundaryName = "Boundary Web test 1";
      this.Msg.Title(boundaryName, "Create boundary with duplicate name");

      var boundaryRequest = BoundaryRequest.Create(string.Empty, boundaryName, boundaryWKT);
      var boundary = JsonConvert.SerializeObject(boundaryRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}", "PUT", boundary);
      var boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual("success", boundaryResponse.Message, "Expecting success");
      //Now duplicate
      responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}", "PUT", boundary);
      boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual("Duplicate boundary name", boundaryResponse.Message, "Expecting duplicate boundary name");
      Assert.AreEqual(2062, boundaryResponse.Code, "Wrong error code");
    }

    [TestMethod]
    public void CreateBoundary_SameNameDifferentProject()
    {
      const string boundaryName = "Boundary Web test 2";
      this.Msg.Title(boundaryName, "Create boundary with same name in different project");

      var boundaryRequest = BoundaryRequest.Create(string.Empty, boundaryName, boundaryWKT);
      var boundary = JsonConvert.SerializeObject(boundaryRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}", "PUT", boundary);
      var boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual("success", boundaryResponse.Message, "Expecting success (1)");
      //Now create in another project
      responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid2}", "PUT", boundary);
      boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual("success", boundaryResponse.Message, "Expecting success (2)");
    }

    [TestMethod]
    public void CreateBoundary_HappyPath()
    {
      const string boundaryName = "Boundary Web test 3";
      this.Msg.Title(boundaryName, "Create boundary");

      var boundaryRequest = BoundaryRequest.Create(string.Empty, boundaryName, boundaryWKT);
      var boundary = JsonConvert.SerializeObject(boundaryRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}", "PUT", boundary);
      var boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(boundaryWKT, boundaryResponse.GeofenceData.GeometryWKT, "Boundary WKT doesn't match for PUT request");
      Assert.AreEqual(boundaryName, boundaryResponse.GeofenceData.GeofenceName, "Boundary name doesn't match for PUT request");
      var boundaryUid = boundaryResponse.GeofenceData.GeofenceUID;
      var responseGet = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}?boundaryUid={boundaryUid}", "GET");
      var boundaryResponseGet = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(boundaryWKT, boundaryResponseGet.GeofenceData.GeometryWKT, "Boundary WKT doesn't match for GET request");
      Assert.AreEqual(boundaryName, boundaryResponseGet.GeofenceData.GeofenceName, "Boundary name doesn't match for GET request");
    }

    [TestMethod]
    public void DeleteBoundary_HappyPath()
    {
      const string boundaryName = "Boundary Web test 4";
      this.Msg.Title(boundaryName, "Create then delete boundary");

      var boundaryRequest = BoundaryRequest.Create(string.Empty, boundaryName, boundaryWKT);
      var boundary = JsonConvert.SerializeObject(boundaryRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}", "PUT", boundary);
      var boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(response, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var boundaryUid = boundaryResponse.GeofenceData.GeofenceUID;
      var responseDelete = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}?boundaryUid={boundaryUid}", "DELETE");
      var responseDel = JsonConvert.DeserializeObject<ContractExecutionResult>(responseDelete, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual("success", responseDel.Message, "delete call to wep api expects success");

      var responseGet = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}?boundaryUid={boundaryUid}", "GET");
      var respGet = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual("GetBoundary By BoundaryUid. The requested Boundary does not exist, or does not belong to the requesting project or filter.", respGet.Message,
        "Expecting an error message to say the boundary does not exist.");
    }

    [TestMethod]
    public void GetBoundaries()
    {
      ts.DeleteAllBoundariesAndAssociations();

      const string boundaryName = "Boundary Web test 5";
      this.Msg.Title(boundaryName, "Get boundaries");

      var boundaryRequest = BoundaryRequest.Create(string.Empty, boundaryName + ".1", GenerateWKTPolygon());
      var boundary = JsonConvert.SerializeObject(boundaryRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}", "PUT", boundary);
      var boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var boundaryUid1 = boundaryResponse.GeofenceData.GeofenceUID;

      boundaryRequest = BoundaryRequest.Create(string.Empty, boundaryName + ".2", GenerateWKTPolygon());
      boundary = JsonConvert.SerializeObject(boundaryRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      responseCreate = ts.CallFilterWebApi($"api/v1/boundary/{ProjectUid}", "PUT", boundary);
      boundaryResponse = JsonConvert.DeserializeObject<GeofenceDataSingleResult>(responseCreate, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var boundaryUid2 = boundaryResponse.GeofenceData.GeofenceUID;

      var responseGet = ts.CallFilterWebApi($"api/v1/boundaries/{ProjectUid}", "GET");
      var boundaryResponseGet = JsonConvert.DeserializeObject<GeofenceDataListResult>(responseGet, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      Assert.AreEqual(2, boundaryResponseGet.GeofenceData.Count);
      Assert.IsNotNull(boundaryResponseGet.GeofenceData.Single(g => g.GeofenceUID == boundaryUid1), "Missing boundary 1");
      Assert.IsNotNull(boundaryResponseGet.GeofenceData.Single(g => g.GeofenceUID == boundaryUid2), "Missing boundary 2");
    }
  }
}