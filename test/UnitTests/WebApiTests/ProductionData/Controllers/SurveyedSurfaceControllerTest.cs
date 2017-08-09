using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class SurveyedSurfaceControllerTest
  {
    private const long PD_MODEL_ID = 1;//544; // Dimensions 2012 project...

    /// <summary>
    /// Creates an instance of the SurveyedSurfaceRequest class.
    /// </summary>
    /// <returns>The created instance.</returns>
    private SurveyedSurfaceRequest CreateRequest()
    {
      return SurveyedSurfaceRequest.CreateSurveyedSurfaceRequest(
        PD_MODEL_ID,
        DesignDescriptor.HelpSample,
        DateTime.UtcNow
        );
    }
  }
}