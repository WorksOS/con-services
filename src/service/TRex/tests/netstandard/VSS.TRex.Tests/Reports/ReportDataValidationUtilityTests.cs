using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.DI;
using VSS.TRex.Gateway.WebApi.ActionServices;
using Moq;
using Xunit;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Alignments.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.TRex.Tests.Reports.StationOffset
{
  public class ReportDataValidationUtilityTests : IDisposable
  {
    [Theory]
    [InlineData("17e6bd66-54d8-4651-8907-88b15d81b2d7", "27e6bd66-54d8-4651-8907-88b15d81b2d7", null)]
    public void ValidateReportData_GriddedSuccess(Guid projectUid, Guid cutFillDesignUid, Guid? alignmentDesignUid)
    {
      var mockSiteModel = new Mock<ISiteModel>();
      var mockSiteModels = new Mock<ISiteModels>();
      mockSiteModels.Setup(x => x.GetSiteModel(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(mockSiteModel.Object);

      var mockDesign = new Mock<IDesign>();
      var mockDesigns = new Mock<IDesigns>();
      var mockDesignManager = new Mock<IDesignManager>();
      mockDesignManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockDesigns.Object);
      mockDesigns.Setup(x => x.Locate(It.IsAny<Guid>())).Returns(mockDesign.Object);

      var mockAlignment = new Mock<IAlignment>();
      var mockAlignments = new Mock<IAlignments>();
      var mockAlignmentManager = new Mock<IAlignmentManager>();
      mockAlignmentManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockAlignments.Object);
      mockAlignments.Setup(x => x.Locate(It.IsAny<Guid>())).Returns((IAlignment) null);

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ISiteModels>(mockSiteModels.Object))
        .Add(x => x.AddSingleton<IDesignManager>(mockDesignManager.Object))
        .Add(x => x.AddSingleton<IAlignmentManager>(mockAlignmentManager.Object))
        .Add(x => x.AddTransient<IReportDataValidationUtility, ReportDataValidationUtility>())
        .Complete();

      var request = CompactionReportGridTRexRequest.CreateRequest(
        projectUid, null,
        true, true, true, true, true, true,
        cutFillDesignUid, 
        null, GridReportOption.Automatic,
        800000, 400000, 800001, 400001, 2);

      var isOk = DIContext.Obtain<IReportDataValidationUtility>().ValidateData(request);
      Assert.True(isOk);
    }

    [Theory]
    [InlineData("17e6bd66-54d8-4651-8907-88b15d81b2d7", null, "27e6bd66-54d8-4651-8907-88b15d81b2d7")]
    public void ValidateReportData_AlignmentSuccess(Guid projectUid, Guid? cutFillDesignUid, Guid alignmentDesignUid)
    {
      var mockSiteModel = new Mock<ISiteModel>();
      var mockSiteModels = new Mock<ISiteModels>();
      mockSiteModels.Setup(x => x.GetSiteModel(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(mockSiteModel.Object);

      var mockDesign = new Mock<IDesign>();
      var mockDesigns = new Mock<IDesigns>();
      var mockDesignManager = new Mock<IDesignManager>();
      mockDesignManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockDesigns.Object);
      mockDesigns.Setup(x => x.Locate(It.IsAny<Guid>())).Returns((IDesign)null);

      var mockAlignment = new Mock<IAlignment>();
      var mockAlignments = new Mock<IAlignments>();
      var mockAlignmentManager = new Mock<IAlignmentManager>();
      mockAlignmentManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockAlignments.Object);
      mockAlignments.Setup(x => x.Locate(It.IsAny<Guid>())).Returns(mockAlignment.Object);

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ISiteModels>(mockSiteModels.Object))
        .Add(x => x.AddSingleton<IDesignManager>(mockDesignManager.Object))
        .Add(x => x.AddSingleton<IAlignmentManager>(mockAlignmentManager.Object))
        .Add(x => x.AddTransient<IReportDataValidationUtility, ReportDataValidationUtility>())
        .Complete();
      
      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, null,
        true, true, true, true, true, true,
        cutFillDesignUid, alignmentDesignUid,
        2.0, 100, 200, new[] { -1.0, 0, 1.3 });

      var isOk = DIContext.Obtain<IReportDataValidationUtility>().ValidateData(request);
      Assert.True(isOk);
    }

    [Theory]
    [InlineData("17e6bd66-54d8-4651-8907-88b15d81b2d7", "27e6bd66-54d8-4651-8907-88b15d81b2d7", "37e6bd66-54d8-4651-8907-88b15d81b2d7")]
    public void ValidateReportData_AlignmentIncludingCutFill_Success(Guid projectUid, Guid cutFillDesignUid, Guid alignmentDesignUid)
    {
      var mockSiteModel = new Mock<ISiteModel>();
      var mockSiteModels = new Mock<ISiteModels>();
      mockSiteModels.Setup(x => x.GetSiteModel(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(mockSiteModel.Object);

      var mockDesign = new Mock<IDesign>();
      var mockDesigns = new Mock<IDesigns>();
      var mockDesignManager = new Mock<IDesignManager>();
      mockDesignManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockDesigns.Object);
      mockDesigns.Setup(x => x.Locate(It.IsAny<Guid>())).Returns(mockDesign.Object);

      var mockAlignment = new Mock<IAlignment>();
      var mockAlignments = new Mock<IAlignments>();
      var mockAlignmentManager = new Mock<IAlignmentManager>();
      mockAlignmentManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockAlignments.Object);
      mockAlignments.Setup(x => x.Locate(It.IsAny<Guid>())).Returns(mockAlignment.Object);

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ISiteModels>(mockSiteModels.Object))
        .Add(x => x.AddSingleton<IDesignManager>(mockDesignManager.Object))
        .Add(x => x.AddSingleton<IAlignmentManager>(mockAlignmentManager.Object))
        .Add(x => x.AddTransient<IReportDataValidationUtility, ReportDataValidationUtility>())
        .Complete();

      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, null,
        true, true, true, true, true, true,
        cutFillDesignUid, alignmentDesignUid,
        2.0, 100, 200, new[] { -1.0, 0, 1.3 });

      var isOk = DIContext.Obtain<IReportDataValidationUtility>().ValidateData(request);
      Assert.True(isOk);
    }

    [Theory]
    [InlineData("17e6bd66-54d8-4651-8907-88b15d81b2d7", "27e6bd66-54d8-4651-8907-88b15d81b2d7", null)]
    public void ValidateReportData_GriddedNoSiteModel_Fail(Guid projectUid, Guid cutFillDesignUid, Guid? alignmentDesignUid)
    {
      var mockSiteModel = new Mock<ISiteModel>();
      var mockSiteModels = new Mock<ISiteModels>();
      mockSiteModels.Setup(x => x.GetSiteModel(It.IsAny<Guid>(), It.IsAny<bool>())).Returns((ISiteModel) null);

      var mockDesign = new Mock<IDesign>();
      var mockDesigns = new Mock<IDesigns>();
      var mockDesignManager = new Mock<IDesignManager>();
      mockDesignManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockDesigns.Object);
      mockDesigns.Setup(x => x.Locate(It.IsAny<Guid>())).Returns(mockDesign.Object);

      var mockAlignment = new Mock<IAlignment>();
      var mockAlignments = new Mock<IAlignments>();
      var mockAlignmentManager = new Mock<IAlignmentManager>();
      mockAlignmentManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockAlignments.Object);
      mockAlignments.Setup(x => x.Locate(It.IsAny<Guid>())).Returns((IAlignment)null);

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ISiteModels>(mockSiteModels.Object))
        .Add(x => x.AddSingleton<IDesignManager>(mockDesignManager.Object))
        .Add(x => x.AddSingleton<IAlignmentManager>(mockAlignmentManager.Object))
        .Add(x => x.AddTransient<IReportDataValidationUtility, ReportDataValidationUtility>())
        .Complete();

      var request = CompactionReportGridTRexRequest.CreateRequest(
        projectUid, null,
        true, true, true, true, true, true,
        cutFillDesignUid,
        null, GridReportOption.Automatic,
        800000, 400000, 800001, 400001, 2);

      var ex = Assert.Throws<ServiceException>(() => DIContext.Obtain<IReportDataValidationUtility>().ValidateData(request));
      Assert.Equal(System.Net.HttpStatusCode.BadRequest, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code);
      Assert.Equal($"Project: {projectUid} is not found.", ex.GetResult.Message);
    }

    [Theory]
    [InlineData("17e6bd66-54d8-4651-8907-88b15d81b2d7", "27e6bd66-54d8-4651-8907-88b15d81b2d7", null)]
    public void ValidateReportData_GriddedNoDesign_Fail(Guid projectUid, Guid cutFillDesignUid, Guid? alignmentDesignUid)
    {
      var mockSiteModel = new Mock<ISiteModel>();
      var mockSiteModels = new Mock<ISiteModels>();
      mockSiteModels.Setup(x => x.GetSiteModel(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(mockSiteModel.Object);

      var mockDesign = new Mock<IDesign>();
      var mockDesigns = new Mock<IDesigns>();
      var mockDesignManager = new Mock<IDesignManager>();
      mockDesignManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockDesigns.Object);
      mockDesigns.Setup(x => x.Locate(It.IsAny<Guid>())).Returns((IDesign) null);

      var mockAlignment = new Mock<IAlignment>();
      var mockAlignments = new Mock<IAlignments>();
      var mockAlignmentManager = new Mock<IAlignmentManager>();
      mockAlignmentManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockAlignments.Object);
      mockAlignments.Setup(x => x.Locate(It.IsAny<Guid>())).Returns((IAlignment)null);

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ISiteModels>(mockSiteModels.Object))
        .Add(x => x.AddSingleton<IDesignManager>(mockDesignManager.Object))
        .Add(x => x.AddSingleton<IAlignmentManager>(mockAlignmentManager.Object))
        .Add(x => x.AddTransient<IReportDataValidationUtility, ReportDataValidationUtility>())
        .Complete();

      var request = CompactionReportGridTRexRequest.CreateRequest(
        projectUid, null,
        true, true, true, true, true, true,
        cutFillDesignUid,
        null, GridReportOption.Automatic,
        800000, 400000, 800001, 400001, 2);

      var ex = Assert.Throws<ServiceException>(() => DIContext.Obtain<IReportDataValidationUtility>().ValidateData(request));
      Assert.Equal(System.Net.HttpStatusCode.BadRequest, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code);
      Assert.Equal($"CutFill design {cutFillDesignUid} is not found.", ex.GetResult.Message);
    }

    [Theory]
    [InlineData("17e6bd66-54d8-4651-8907-88b15d81b2d7", null, "27e6bd66-54d8-4651-8907-88b15d81b2d7")]
    public void ValidateReportData_AlignmentFailed(Guid projectUid, Guid? cutFillDesignUid, Guid alignmentDesignUid)
    {
      var mockSiteModel = new Mock<ISiteModel>();
      var mockSiteModels = new Mock<ISiteModels>();
      mockSiteModels.Setup(x => x.GetSiteModel(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(mockSiteModel.Object);

      var mockDesign = new Mock<IDesign>();
      var mockDesigns = new Mock<IDesigns>();
      var mockDesignManager = new Mock<IDesignManager>();
      mockDesignManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockDesigns.Object);
      mockDesigns.Setup(x => x.Locate(It.IsAny<Guid>())).Returns((IDesign)null);

      var mockAlignment = new Mock<IAlignment>();
      var mockAlignments = new Mock<IAlignments>();
      var mockAlignmentManager = new Mock<IAlignmentManager>();
      mockAlignmentManager.Setup(x => x.List(It.IsAny<Guid>())).Returns(mockAlignments.Object);
      mockAlignments.Setup(x => x.Locate(It.IsAny<Guid>())).Returns((IAlignment)null);

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ISiteModels>(mockSiteModels.Object))
        .Add(x => x.AddSingleton<IDesignManager>(mockDesignManager.Object))
        .Add(x => x.AddSingleton<IAlignmentManager>(mockAlignmentManager.Object))
        .Add(x => x.AddTransient<IReportDataValidationUtility, ReportDataValidationUtility>())
        .Complete();

      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, null,
        true, true, true, true, true, true,
        cutFillDesignUid, alignmentDesignUid,
        2.0, 100, 200, new[] { -1.0, 0, 1.3 });

      var ex = Assert.Throws<ServiceException>(() => DIContext.Obtain<IReportDataValidationUtility>().ValidateData(request));
      Assert.Equal(System.Net.HttpStatusCode.BadRequest, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code);
      Assert.Equal($"Alignment design {alignmentDesignUid} is not found.", ex.GetResult.Message);
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }
}


