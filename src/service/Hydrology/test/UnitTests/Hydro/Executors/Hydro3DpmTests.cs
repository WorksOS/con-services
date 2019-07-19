using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Common.Helpers;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Hydrology.Tests.Hydro.Executors
{
  [TestClass]
  public class Hydro3DpmTests : UnitTestsDIFixture<Hydro3DpmTests>
  {
    private const string TTMZippedFile = "..\\..\\..\\..\\TestData\\Large Sites Road - Trimble Road.zip";
    private const string TTMUnzippedFile = "..\\..\\..\\..\\TestData\\Large Sites Road - Trimble Road.ttm";
    private const string TTMnoTTMFile = "..\\..\\..\\..\\TestData\\Triangle.zip";

    [TestMethod]
    public async Task GetSurfaceFrom3dp_Success()
    {
      var request = new HydroRequest(Guid.NewGuid(), Guid.NewGuid(), new HydroOptions(resolution:1.0), "resultantFileName.zip");
      request.Validate();

      var ttmZip = new FileStream(TTMZippedFile, FileMode.Open);
      var fileResult = new FileStreamResult(ttmZip, ContentTypeConstants.ApplicationZip);
      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(rp => rp.GetExportSurface(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<bool>(), null))
        .ReturnsAsync(fileResult.FileStream);

      var currentGroundTTMStream = await HydroRequestHelperCurrentGround.GetCurrentGround3Dp(request, Log,
        ServiceExceptionHandler, new Dictionary<string, string>(), raptorProxy.Object);

      var ttmFile = new FileStream(TTMUnzippedFile, FileMode.Open);
      
      Assert.IsNotNull(currentGroundTTMStream);
      Assert.AreEqual(ttmFile.Length, currentGroundTTMStream.Length, "TTM stream from raptorProxy invalid");
      ttmZip.Close();
      ttmFile.Close();
      currentGroundTTMStream.Close();
    }

    [TestMethod]
    public void GetSurfaceFrom3dp_InvalidZip()
    {
      var request = new HydroRequest(Guid.NewGuid(), Guid.NewGuid(), new HydroOptions(resolution: 1.0), "resultantFileName.zip");
      request.Validate();

      var ttmUnZippedFile = new FileStream(TTMUnzippedFile, FileMode.Open);
      var fileResult = new FileStreamResult(ttmUnZippedFile, ContentTypeConstants.ApplicationZip);
      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(rp => rp.GetExportSurface(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<bool>(), null))
        .ReturnsAsync(fileResult.FileStream);

      var ex = Assert.ThrowsExceptionAsync<ServiceException>(async () => await HydroRequestHelperCurrentGround.GetCurrentGround3Dp(request, Log,
       ServiceExceptionHandler, new Dictionary<string, string>(), raptorProxy.Object));

      Assert.AreEqual(HttpStatusCode.InternalServerError, ex.Result.Code);
      Assert.AreEqual(2027, ex.Result.GetResult.Code);
      Assert.AreEqual(HydroErrorCodesProvider.FirstNameWithOffset(27), ex.Result.GetResult.Message);
    }

    [TestMethod]
    public void GetSurfaceFrom3dp_MissingTTM()
    {
      var request = new HydroRequest(Guid.NewGuid(), Guid.NewGuid(), new HydroOptions(resolution: 1.0), "resultantFileName.zip");
      request.Validate();

      var ttmNoTTMFileZipped = new FileStream(TTMnoTTMFile, FileMode.Open);
      var fileResult = new FileStreamResult(ttmNoTTMFileZipped, ContentTypeConstants.ApplicationZip);
      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(rp => rp.GetExportSurface(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<bool>(), null))
        .ReturnsAsync(fileResult.FileStream);

      var ex = Assert.ThrowsExceptionAsync<ServiceException>(async () => await HydroRequestHelperCurrentGround.GetCurrentGround3Dp(request, Log,
        ServiceExceptionHandler, new Dictionary<string, string>(), raptorProxy.Object));

      Assert.AreEqual(HttpStatusCode.InternalServerError, ex.Result.Code);
      Assert.AreEqual(2026, ex.Result.GetResult.Code);
      Assert.AreEqual(HydroErrorCodesProvider.FirstNameWithOffset(26), ex.Result.GetResult.Message);
    }

  }
}
