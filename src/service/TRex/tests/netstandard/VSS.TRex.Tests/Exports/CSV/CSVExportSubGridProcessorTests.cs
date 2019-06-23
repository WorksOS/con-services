using System;
using System.IO;
using System.Linq;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using FluentAssertions;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using System.Collections.Generic;
using Tests.Common;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Types;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client.Types;
using GPSAccuracy = VSS.TRex.Types.GPSAccuracy;
using VSS.TRex.Common.Models;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportSubGridProcessorTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    public CSVExportSubGridProcessorTests()
    {
      DILoggingFixture.SetMaxExportRowsConfig(Consts.DEFAULT_MAX_EXPORT_ROWS);
    }

    [Fact]
    public void PassCountLastPassNotDbase()
    {
      DILoggingFixture.SetMaxExportRowsConfig(1000);

      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountLastPass, false,
        out CSVExportRequestArgument requestArgument, out ISiteModel _);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      rows[0].Should().Be(@"2019/Jan/23 00:22:09.993,808532.750m,376734.110m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",34.2km/h,RTK Fixed,unknown: 50 (0.256m),?,1,1,?,?,?,?,?,?,?,?,?,?,?");
    }

    [Fact(Skip = "Importing a DC file is currently being implemented")]
    public void PassCountLastPassNotDbaseWithLatLong()
    {
      DILoggingFixture.SetMaxExportRowsConfig(1000);

      var requestedSubGrids = GetSubGrids(CoordType.LatLon, OutputTypes.PassCountLastPass, false,
        out CSVExportRequestArgument requestArgument, out ISiteModel _);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      rows[0].Should().Be(@"2019/Jan/23 00:22:10.033,808,525.440m,376,730.880m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",34.2km/h,RTK Fixed,Medium (0.050m),?,1,1,?,?,?,?,?,?,?,?,?,?,?");
    }

    [Fact]
    public void PassCountLastPassDBase()
    {
      DILoggingFixture.SetMaxExportRowsConfig(1000);

      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountLastPass, true,
        out CSVExportRequestArgument requestArgument, out ISiteModel _);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(226);
      rows[0].Should().Be(@"2019/Jan/23 00:22:09.993,808532.750,376734.110,68.631,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",34.2,RTK Fixed,unknown: 50 (0.256),,1,1,,,,,,,,,,,");
    }

    [Fact]
    public void VetaFinalPass_NorthingEasting()
    {
      DILoggingFixture.SetMaxExportRowsConfig(100);

      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.VedaFinalPass, false,
        out CSVExportRequestArgument requestArgument, out ISiteModel _); 

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(100);
      rows[0].Should().Be(@"2019-Jan-23 00:22:09.993,808532.750m,376734.110m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",34.2km/h,RTK Fixed,unknown: 50 (0.256m),?,1,1,?,?,?,?,?,?,?,?,?,?,?");
    }

    [Fact(Skip = "Skip until coreX is available")]
    public void VetaFinalPass_LatLong()
    {
      DILoggingFixture.SetMaxExportRowsConfig(10);

      //todo when this test is enabled, you'll need to duplicate some of the "Dimensions2018-CaseMachine" tag files
      // from their outer location of TRex\tests\netstandard\TAGFiles.Tests\TestData\TAGFiles
      // to                           TRex\tests\netstandard\VSS.TRex.Tests\TestData\TAGFiles
      var requestedSubGrids = GetSubGrids(CoordType.LatLon, OutputTypes.VedaFinalPass, false,
        out CSVExportRequestArgument requestArgument, out ISiteModel siteModel, "Dimensions2018-CaseMachine");

      DITAGFileAndSubGridRequestsFixture.AddCSIBToSiteModel(ref siteModel, TestCommonConsts.DIMENSIONS_2012_DC_CSIB);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      rows.Count.Should().Be(8);
      rows[0].Should().Be(@"2012-Nov-05 19:44:47.459,1174.870m,2869.770m,599.220m,2,0,Trimble Road with Ref Surfaces v2,""Unknown"",7.4km/h,Not_Applicable,Fine (0.000m),?,2,1,?,?,?,?,?,?,?,?,?,?,?");
    }

    [Fact]
    public void PassCountAllPassesNotDBase()
    {
      DILoggingFixture.SetMaxExportRowsConfig(1000);

      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountAllPasses, false,
        out CSVExportRequestArgument requestArgument, out ISiteModel _);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);
      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileAllPassesLeafSubgrid);
      rows.Count.Should().Be(384);
      rows[0].Should().Be(@"2019/Jan/23 00:22:09.993,808532.750m,376734.110m,68.631m,1,0,Site Extended (Preliminary) 180302 EW,""Unknown"",34.2km/h,RTK Fixed,unknown: 50 (0.256m),?,0,1,?,?,?,?,?,?,?,?,?,?,?");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(382)]
    [InlineData(384)]
    [InlineData(385)]
    [InlineData(400)]
    public void RowCountLimit_AllPasses(int maxExportRows)
    {
      DILoggingFixture.SetMaxExportRowsConfig(maxExportRows);

      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountAllPasses, false,
        out CSVExportRequestArgument requestArgument, out ISiteModel _);

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
      DILoggingFixture.SetMaxExportRowsConfig(maxExportRows);

      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.PassCountAllPasses, false,
        out CSVExportRequestArgument requestArgument, out ISiteModel _);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);

      var rows = new List<string>();
      foreach (var subGrid in requestedSubGrids)
        rows.AddRange(subGridProcessor.ProcessSubGrid(subGrid as ClientCellProfileAllPassesLeafSubgrid));

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
      DILoggingFixture.SetMaxExportRowsConfig(maxExportRows);

      var requestedSubGrids = GetSubGrids(CoordType.Northeast, OutputTypes.VedaFinalPass, false,
        out CSVExportRequestArgument requestArgument, out ISiteModel _);

      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);

      var rows = subGridProcessor.ProcessSubGrid(requestedSubGrids[0] as ClientCellProfileLeafSubgrid);
      if (maxExportRows <= 226) // 226 is the number of rows we could potentially get from this set
        rows.Count.Should().Be(maxExportRows);
      else
        rows.Count.Should().BeLessOrEqualTo(maxExportRows);
    }

    [Fact]
    public void CellProfile_FullDataContingent()
    {
      DILoggingFixture.SetMaxExportRowsConfig(1);

      SetupSiteAndRequestArgument(CoordType.Northeast, OutputTypes.VedaFinalPass, false, "ElevationMappingMode-KettlewellDrive",
        out CSVExportRequestArgument requestArgument);
      requestArgument.MappedMachines = new List<CSVExportMappedMachine>() {new CSVExportMappedMachine() {InternalSiteModelMachineIndex = 2, Name = "The machine Name"}};

      var clientGrid = SetupProfileSampleCell();
      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);

      var rows = subGridProcessor.ProcessSubGrid(clientGrid);
      rows.Count.Should().Be(1);
      rows[0].Should().Be(@"2019-Mar-14 23:45:00.000,1277752770.730m,1277752770.730m,6509.000m,1,34,Full Site (Kettlewell Drive 171219) Earthworks,""The machine Name"",1,188.0km/h,Float,Coarse (0.300m),5,3,2,90.1,85.0,13.0,11.0,11.1,96.0Hz,45.60mm,45.000m,Forward_2,On,104.0°C");
    }


    [Fact]
    public void CellPasses_IncludesHalfPasses()
    {
      DILoggingFixture.SetMaxExportRowsConfig(10);

      SetupSiteAndRequestArgument(CoordType.Northeast, OutputTypes.PassCountAllPasses, false, "ElevationMappingMode-KettlewellDrive",
        out CSVExportRequestArgument requestArgument);
      requestArgument.MappedMachines = new List<CSVExportMappedMachine>() { new CSVExportMappedMachine() { InternalSiteModelMachineIndex = 2, Name = "The machine Name" } };

      var clientGrid = SetupProfileAllPassesSampleCell();
      var subGridProcessor = new CSVExportSubGridProcessor(requestArgument);

      var rows = subGridProcessor.ProcessSubGrid(clientGrid);
      rows.Count.Should().Be(3);
      rows[0].Should().Be(@"2019/Apr/15 00:00:00.000,1277752770.730m,1277752770.730m,555.000m,1,0,?,""The machine Name"",23.9km/h,Old Position,Fine (0.000m),5,3,5,77.7,0.8,0.0,0.0,0.0,0.0Hz,0.00mm,0.000m,Neutral,Off,0.0°C");
      rows[1].Should().Be(@"2019/May/16 00:00:00.000,1277752770.730m,1277752770.730m,20.000m,1,0,?,""Unknown"",1.6km/h,Old Position,Fine (0.000m),2,1,1,4.4,6.6,0.0,0.0,0.0,0.0Hz,0.00mm,0.000m,Neutral,Off,0.0°C");
      rows[2].Should().Be(@"2019/Apr/15 00:00:00.000,1277752770.730m,1277752770.730m,565.000m,1,0,?,""The machine Name"",27.9km/h,Old Position,Fine (0.000m),5,3,5,33.3,8.8,0.0,0.0,0.0,0.0Hz,0.00mm,0.000m,Neutral,Off,0.0°C");
    }

    private ClientCellProfileLeafSubgrid SetupProfileSampleCell()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellProfile) as ClientCellProfileLeafSubgrid;
      clientGrid.Should().NotBe(null);
      clientGrid.Cells[0, 0] = new ClientCellProfileLeafSubgridRecord
      {
        LastPassTime = DateTime.SpecifyKind(new DateTime(2019, 3, 14, 23, 45, 00), DateTimeKind.Utc),
        Height = 6509,
        PassCount = 1,
        LastPassValidRadioLatency = 34,
        EventDesignNameID = 1,
        InternalSiteModelMachineIndex = 2,
        MachineSpeed = 33000,
        LastPassValidGPSMode = GPSMode.Float,
        GPSAccuracy = GPSAccuracy.Coarse,
        GPSTolerance = 300,
        TargetPassCount = 5,
        TotalWholePasses = 3,
        LayersCount = 2,
        LastPassValidCCV = 901,
        TargetCCV = 850,
        LastPassValidMDP = 130,
        TargetMDP = 110,
        LastPassValidRMV = 111,
        LastPassValidFreq = 960,
        LastPassValidAmp = 4560,
        TargetThickness = 45,
        EventMachineGear = MachineGear.Forward2,
        EventVibrationState = VibrationState.On,
        LastPassValidTemperature = 1040
      };
      return clientGrid;
    }

    // 1 cell with 5 passes, 2-halves and one whole
    private ClientCellProfileAllPassesLeafSubgrid SetupProfileAllPassesSampleCell()
    {
      var firstHalfPassTime = DateTime.SpecifyKind(new DateTime(2019, 3, 14), DateTimeKind.Utc);
      var secondHalfPassTime = DateTime.SpecifyKind(new DateTime(2019, 4, 15), DateTimeKind.Utc);
      var fullPassTime = DateTime.SpecifyKind(new DateTime(2019, 5, 16), DateTimeKind.Utc);
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellPasses) as ClientCellProfileAllPassesLeafSubgrid;
      clientGrid.Should().NotBe(null);
      clientGrid.Cells[0, 0] = new ClientCellProfileAllPassesLeafSubgridRecord()
      {
        TotalPasses = 5,
        CellPasses = new[]
        {
          new ClientCellProfileLeafSubgridRecord
          {
            HalfPass = true,
            LastPassTime = firstHalfPassTime,
            Height = 10,
            PassCount = 1,
            InternalSiteModelMachineIndex = 2,
            MachineSpeed = 33,
            TargetPassCount = 4,
            TotalWholePasses = 1,
            LayersCount = 2,
            LastPassValidCCV = 666,
            TargetCCV = 5
          },
          new ClientCellProfileLeafSubgridRecord
          {
            HalfPass = true,
            LastPassTime = secondHalfPassTime,
            Height = 555,
            PassCount = 1,
            InternalSiteModelMachineIndex = 2,
            MachineSpeed = 664,
            TargetPassCount = 5,
            TotalWholePasses = 3,
            LayersCount = 5,
            LastPassValidCCV = 777,
            TargetCCV = 8
          },
          new ClientCellProfileLeafSubgridRecord
          {
            HalfPass = false,
            LastPassTime = fullPassTime,
            Height = 20,
            PassCount = 1,
            InternalSiteModelMachineIndex = 3,
            MachineSpeed = 44,
            TargetPassCount = 2,
            TotalWholePasses = 1,
            LayersCount = 1,
            LastPassValidCCV = 44,
            TargetCCV = 66
          },
          new ClientCellProfileLeafSubgridRecord
          {
            HalfPass = true,
            LastPassTime = firstHalfPassTime,
            Height = 10,
            PassCount = 1,
            InternalSiteModelMachineIndex = 2,
            MachineSpeed = 33,
            TargetPassCount = 4,
            TotalWholePasses = 1,
            LayersCount = 2,
            LastPassValidCCV = 666,
            TargetCCV = 5
          },
          new ClientCellProfileLeafSubgridRecord
          {
            HalfPass = true,
            LastPassTime = secondHalfPassTime,
            Height = 565,
            PassCount = 1,
            InternalSiteModelMachineIndex = 2,
            MachineSpeed = 776,
            TargetPassCount = 5,
            TotalWholePasses = 3,
            LayersCount = 5,
            LastPassValidCCV = 333,
            TargetCCV = 88
          }
        }
      };
      return clientGrid;
    }

    private ISiteModel SetupSiteAndRequestArgument(CoordType coordType, OutputTypes outputType, bool isRawDataAsDBaseRequired, string tagFileDirectory,
      out CSVExportRequestArgument requestArgument)
    {
      // tagFileDirectory: "Dimensions2018-CaseMachine" - extents match the CSIB constant
      //                   "ElevationMappingMode-KettlewellDrive"
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", tagFileDirectory), "*.tag").ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out var _);
      var csvExportUserPreference = new CSVExportUserPreferences();
      requestArgument = new CSVExportRequestArgument
      (
        siteModel.ID, new FilterSet(new CombinedFilter()), "the filename",
        coordType, outputType, csvExportUserPreference, new List<CSVExportMappedMachine>(), false, isRawDataAsDBaseRequired
      );
      return siteModel;
    }

    private List<IClientLeafSubGrid> GetSubGrids(CoordType coordType, OutputTypes outputType, bool isRawDataAsDBaseRequired, 
      out CSVExportRequestArgument requestArgument, out ISiteModel siteModel, string tagFileDirectory = "ElevationMappingMode-KettlewellDrive")
    {
      siteModel = SetupSiteAndRequestArgument(coordType, outputType, isRawDataAsDBaseRequired, tagFileDirectory, out requestArgument);

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
      requestedSubGrids.Count.Should().Be(tagFileDirectory == "ElevationMappingMode-KettlewellDrive" ? 18 : 9);
      return requestedSubGrids;
    }
  }
}

