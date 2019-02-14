using Gherkin.Ast;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using ProductionDataSvc.AcceptanceTests.Utils;
using Xunit;
using Xunit.Gherkin.Quick;
using Feature = Xunit.Gherkin.Quick.Feature;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  public abstract class FeaturePostRequestBase<TRequest, TResponse> : Feature where TRequest : JObject, new() where TResponse : ResponseBase
  {
    protected Poster<TRequest, TResponse> PostRequestHandler;

    [Given(@"the service URI ""(.*)""")]
    public void GivenTheServiceUri(string uri)
    {
      uri = RestClient.Productivity3DServiceBaseUrl + uri;
      PostRequestHandler = new Poster<TRequest, TResponse>(uri);
    }

    [Given(@"the service route ""(.*)"" and request repo ""(.*)""")]
    public void GivenTheServiceRouteAndRequestRepo(string route, string requestFile)
    {
      PostRequestHandler = new Poster<TRequest, TResponse>(RestClient.Productivity3DServiceBaseUrl + route, requestFile);
    }

    [Given(@"the service route ""(.*)"" request repo ""(.*)"" and result repo ""(.*)""")]
    public void GivenTheServiceRouteRequestRepoAndResultRepo(string route, string requestFile, string resultFile)
    {
      PostRequestHandler = new Poster<TRequest, TResponse>(RestClient.Productivity3DServiceBaseUrl + route, requestFile, resultFile);
    }

    [And(@"request body property ""(.*)"" with value ""(.*)""")]
    public void AndRequestBodyPropertyWithValue(string propertyName, string propertyValue)
    {
      TrySetProperty(PostRequestHandler.CurrentRequest, propertyName, propertyValue);
    }

    [When(@"I POST with no parameters I expect response code (\d+)")]
    public void WhenIPostWithNoParametersIExpectResponseCode(int httpCode)
    {
      PostRequestHandler.DoRequest(null, expectedHttpCode: httpCode);
    }

    [When(@"I POST with parameter ""(.*)"" I expect response code (\d+)")]
    public void WhenIPostWithParameterIExpectResponseCode(string parameterValue, int httpCode)
    {
      PostRequestHandler.DoRequest(parameterValue, expectedHttpCode: httpCode);
    }

    [When(@"I POST Content-Type ""(.*)"" with parameter ""(.*)"" I expect response code (\d+)")]
    public void WhenIPostWithParameterAndContentTypeIExpectResponseCode(string contentType, string parameterValue, int httpCode)
    {
      PostRequestHandler.DoRequest(parameterValue, contentType, httpCode);
    }

    [Then(@"the response should match ""(.*)"" from the repository")]
    public void ThenTheProductionDataCellDatumResponseShouldMatchResultFromTheRepository(string resultName)
    {
      var expectedResultJson = JsonConvert.SerializeObject(PostRequestHandler.ResponseRepo[resultName], Formatting.Indented);
      var actualResultJson = JsonConvert.SerializeObject(PostRequestHandler.CurrentResponse, Formatting.Indented);

      ObjectComparer.AssertAreEqual(
        actualResultJson: actualResultJson,
        expectedResultJson: expectedResultJson,
        ignoreCase: true,
        resultName: resultName);
    }

    [Then(@"the response should contain code ""(.*)""")]
    public void ThenTheResponseShouldContainCode(int expectedCode)
    {
      Assert.Equal(expectedCode, PostRequestHandler.CurrentResponse.Code);
    }

    [Then(@"the response should contain the message ""(.*)""")]
    public void ThenTheResultShouldContainTheMessage(string expectedMessage)
    {
      Assert.Equal(expectedMessage, PostRequestHandler.CurrentResponse.Message);
    }

    [Then(@"the response should contain message ""(.*)"" and code ""(.*)""")]
    public void ThenTheResultShouldContainMessageAndCode(string expectedMessage, int expectedCode)
    {
      Assert.Equal(expectedCode, PostRequestHandler.CurrentResponse.Code);
      Assert.Equal(expectedMessage, PostRequestHandler.CurrentResponse.Message);
    }

    [Then(@"the response should be:")]
    public void ThenTheResponseShouldBe(DocString json)
    {
      // We deliberately serialize to formatted JSON because if the test fails it's easier to see the actual vs expected which comparing JObjects doesn't give us.
      var expectedJObject = JsonConvert.DeserializeObject<JObject>(json.Content);
      var actualJObject = JObject.FromObject(PostRequestHandler.CurrentResponse);

      ObjectComparer.RoundAllDoubleProperties(actualJObject, roundingPrecision: 8);
      ObjectComparer.RoundAllDoubleProperties(expectedJObject, roundingPrecision: 8);

      ObjectComparer.AssertAreEqual(actualResultObj: actualJObject, expectedResultObj: expectedJObject);
    }
    
    /// <summary>
    /// Adds and sets a new property on the JObject receiver.
    /// </summary>
    private static void TrySetProperty(JObject obj, string property, string value)
    {
      // All inputs are strings despite their type, because their origin is a .feature file.
      obj[property] = value;
    }

    protected void AssertObjectsAreEqual<T>(T expectedResult)
    {
      var actualResultJson = JsonConvert.SerializeObject(PostRequestHandler.CurrentResponse);
      var expectedResultJson = JsonConvert.SerializeObject(expectedResult);

      Assert.True(JToken.DeepEquals(expectedResultJson, actualResultJson));
    }
  }
}
