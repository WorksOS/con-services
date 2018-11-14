using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common;
using VSS.TRex.Designs.Storage;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Geometry;
using VSS.TRex.SurveyedSurfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Gateway.Tests
{
  public class DesignExecutorTests
  {

    private Guid projectUid = Guid.Parse("A11F2458-6666-424F-A995-4426a00771AE");
    private string transferFileName = "TransferTestDesign.ttm";

    [Theory]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", 1, "validFileName.ttm", "408A150C-B606-E311-9E53-0050568824D7")]
    public void CreateDesignRequestValidation_HappyPath(string projectUid, int fileType, string fileName, string designUid)
    {
      DesignRequest designSurfaceRequest = new DesignRequest(Guid.Parse(projectUid), (ImportedFileType) fileType, fileName, Guid.Parse(designUid), null);
      designSurfaceRequest.Validate();
    }

    [Theory]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", 1, "invalidFileName.dxf", "408A150C-B606-E311-9E53-0050568824D7", -1, "File name extension must be ttm")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", 1, "", "408A150C-B606-E311-9E53-0050568824D7", -1, "File name must be provided")]
    [InlineData("00000000-0000-0000-0000-000000000000", 1, "validFileName.ttm", "408A150C-B606-E311-9E53-0050568824D7", -1, "ProjectUid must be provided")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", 1, "validFileName.ttm", "00000000-0000-0000-0000-000000000000", -1, "DesignUid must be provided")]
    public void CreateDesignRequestValidation_Errors(string projectUid, int fileType, string fileName, string designUid, int expectedCode, string expectedMessage)
    {
      DesignRequest designSurfaceRequest = new DesignRequest(Guid.Parse(projectUid), (ImportedFileType)fileType, fileName, Guid.Parse(designUid), null);

      var ex = Assert.Throws<ServiceException>(() => designSurfaceRequest.Validate());
      Assert.Equal(expectedCode, ex.GetResult.Code);
      Assert.Equal(expectedMessage, ex.GetResult.Message);
    }


    [Fact]
    public void FileTransfer_HappyPath()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<ITransferProxy>(sp => new TransferProxy(sp.GetRequiredService<IConfigurationStore>(), "AWS_DESIGNIMPORT_BUCKET_NAME")))
        .Complete();

      var isWrittenToS3Ok = S3FileTransfer.WriteFile("TestData", projectUid, transferFileName);
      Assert.True(isWrittenToS3Ok);

      var isReadFromS3Ok = S3FileTransfer.ReadFile(projectUid, transferFileName, Path.GetTempPath()).Result;
      Assert.True(isReadFromS3Ok);
    }

    [Fact]
    public void MapDesignsurfaceToResult()
    {
      var fileName = "theFile name.ttm";
      var designUid = Guid.NewGuid();
      var designDescriptor = new TRex.Designs.Models.DesignDescriptor(designUid, "", fileName, 0);
      var extents = new BoundingWorldExtent3D(1, 2, 50, 100, -45, 50 );
      var design = new Design(designUid, designDescriptor, extents);

      var result = AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(design);

      Assert.Equal(ImportedFileType.DesignSurface, result.FileType);
      Assert.Equal(fileName, result.Name);
      Assert.Equal(designUid.ToString(), result.DesignUid);
      Assert.Equal(extents.MaxX, result.Extents.MaxX);
      Assert.Null(result.SurveyedUtc);
    }

    [Fact]
    public void MapSurveyedSurfaceToResult()
    {
      var fileName = "theFile name.ttm";
      var designUid = Guid.NewGuid();
      var designDescriptor = new TRex.Designs.Models.DesignDescriptor(designUid, "", fileName, 0);
      var extents = new BoundingWorldExtent3D(1, 2, 50, 100);
      var surveyedUtc = DateTime.UtcNow.AddDays(-2);
      var design = new SurveyedSurface(designUid, designDescriptor, surveyedUtc, extents);

      var result = AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(design);

      Assert.Equal(ImportedFileType.SurveyedSurface, result.FileType);
      Assert.Equal(fileName, result.Name);
      Assert.Equal(designUid.ToString(), result.DesignUid);
      Assert.Equal(extents.MaxX, result.Extents.MaxX);
      Assert.Equal(surveyedUtc, result.SurveyedUtc);
    }

  }
}
