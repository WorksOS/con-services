using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
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

    private string MockS3FileTransfer_UploadToBucket()
    {
      var tempFileName = Path.GetTempFileName() + ".zip";

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

    [Fact]
    public void Test_CSVExportRequest_Creation()
    {
      var request = new CSVExportRequest();
      request.Should().NotBeNull();
    }

    private CSVExportRequestArgument SimpleCSVExportRequestArgument(Guid projectUid)
    {
      return new CSVExportRequestArgument
      {
        FileName = "the file name",
        Filters = new FilterSet(new CombinedFilter()),
        CoordType = CoordType.Northeast,
        OutputType = OutputTypes.PassCountLastPass,
        UserPreferences = new CSVExportUserPreferences(),
        MappedMachines = new List<CSVExportMappedMachine>(),
        RestrictOutputSize = false,
        RawDataAsDBase = false,
        TRexNodeID = "'Test_CSVExportRequest_Execute_EmptySiteModel",
        ProjectID = projectUid
      };
    }

    [Fact]
    public void Test_CSVExportRequest_Execute_EmptySiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      var response = request.Execute(SimpleCSVExportRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public void Test_CSVExportRequest_Execute_SingleCellSinglePass()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tempFileName = MockS3FileTransfer_UploadToBucket();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();

      var baseDate = new DateTime(2000, 1, 1, 1, 0, 0, 0);
      var cellPasses = new[]
      {
        new CellPass
        {
          Time = baseDate,
          Height = 1.0f
        }
      };

      DITAGFileAndSubGridRequestsWithIgniteFixture.AddSingleCellWithPasses(siteModel,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      var response = request.Execute(SimpleCSVExportRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);

      // Read back the zip file
      var archive = ZipFile.Open(tempFileName, ZipArchiveMode.Read);
      var extractedFileName = Path.GetTempFileName() + ".csv";
      archive.Entries[0].ExtractToFile(extractedFileName);

      var lines = File.ReadAllLines(extractedFileName);

      lines.Length.Should().Be(2);
    }

    [Fact]
    public void Test_CSVExportRequest_Execute_SingleSubGridSinglePass()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tempFileName = MockS3FileTransfer_UploadToBucket();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CSVExportRequest();
      var baseDate = new DateTime(2000, 1, 1, 1, 0, 0, 0);

      var cellPasses = new CellPass[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension][];
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        cellPasses[x, y] = new[]
        {
          new CellPass
          {
            Time = baseDate,
            Height = 1.0f
          }
        };
      });

      DITAGFileAndSubGridRequestsWithIgniteFixture.AddSingleSubGridWithPasses(siteModel,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      var response = request.Execute(SimpleCSVExportRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);

      // Read back the zip file
      var archive = ZipFile.Open(tempFileName, ZipArchiveMode.Read);
      var extractedFileName = Path.GetTempFileName() + ".csv";
      archive.Entries[0].ExtractToFile(extractedFileName);

      var lines = File.ReadAllLines(extractedFileName);

      lines.Length.Should().Be(SubGridTreeConsts.SubGridTreeCellsPerSubGrid + 1);
    }
  }
}


