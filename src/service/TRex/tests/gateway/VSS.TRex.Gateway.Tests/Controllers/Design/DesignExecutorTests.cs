using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Alignments;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Geometry;
using VSS.TRex.SurveyedSurfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Design
{
  public class DesignExecutorTests : IDisposable
  {
    [Theory]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", ImportedFileType.DesignSurface, "validFileName.ttm", "408A150C-B606-E311-9E53-0050568824D7", null)]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", ImportedFileType.SurveyedSurface, "validFileName.ttm", "408A150C-B606-E311-9E53-0050568824D7", "2018-10-12")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", ImportedFileType.Alignment, "validFileName.svl", "408A150C-B606-E311-9E53-0050568824D7", null)]
    public void DesignRequestValidation_HappyPath(string projectUid, ImportedFileType fileType, string fileName, string designUid, DateTime surveyedUtc)
    {
      DesignRequest designSurfaceRequest = new DesignRequest(Guid.Parse(projectUid), fileType, fileName, Guid.Parse(designUid), surveyedUtc);
      designSurfaceRequest.Validate();
    }

    [Theory]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", ImportedFileType.DesignSurface, "invalidFileName.dxf", "408A150C-B606-E311-9E53-0050568824D7", -1, "File name extension incorrect")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", ImportedFileType.DesignSurface, "", "408A150C-B606-E311-9E53-0050568824D7", -1, "File name must be provided")]
    [InlineData("00000000-0000-0000-0000-000000000000", ImportedFileType.DesignSurface, "validFileName.ttm", "408A150C-B606-E311-9E53-0050568824D7", -1, "ProjectUid must be provided")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", ImportedFileType.DesignSurface, "validFileName.ttm", "00000000-0000-0000-0000-000000000000", -1, "DesignUid must be provided")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", ImportedFileType.SurveyedSurface, "validFileName.ttm", "408A150C-B606-E311-9E53-0050568824D7", -1, "SurveyedUtc must be provided for a SurveyedSurface file type")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", ImportedFileType.Alignment, "validFileName.ttm", "408A150C-B606-E311-9E53-0050568824D7", -1, "File name extension incorrect")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", ImportedFileType.MassHaulPlan, "validFileName.ttm", "408A150C-B606-E311-9E53-0050568824D7", -1, "File type must be DesignSurface, SurveyedSurface or Alignment")]
    public void DesignRequestValidation_Errors(string projectUid, ImportedFileType fileType, string fileName, string designUid, int expectedCode, string expectedMessage)
    {
      DesignRequest designSurfaceRequest = new DesignRequest(Guid.Parse(projectUid), fileType, fileName, Guid.Parse(designUid), null);

      var ex = Assert.Throws<ServiceException>(() => designSurfaceRequest.Validate());
      Assert.Equal(expectedCode, ex.GetResult.Code);
      Assert.Equal(expectedMessage, ex.GetResult.Message);
    }


    [Fact]
    public void FileTransfer_HappyPath()
    {
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>()));
      var mockConfig = new Mock<IConfigurationStore>();
      mockConfig.Setup(x => x.GetValueString("AWS_DESIGNIMPORT_BUCKET_NAME")).Returns("vss-projects-stg");

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton(mockConfig.Object))
        .Add(x => x.AddSingleton(mockTransferProxy.Object))
        .Complete();

      Guid projectUid = Guid.Parse("A11F2458-6666-424F-A995-4426a00771AE");
      string transferFileName = "TransferTestDesign.ttm";
      var isWrittenToS3Ok = S3FileTransfer.WriteFile("TestData", projectUid, transferFileName);
      Assert.True(isWrittenToS3Ok);
    }

    [Fact]
    public void MapDesignsurfaceToResult()
    {
      var fileName = "theFile name.ttm";
      var designUid = Guid.NewGuid();
      var designDescriptor = new TRex.Designs.Models.DesignDescriptor(designUid, "", fileName, 0);
      var extents = new BoundingWorldExtent3D(1, 2, 50, 100, -45, 50 );
      var design = new Designs.Storage.Design(designUid, designDescriptor, extents);

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

    [Fact]
    public void MapAlignmentToResult()
    {
      var fileName = "theFile name.svl";
      var designUid = Guid.NewGuid();
      var designDescriptor = new TRex.Designs.Models.DesignDescriptor(designUid, "", fileName, 0);
      var extents = new BoundingWorldExtent3D(1, 2, 50, 100);
      var design = new Alignment(designUid, designDescriptor, extents);

      var result = AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(design);

      Assert.Equal(ImportedFileType.Alignment, result.FileType);
      Assert.Equal(fileName, result.Name);
      Assert.Equal(designUid.ToString(), result.DesignUid);
      Assert.Equal(extents.MaxX, result.Extents.MaxX);
    }
    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }
}
