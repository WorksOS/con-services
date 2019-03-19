using System.IO;
using System.Linq;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using FluentAssertions;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using System.Collections.Generic;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Types;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.ConfigurationStore;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportSubGridProcessorTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void PassCountLastPassNotDbase()
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountLastPass, false,
        out CSVExportRequestArgument requestArgument);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      string row0 = @"2019/Jan/23 00:22:10.033,808525.440m,376730.880m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",34.2km/h,RTK Fixed,Medium (0.050m),?,1,1,?,?,?,?,?,?,?,0.000m,Neutral,Off,?";
      rows[0].Should().Be(row0);
    }

    [Fact(Skip = "Importing a DC file is currently being implemented")]
    public void PassCountLastPassNotDbaseWithLatLong()
    {
      var requestedSubGrids = GetSubGrids(CoordType.LatLon, OutputTypes.PassCountLastPass, false,
        out CSVExportRequestArgument requestArgument);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      string row0 = @"2019/Jan/23 00:22:10.033,808,525.440m,376,730.880m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",34.2km/h,RTK Fixed,Medium (0.050m),?,1,1,?,?,?,?,?,?,?,0.000m,Neutral,Off,?";
      rows[0].Should().Be(row0);
    }

    [Fact]
    public void PassCountLastPassDBase()
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountLastPass, true,
        out CSVExportRequestArgument requestArgument);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      string row0 = @"2019/Jan/23 00:22:10.033,808525.440,376730.880,68.631,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",34.2,RTK Fixed,Medium (0.050),,1,1,,,,,,,,0.000,Neutral,Off,";
      rows[0].Should().Be(row0);
    }
    
    [Fact]
    public void VetaFinalPass()
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.VedaFinalPass, false,
        out CSVExportRequestArgument requestArgument);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = new List<string>();
      rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      string row0 = @"2019-Jan-23 00:22:10.033,808525.440m,376730.880m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",34.2km/h,RTK Fixed,Medium (0.050m),?,1,1,?,?,?,?,?,?,?,0.000m,Neutral,Off,?";
      rows[0].Should().Be(row0);
    }
    
    [Fact]
    public void PassCountAllPassesNotDBase()
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountAllPasses, false,
        out CSVExportRequestArgument requestArgument);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileAllPassesLeafSubgrid);
      rows.Count.Should().Be(384);
      string row0 = @"2019/Jan/23 00:22:10.033,808525.440m,376730.880m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",34.2km/h,RTK Fixed,Medium (0.050m),?,0,1,?,0.0,?,0.0,?,?,?,0.000m,Neutral,Off,?";
      rows[0].Should().Be(row0);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(382)]
    [InlineData(384)]
    [InlineData(385)]
    [InlineData(400)]
    public void RowCountLimit_AllPasses(int maxExportRows)
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountAllPasses, false,
        out CSVExportRequestArgument requestArgument);

      // the fixture requires it's own ConfigStore settings, which are used in GetSubGrids.
      //  need to restore it for the next test
      var moqConfiguration = DIContext.Obtain<Mock<IConfigurationStore>>();
      moqConfiguration.Setup(x => x.GetValueInt("MAX_EXPORT_ROWS", It.IsAny<int>())).Returns(maxExportRows);

      DIBuilder.Continue().Add(x => x.AddSingleton(moqConfiguration.Object)).Complete();

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);

      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileAllPassesLeafSubgrid);
      if (maxExportRows <= 384) // 384 is the number of rows we could potentially get from this set
        rows.Count.Should().Be(maxExportRows);
      else
        rows.Count.Should().BeLessOrEqualTo(maxExportRows);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(5050)]
    [InlineData(5051)]
    [InlineData(5052)]
    [InlineData(400000)]
    public void RowCountLimit_AllPassesMultiSubGrids(int maxExportRows)
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountAllPasses, false,
        out CSVExportRequestArgument requestArgument);

      //the fixture requires it's own ConfigStore settings, which are used in GetSubGrids.
      //  need to restore it for the next test
      var moqConfiguration = DIContext.Obtain<Mock<IConfigurationStore>>();
      moqConfiguration.Setup(x => x.GetValueInt("MAX_EXPORT_ROWS", It.IsAny<int>())).Returns(maxExportRows);

      DIBuilder.Continue().Add(x => x.AddSingleton(moqConfiguration.Object)).Complete();

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);

      var rows = new List<string>();
      for (int index = 0; index < requestedSubGrids.Count; index++)
      {
        rows.AddRange(subGridProcessor.ProcessSubGrid(requestedSubGrids[index] as ClientCellProfileAllPassesLeafSubgrid));
      }

      if (maxExportRows <= 5051) // 5051 is the number of rows we could potentially get from this set
        rows.Count.Should().Be(maxExportRows);
      else
        rows.Count.Should().BeLessOrEqualTo(maxExportRows);
    }


    [Theory]
    [InlineData(10)]
    [InlineData(225)]
    [InlineData(226)]
    [InlineData(227)]
    [InlineData(400)]
    public void RowCountLimit_FinalPass(int maxExportRows)
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.VedaFinalPass, false,
        out CSVExportRequestArgument requestArgument);

      // the fixture requires it's own ConfigStore settings, which are used in GetSubGrids.
      //  need to restore it for the next test
      var moqConfiguration = DIContext.Obtain<Mock<IConfigurationStore>>();
      moqConfiguration.Setup(x => x.GetValueInt("MAX_EXPORT_ROWS", It.IsAny<int>())).Returns(maxExportRows);

      DIBuilder.Continue().Add(x => x.AddSingleton(moqConfiguration.Object)).Complete();

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = new List<string>();
      rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      if (maxExportRows <= 226) // 226 is the number of rows we could potentially get from this set
        rows.Count.Should().Be(maxExportRows);
      else
        rows.Count.Should().BeLessOrEqualTo(maxExportRows);
    }


    private List<IClientLeafSubGrid> GetSubGrids(CoordType coordType, OutputTypes outputType, bool isRawDataAsDBaseRequired, 
      out CSVExportRequestArgument requestArgument)
    {
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive"), "*.tag").ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out var _);
      var csvExportUserPreference = new CSVExportUserPreferences();
      requestArgument = new CSVExportRequestArgument
      (
        siteModel.ID, new FilterSet(new CombinedFilter()), "the filename",
        coordType, outputType, csvExportUserPreference, new List<CSVExportMappedMachine>(), false, isRawDataAsDBaseRequired
      );

      var utilities = DIContext.Obtain<IRequestorUtilities>();
      var gridDataType = outputType == OutputTypes.PassCountLastPass || outputType == OutputTypes.VedaFinalPass
        ? GridDataType.CellProfile : GridDataType.CellPasses;
      var requestors = utilities.ConstructRequestors(siteModel,
        utilities.ConstructRequestorIntermediaries(siteModel, requestArgument.Filters, false, gridDataType),
        AreaControlSet.CreateAreaControlSet(), siteModel.ExistenceMap);
      requestors.Should().NotBeNull();
      requestors.Length.Should().Be(1);

      // Request sub grids from the model
      var requestedSubGrids = new List<IClientLeafSubGrid>();
      siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(x =>
      {
        if (requestors[0].RequestSubGridInternal(x, true, false, out IClientLeafSubGrid clientGrid) == ServerRequestResult.NoError)
          requestedSubGrids.Add(clientGrid);
      });
      requestedSubGrids.Count.Should().Be(18);
      return requestedSubGrids;
    }
  }
}

