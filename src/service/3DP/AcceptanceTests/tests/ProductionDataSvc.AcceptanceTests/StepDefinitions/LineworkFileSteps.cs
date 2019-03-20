using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("LineworkFile.feature")]
  public class LineworkFileSteps : FeaturePostRequestBase<JObject, GeoJson>
  {
    private string DxfUnits;
    private string MaxBoundariesToProcess;
    private string ConvertLineStringCoordsToPolygon;
    private Stream dxfFileStream;
    private Stream dcFileStream;

    private static string GetResource(string resourceName)
    {
      resourceName = $".TestData.LineworkFiles.{resourceName}";
      resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(n => n.Contains(resourceName));

      if (string.IsNullOrEmpty(resourceName))
      {
        throw new Exception($"Error attempting to find resource name {resourceName}.");
      }

      return resourceName;
    }

    [And(@"with property DxfUnits with value ""(.*)""")]
    public void AndWithPropertyDxfUnitsWithValue(string parameterValue)
    {
      DxfUnits = parameterValue;
    }

    [And(@"with property MaxBoundariesToProcess with value ""(.*)""")]
    public void AndWithPropertyMaxBoundariesToProcessWithValue(string parameterValue)
    {
      MaxBoundariesToProcess = parameterValue;
    }
    
    [And(@"with property ConvertLineStringCoordsToPolygon with value ""(.*)""")]
    public void AndWithPropertyConvertLineStringCoordsToPolygonWithValue(string parameterValue)
    {
      ConvertLineStringCoordsToPolygon = parameterValue;
    }

    [And(@"with property DxfFile with value ""(.*)""")]
    public void AndWithPropertyDxfFileWithValue(string parameterValue)
    {
      dxfFileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetResource(parameterValue));
    }

    [And(@"with property CoordinateSystemFile with value ""(.*)""")]
    public void AndWithPropertyCoordinateSystemFileWithValue(string parameterValue)
    {
      dcFileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetResource(parameterValue));
    }

    [When(@"I POST the multipart request I expect response code (\d+)")]
    public void WhenIPostWithParameterAndContentTypeIExpectResponseCode(int expectedHttpCode)
    {
      PostRequestHandler.HttpResponseMessage = Upload().Result;

      var receiveStream = PostRequestHandler.HttpResponseMessage.Content.ReadAsStreamAsync().Result;
      var readStream = new StreamReader(receiveStream, Encoding.UTF8);
      var responseBody = readStream.ReadToEnd();

      Assert.True(expectedHttpCode == (int)PostRequestHandler.HttpResponseMessage.StatusCode, responseBody);

      PostRequestHandler.CurrentResponse = JsonConvert.DeserializeObject<GeoJson>(responseBody, new JsonSerializerSettings
      {
        Formatting = Formatting.Indented
      });
    }

    private Task<HttpResponseMessage> Upload()
    {
      var httpClient = new HttpClient();

      httpClient.DefaultRequestHeaders.Add("X-VisionLink-CustomerUid", "87bdf851-44c5-e311-aa77-00505688274d");
      httpClient.DefaultRequestHeaders.Add("X-JWT-Assertion", "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6IjE0NTU1Nzc4MjM5MzAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoxMDc5LCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlV0aWxpemF0aW9uIERldmVsb3AgQ0kiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBpY29udGV4dCI6Ii90L3RyaW1ibGUuY29tL3V0aWxpemF0aW9uYWxwaGFlbmRwb2ludCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjEuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJDbGF5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9sYXN0bmFtZSI6IkFuZGVyc29uIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9vbmVUaW1lUGFzc3dvcmQiOm51bGwsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6IlN1YnNjcmliZXIscHVibGlzaGVyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91dWlkIjoiMjM4ODY5YWYtY2E1Yy00NWUyLWI0ZjgtNzUwNjE1YzhhOGFiIn0=.kTaMf1IY83fPHqUHTtVHn6m6aQ9wFch6c0FsNDQ7x1k=");
      httpClient.DefaultRequestHeaders.Add("X-VisionLink-ClearCache", "true");
      httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");

      var formContent = new MultipartFormDataContent("C6A88977");

      formContent.Headers.ContentType.MediaType = "multipart/form-data";

      if (int.Parse(DxfUnits) >= 0) { formContent.Add(new StringContent(DxfUnits), "DxfUnits"); }
      formContent.Add(new StringContent(MaxBoundariesToProcess), "MaxBoundariesToProcess");
      if (!string.IsNullOrEmpty(ConvertLineStringCoordsToPolygon)) { formContent.Add(new StringContent(ConvertLineStringCoordsToPolygon), "ConvertLineStringCoordsToPolygon"); }
      formContent.Add(new StreamContent(dxfFileStream), "DxfFile", "dxfFile.dxf");
      formContent.Add(new StreamContent(dcFileStream), "CoordinateSystemFile", "coordinateSystemFile.dc");

      return httpClient.PostAsync(PostRequestHandler.Uri, formContent);
    }
  }
}
