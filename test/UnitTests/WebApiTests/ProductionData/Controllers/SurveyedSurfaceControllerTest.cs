using System;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Raptor.Service.WebApi.ProductionData.Controllers;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class SurveyedSurfaceControllerTest
  {
    private const long PD_MODEL_ID = 1;//544; // Dimensions 2012 project...

    /// <summary>
    /// Creates an instance of the SurveyedSurfaceRequest class.
    /// </summary>
    /// <returns>The created instance.</returns>
    /// 
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
