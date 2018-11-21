using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ProductionDataSvc.AcceptanceTests.Models;
using ProductionDataSvc.AcceptanceTests.Utils;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("SurveyedSurface.feature")]
  public class SurveyedSurfaceSteps : Feature
  {
    private Getter<GetSurveydSurfacesResult> surfacesValidator;
    private GetSurveydSurfacesResult result;
    private DummyRequestResult resultPost;

    private Getter<DummyRequestResult> surfacesDeleteValidator;
    private Poster<SurveyedSurfaceRequest, DummyRequestResult> surfaceToPost;
    private DummyRequestResult resultDelete;
    private string uriProd;
    private string postAddress;

    [Given(@"the Surveyd surface service URI ""(.*)""")]
    public void GivenTheSurveydSurfaceServiceURI(string uri)
    {
      this.uriProd = uri = RestClient.Productivity3DServiceBaseUrl + uri;
      this.surfacesValidator = new Getter<GetSurveydSurfacesResult>(uri);
      surfacesDeleteValidator = new Getter<DummyRequestResult>(uri + "/1234/delete");
    }

    [And(@"using repository ""(.*)""")]
    public void GivenUsingRepository(string p0)
    {
      surfaceToPost = new Poster<SurveyedSurfaceRequest, DummyRequestResult>(postAddress, p0, p0);
    }

    [And(@"the Surveyd surface service POST URI ""(.*)""")]
    public void GivenTheSurveydSurfaceServicePOSTURI(string p0)
    {
      postAddress = RestClient.Productivity3DServiceBaseUrl + p0;
      surfaceToPost.Uri = RestClient.Productivity3DServiceBaseUrl + p0;
    }

    [And(@"a project Id (.*)")]
    public void GivenAProjectId(int projectId)
    {
      surfacesValidator.Uri = string.Format(surfacesValidator.Uri, projectId);
      surfacesDeleteValidator.Uri = string.Format(surfacesDeleteValidator.Uri, projectId);
    }

    [When(@"I request surveyd SurveyedSurfaces")]
    public void WhenIRequestSurveydSurfaces()
    {
      result = surfacesValidator.SendRequest();
    }

    [When(@"I delete surveyd SurveyedSurfaces")]
    public void WhenIDeleteSurveydSurfaces()
    {
      resultDelete = surfacesDeleteValidator.SendRequest();
    }

    [Then(@"the following machine designs should be returned:")]
    public void ThenTheFollowingMachineDesignsShouldBeReturned(Gherkin.Ast.DataTable dataTable)
    {
      var expectedResult = new GetSurveydSurfacesResult();
      var expectedSurfaces = new List<SurveyedSurfaces>();

      foreach (var row in dataTable.Rows.Skip(1))
      {
        expectedSurfaces.Add(new SurveyedSurfaces{
          AsAtDate = DateTime.Parse(row.Cells.ElementAt(0).Value),
          Id = long.Parse(row.Cells.ElementAt(1).Value),
          SurveyedSurface = new DesignDescriptor { file = new FileDescriptor { fileName = row.Cells.ElementAt(2).Value, filespaceId = row.Cells.ElementAt(3).Value } }
        });
      }

      expectedResult.SurveyedSurfaces = expectedSurfaces;

      Assert.Equal(expectedResult, result);
    }

    [When(@"I post surveyd surface")]
    public void WhenIPostSurveydSurface()
    {
      surfaceToPost.DoRequest("PostStandardFile");
    }

    [When(@"I request Surveyed Surface expecting Bad Request")]
    public void WhenIRequestSurveyedSurfaceExpectingBadRequest()
    {
      result = surfacesValidator.SendRequest(expectedHttpCode: (int)HttpStatusCode.BadRequest);
    }

    [Then(@"the response should contain Code (.*) and Message ""(.*)""")]
    public void ThenTheResponseShouldContainCodeAndMessage(int code, string message)
    {
      Assert.True(code == result.Code && message == result.Message);
    }

    [When(@"I Post Surveyd Surface ""(.*)"" expecting Bad Request")]
    public void WhenIPostSurveydSurfaceExpectingBadRequest(string paramName)
    {
      resultPost = surfaceToPost.DoRequest(paramName, (int)HttpStatusCode.BadRequest);
    }

    [Then(@"the Post response should contain Code (.*) and Message ""(.*)""")]
    public void ThenThePostResponseShouldContainCodeAndMessage(int code, string message)
    {
      Assert.True(code == resultPost.Code && message == resultPost.Message);
    }
  }
}
