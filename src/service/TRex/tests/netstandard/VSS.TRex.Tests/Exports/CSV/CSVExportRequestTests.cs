using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Tests.Exports.CSV
{
  [UnitTestCoveredRequest(RequestType = typeof(CSVExportRequest))]
  public class CSVExportRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<CSVExportRequestComputeFunc, CSVExportRequestArgument, CSVExportRequestResponse>();
    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    public CSVExportRequestTests()
    {
      DILoggingFixture.SetMaxExportRowsConfig(Consts.DEFAULT_MAX_EXPORT_ROWS);
    }

    [Fact]
    public void CSVExportRequest_Creation()
    {
      var request = new CSVExportRequest();
      request.Should().NotBeNull();
    }

    [Fact]
    public async Task CSVExportRequest_Execute_EmptySiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      var response = await request.ExecuteAsync(SimpleCSVExportRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public async Task CSVExportRequest_Execute_SingleCellSinglePass_CellProfile()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tempFileName = MockS3FileTransfer_UploadToBucket();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      var baseDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0, 0), DateTimeKind.Utc);

      var cellPasses = new[] { new CellPass { Time = baseDate, Height = 1.0f } };

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      var response = await request.ExecuteAsync(SimpleCSVExportRequestArgument(siteModel.ID));
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);

      // Read back the zip file
      using (var archive = ZipFile.Open(tempFileName, ZipArchiveMode.Read))
      {
        var extractedFileName = tempFileName.Remove(tempFileName.Length - 4) + ".csv";
        archive.Entries[0].ExtractToFile(extractedFileName);

        var lines = File.ReadAllLines(extractedFileName);
        lines.Length.Should().Be(2);
        lines[0].Should()
          .Be(
            "Time,CellN,CellE,Elevation,PassCount,LastRadioLtncy,DesignName,Machine,Speed,LastGPSMode,GPSAccTol,TargPassCount,TotalPasses,Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq,LastAmp,TargThickness,MachineGear,VibeState,LastTemp");
        lines[1].Should()
          .Be(
            @"2000/Jan/01 01:00:00.000,0.170m,0.170m,1.000m,1,0,?,""Unknown"",0.0km/h,Old Position,?,?,1,1,?,?,0.0,?,?,?,?,?,?,?,0.0°C");
      }

      CleanupMockedFile(tempFileName, siteModel.ID);
    }

    [Fact]
    public async Task CSVExportRequest_Execute_SingleCellSinglePass_CellProfileAllPasses()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tempFileName = MockS3FileTransfer_UploadToBucket();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();

      var baseDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0, 0), DateTimeKind.Utc);
      var cellPasses = new[] { new CellPass { Time = baseDate, Height = 1.0f } };

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      var response = await request.ExecuteAsync(SimpleCSVExportRequestArgument(siteModel.ID, OutputTypes.PassCountAllPasses));
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);

      // Read back the zip file
      using (var archive = ZipFile.Open(tempFileName, ZipArchiveMode.Read))
      {
        var extractedFileName = tempFileName.Remove(tempFileName.Length - 4) + ".csv";
        archive.Entries[0].ExtractToFile(extractedFileName);

        var lines = File.ReadAllLines(extractedFileName);
        lines.Length.Should().Be(2);
        lines[0].Should()
          .Be(
            "Time,CellN,CellE,Elevation,PassNumber,LastRadioLtncy,DesignName,Machine,Speed,LastGPSMode,GPSAccTol,TargPassCount,ValidPos,Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq,LastAmp,TargThickness,MachineGear,VibeState,LastTemp");
        lines[1].Should()
          .Be(
            @"2000/Jan/01 01:00:00.000,0.170m,0.170m,1.000m,1,0,?,""Unknown"",0.0km/h,Old Position,?,?,0,1,?,?,0.0,?,?,?,?,?,?,?,0.0°C");
      }

      CleanupMockedFile(tempFileName, siteModel.ID);
    }

    [Fact]
    public async Task CSVExportRequest_Execute_SingleSubGridSinglePass()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tempFileName = MockS3FileTransfer_UploadToBucket();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      var baseDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0, 0), DateTimeKind.Utc);

      var cellPasses = new CellPass[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension][];
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        cellPasses[x, y] = new[] { new CellPass { Time = baseDate, Height = 1.0f } }; 
      });

      DITAGFileAndSubGridRequestsFixture.AddSingleSubGridWithPasses(siteModel,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      var response = await request.ExecuteAsync(SimpleCSVExportRequestArgument(siteModel.ID));
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);

      // Read back the zip file
      using (var archive = ZipFile.Open(tempFileName, ZipArchiveMode.Read))
      {
        var extractedFileName = tempFileName.Remove(tempFileName.Length - 4) + ".csv";
        archive.Entries[0].ExtractToFile(extractedFileName);

        var lines = File.ReadAllLines(extractedFileName);
        lines.Length.Should().Be(SubGridTreeConsts.SubGridTreeCellsPerSubGrid + 1);
        lines[0].Should().BeEquivalentTo(
          "Time,CellN,CellE,Elevation,PassCount,LastRadioLtncy,DesignName,Machine,Speed,LastGPSMode,GPSAccTol,TargPassCount,TotalPasses,Lift,LastCMV,TargCMV,LastMDP,TargMDP,LastRMV,LastFreq,LastAmp,TargThickness,MachineGear,VibeState,LastTemp");
        lines[1].Should()
          .BeEquivalentTo(
            "2000/Jan/01 01:00:00.000,0.170m,0.170m,1.000m,1,0,?,\"Unknown\",0.0km/h,Old Position,?,?,1,1,?,?,0.0,?,?,?,?,?,?,?,0.0°C");
        lines[10].Length.Should().Be(118);
        lines[10].Should()
          .BeEquivalentTo(
            "2000/Jan/01 01:00:00.000,3.230m,0.170m,1.000m,1,0,?,\"Unknown\",0.0km/h,Old Position,?,?,1,1,?,?,0.0,?,?,?,?,?,?,?,0.0°C");
      }

      CleanupMockedFile(tempFileName, siteModel.ID);
    }

    [Fact]
    public async Task CSVExportRequest_Execute_UnableToWriteResultToS3()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tempFileName = MockS3FileTransfer_UploadToBucket("InvalidFilename*@");
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      var baseDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0, 0), DateTimeKind.Utc);

      var cellPasses = new CellPass[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension][];
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        cellPasses[x, y] = new[] {new CellPass {Time = baseDate, Height = 1.0f}};
      });

      DITAGFileAndSubGridRequestsFixture.AddSingleSubGridWithPasses(siteModel,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      var response = await request.ExecuteAsync(SimpleCSVExportRequestArgument(siteModel.ID));
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.ExportUnableToLoadFileToS3);

      CleanupMockedFile(tempFileName, siteModel.ID);
    }

    [Fact]
    public async Task CSVExportRequest_Execute_NoProductionData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tempFileName = MockS3FileTransfer_UploadToBucket();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      
      var response = await request.ExecuteAsync(SimpleCSVExportRequestArgument(siteModel.ID));
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);

      CleanupMockedFile(tempFileName, siteModel.ID);
    }

    
    [Fact]
    public async Task CSVExportRequest_Execute_NoCellPasses()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tempFileName = MockS3FileTransfer_UploadToBucket();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      var baseDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0, 0), DateTimeKind.Utc);

      // half passes only pick up every 2nd one.
      var cellPasses = new CellPass[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension][];
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        cellPasses[x, y] = new[] { new CellPass { Time = baseDate, Height = 1.0f, HalfPass = true} };
      });

      DITAGFileAndSubGridRequestsFixture.AddSingleSubGridWithPasses(siteModel,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      var response = await request.ExecuteAsync(SimpleCSVExportRequestArgument(siteModel.ID));
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.ExportNoDataFound);

      CleanupMockedFile(tempFileName, siteModel.ID);
    }

    [Fact]
    public async Task CSVExportRequest_Execute_ExceedsLimit()
    {
      DILoggingFixture.SetMaxExportRowsConfig(1);

      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tempFileName = MockS3FileTransfer_UploadToBucket();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      var baseDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0, 0), DateTimeKind.Utc);

      var cellPasses = new CellPass[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension][];
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        cellPasses[x, y] = new[] { new CellPass { Time = baseDate, Height = 1.0f} };
      });
      DITAGFileAndSubGridRequestsFixture.AddSingleSubGridWithPasses(siteModel,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      var response = await request.ExecuteAsync(SimpleCSVExportRequestArgument(siteModel.ID));
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.ExportExceededRowLimit);

      CleanupMockedFile(tempFileName, siteModel.ID);
    }

    private CSVExportRequestArgument SimpleCSVExportRequestArgument(Guid projectUid, OutputTypes outputType = OutputTypes.PassCountLastPass)
    {
      return new CSVExportRequestArgument
      {
        FileName = "the file name",
        Filters = new FilterSet(new CombinedFilter()),
        CoordType = CoordType.Northeast,
        OutputType = outputType,
        UserPreferences = new CSVExportUserPreferences(),
        MappedMachines = new List<CSVExportMappedMachine>(),
        RestrictOutputSize = false,
        RawDataAsDBase = false,
        TRexNodeID = "'Test_CSVExportRequest_Execute_EmptySiteModel",
        ProjectID = projectUid
      };
    }

    private string MockS3FileTransfer_UploadToBucket(string overrideFilename = null)
    {
      var tempFileName = overrideFilename ?? Path.GetTempFileName() + ".zip";

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
        .Callback((Stream stream, string s3Path, string s3Bucket) =>
        {
          // Copy the file to a known temporary file
          using (var fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write))
          {
            stream.CopyTo(fileStream);
          }
        });

      DIBuilder.Continue().Add(x => x.AddSingleton(mockTransferProxy.Object)).Complete();

      return tempFileName;
    }

    private void CleanupMockedFile(string mockedFileWithPath, Guid projectUid)
    {
      var nameWithoutZip = mockedFileWithPath.Remove(mockedFileWithPath.Length - 4);
      if (File.Exists(nameWithoutZip))
        File.Delete(nameWithoutZip);

      if (File.Exists(nameWithoutZip + ".csv"))
        File.Delete(nameWithoutZip + ".csv");

      if (File.Exists(mockedFileWithPath))
        File.Delete(mockedFileWithPath);

      var tempProjectPath = Path.Combine(new[] { Path.GetTempPath(), projectUid.ToString() });
      if (Directory.Exists(tempProjectPath))
        Directory.Delete(tempProjectPath);
    }
  }
}


