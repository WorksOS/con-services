using System.IO;
using System.Linq;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using FluentAssertions;
using VSS.TRex.Exports.CSV.GridFabric;
using Formatter = VSS.TRex.Exports.CSV.Executors.Tasks.Formatter;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using System.Collections.Generic;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Types;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportSubGridProcessorTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void PassCountLastPassNotDbase()
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountLastPass, false,
        out ISiteModel siteModel, out CSVExportRequestArgument requestArgument, out Formatter formatter);

      var subGridProcessor = new CSVExportSubGridProcessor(siteModel, requestArgument, formatter);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      string row0 = @"2019/Jan/23 00:22:10.033,808525.440m,376730.880m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",0.2km/h,RTK Fixed,Medium (0.050m),?,1,1,?,?,?,?,?,?,?,0.000m,Neutral,Off,?";
      rows[0].Should().Be(row0);
    }

    [Fact(Skip = "Importing a DC file is currently being implemented")]
    public void PassCountLastPassNotDbaseWithLatLong()
    {
      var requestedSubGrids = GetSubGrids(CoordType.LatLon, OutputTypes.PassCountLastPass, false,
        out ISiteModel siteModel, out CSVExportRequestArgument requestArgument, out Formatter formatter);

      var subGridProcessor = new CSVExportSubGridProcessor(siteModel, requestArgument, formatter);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      string row0 = @"2019/Jan/23 00:22:10.033,808,525.440m,376,730.880m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",0.2km/h,RTK Fixed,Medium (0.050m),?,1,1,?,?,?,?,?,?,?,0.000m,Neutral,Off,?";
      rows[0].Should().Be(row0);
    }

    [Fact]
    public void PassCountLastPassDBase()
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountLastPass, true,
        out ISiteModel siteModel, out CSVExportRequestArgument requestArgument, out Formatter formatter);

      var subGridProcessor = new CSVExportSubGridProcessor(siteModel, requestArgument, formatter);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      string row0 = @"2019/Jan/23 00:22:10.033,808525.440,376730.880,68.631,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",0.2,RTK Fixed,Medium (0.050),,1,1,,,,,,,,0.000,Neutral,Off,";
      rows[0].Should().Be(row0);
    }
    
    [Fact]
    public void VetaFinalPass()
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.VedaFinalPass, false,
        out ISiteModel siteModel, out CSVExportRequestArgument requestArgument, out Formatter formatter);

      var subGridProcessor = new CSVExportSubGridProcessor(siteModel, requestArgument, formatter);
      var rows = new List<string>();
      rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      string row0 = @"2019-Jan-23 00:22:10.033,808525.440m,376730.880m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",0.2km/h,RTK Fixed,Medium (0.050m),?,1,1,?,?,?,?,?,?,?,0.000m,Neutral,Off,?";
      rows[0].Should().Be(row0);
    }
    
    [Fact]
    public void PassCountAllPassesNotDBase()
    {
      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountAllPasses, false,
        out ISiteModel siteModel, out CSVExportRequestArgument requestArgument, out Formatter formatter);

      var subGridProcessor = new CSVExportSubGridProcessor(siteModel, requestArgument, formatter);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileAllPassesLeafSubgrid);
      rows.Count.Should().Be(384);
      string row0 = @"2019/Jan/23 00:22:10.033,808525.440m,376730.880m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",0.2km/h,RTK Fixed,Medium (0.050m),?,0,1,?,0.0,?,0.0,?,?,?,0.000m,Neutral,Off,?";
      rows[0].Should().Be(row0);
    }

    private List<IClientLeafSubGrid> GetSubGrids(CoordType coordType, OutputTypes outputType, bool isRawDataAsDBaseRequired, 
      out ISiteModel siteModel, out CSVExportRequestArgument requestArgument, out Formatter formatter)
    {
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive"), "*.tag").ToArray();
      siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out var _);
      var csvExportUserPreference = new CSVExportUserPreferences();
      requestArgument = new CSVExportRequestArgument
      (
        siteModel.ID, new FilterSet(new CombinedFilter()), "the filename",
        coordType, outputType, csvExportUserPreference, new List<CSVExportMappedMachine>(), false, false
      );

      formatter = new Formatter(csvExportUserPreference, outputType, isRawDataAsDBaseRequired);

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

