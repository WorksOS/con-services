using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICProfileCell;
using System;
using System.IO;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Executors;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionProfileExecutorTests
  {
    public IServiceProvider serviceProvider;

    [TestInitialize]
    public void InitTest()
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ErrorCodesProvider>();

      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void ProfileExecutorSlicerNoResult()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var raptorClient = new Mock<IASNodeClient>();
    
      raptorClient
        .Setup(x => x.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()))
        .Returns((MemoryStream)null);

      var request = ProfileProductionDataRequest.CreateProfileProductionData(1234, Guid.Empty, ProductionDataType.Height, null, -1,
        null, null, null, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION, null, false);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionProfileExecutor>(logger, raptorClient.Object);
      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
      
    }

    [TestMethod]
    public void ProfileExecutorSlicerInOneCell()
    {
      // O-----O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.085,
            CellLowestElev=597.292F, CellHighestElev=597.629F, CellFirstElev=597.294F, CellLastElev=597.367F, CellLastCompositeElev=597.367F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, "wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(2, result.cells.Count, "wrong number of cells");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[0].cellType, "wrong cellType 1");
      Assert.AreEqual(packager.CellList[0].Station, result.cells[0].station, "wrong station 1");
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.cells[0].lastPassHeight, "wrong lastPassHeight 1");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[1].cellType, "wrong cellType 2");
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result.cells[1].station, "wrong station 2");
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.cells[1].lastPassHeight, "wrong lastPassHeight 2");
      
    }


    [TestMethod]
    public void ProfileExecutorSlicesOneEdge()
    {
      // O-----X-------O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.05,
            CellLowestElev=597.2F, CellHighestElev=598.6F, CellFirstElev=597.2F, CellLastElev=596.3F, CellLastCompositeElev=597.3F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
          new TICProfileCell
          {
            Station= 0.05, InterceptLength= 0.07,
            CellLowestElev=597.4F, CellHighestElev=598.0F, CellFirstElev=597.1F, CellLastElev=596.7F, CellLastCompositeElev=597.8F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);
      
      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, "wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(3, result.cells.Count, "wrong number of cells");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[0].cellType, "wrong cellType 1");
      Assert.AreEqual(packager.CellList[0].Station, result.cells[0].station, "wrong station 1");
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.cells[0].lastPassHeight, "wrong lastPassHeight 1");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[1].cellType, "wrong cellType 2");
      Assert.AreEqual(packager.CellList[1].Station, result.cells[1].station, "wrong station 2");
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.cells[1].lastPassHeight, "wrong lastPassHeight 2");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[2].cellType, "wrong cellType 3");
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result.cells[2].station, "wrong station 3");
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.cells[2].lastPassHeight, "wrong lastPassHeight 3");
    }

    [TestMethod]
    public void ProfileExecutorSlicesTwoEdges()
    {
      // O-----X-------X-----O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
          new TICProfileCell
          {
            Station= 0.5, InterceptLength= 0.5,
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=250F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, "wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(5, result.cells.Count, "wrong number of cells");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[0].cellType, "wrong cellType 1");
      Assert.AreEqual(packager.CellList[0].Station, result.cells[0].station, "wrong station 1");
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.cells[0].lastPassHeight, "wrong lastPassHeight 1");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[1].cellType, "wrong cellType 2");
      Assert.AreEqual(packager.CellList[1].Station, result.cells[1].station, "wrong station 2");
      Assert.AreEqual(200, result.cells[1].lastPassHeight, "wrong lastPassHeight 2");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[2].cellType, "wrong cellType 3");
      var expectedStation = packager.CellList[1].Station +
                             (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result.cells[2].station, "wrong station 3");
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.cells[2].lastPassHeight, "wrong lastPassHeight 3");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[3].cellType, "wrong cellType 4");
      Assert.AreEqual(packager.CellList[2].Station, result.cells[3].station, "wrong station 4");
      Assert.AreEqual(230, result.cells[3].lastPassHeight, "wrong lastPassHeight 4");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[4].cellType, "wrong cellType 5");
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result.cells[4].station, "wrong station 5");
      Assert.AreEqual(packager.CellList[2].CellLastElev, result.cells[4].lastPassHeight, "wrong lastPassHeight 5");
    }


    [TestMethod]
    public void ProfileExecutorSlicerNoGaps()
    {
      // O-----X------X------X------O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
          new TICProfileCell
          {
            Station= 0.5, InterceptLength= 0.5,
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=250F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
          new TICProfileCell
          {
            Station= 1.5, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=235F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, "wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(7, result.cells.Count, "wrong number of cells");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[0].cellType, "wrong cellType 1");
      Assert.AreEqual(packager.CellList[0].Station, result.cells[0].station, "wrong station 1");
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.cells[0].lastPassHeight, "wrong lastPassHeight 1");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[1].cellType, "wrong cellType 2");
      Assert.AreEqual(packager.CellList[1].Station, result.cells[1].station, "wrong station 2");
      Assert.AreEqual(200, result.cells[1].lastPassHeight, "wrong lastPassHeight 2");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[2].cellType, "wrong cellType 3");
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result.cells[2].station, "wrong station 3");
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.cells[2].lastPassHeight, "wrong lastPassHeight 3");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[3].cellType, "wrong cellType 4");
      Assert.AreEqual(packager.CellList[2].Station, result.cells[3].station, "wrong station 4");
      Assert.AreEqual(220, result.cells[3].lastPassHeight, "wrong lastPassHeight 4");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[4].cellType, "wrong cellType 5");
      expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result.cells[4].station, "wrong station 5");
      Assert.AreEqual(packager.CellList[2].CellLastElev, result.cells[4].lastPassHeight, "wrong lastPassHeight 5");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[5].cellType, "wrong cellType 6");
      Assert.AreEqual(packager.CellList[3].Station, result.cells[5].station, "wrong station 6");
      Assert.AreEqual(205, result.cells[5].lastPassHeight, "wrong lastPassHeight 6");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[6].cellType, "wrong cellType 7");
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result.cells[6].station, "wrong station 7");
      Assert.AreEqual(packager.CellList[3].CellLastElev, result.cells[6].lastPassHeight, "wrong lastPassHeight 7");
    }


    [TestMethod]
    public void ProfileExecutorWithOneGap()
    {
      // O-----X      X------X------O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
           //Gap here
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
          new TICProfileCell
          {
            Station= 1.5, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=235F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, "wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(6, result.cells.Count, "wrong number of cells");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[0].cellType, "wrong cellType 1");
      Assert.AreEqual(packager.CellList[0].Station, result.cells[0].station, "wrong station 1");
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.cells[0].lastPassHeight, "wrong lastPassHeight 1");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Gap, result.cells[1].cellType, "wrong cellType 2");
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result.cells[1].station, "wrong station 2");
      Assert.AreEqual(136, result.cells[1].lastPassHeight, "wrong lastPassHeight 2");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[2].cellType, "wrong cellType 3");
      Assert.AreEqual(packager.CellList[1].Station, result.cells[2].station, "wrong station 3");
      Assert.AreEqual(172, result.cells[2].lastPassHeight, "wrong lastPassHeight 3");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[3].cellType, "wrong cellType 4");
      var expectedStation = packager.CellList[1].Station +
                        (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result.cells[3].station, "wrong station 4");
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.cells[3].lastPassHeight, "wrong lastPassHeight 4");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[4].cellType, "wrong cellType 5");
      Assert.AreEqual(packager.CellList[2].Station, result.cells[4].station, "wrong station 5");
      Assert.AreEqual(205, result.cells[4].lastPassHeight, "wrong lastPassHeight 5");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[5].cellType, "wrong cellType 6");
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result.cells[5].station, "wrong station 6");
      Assert.AreEqual(packager.CellList[2].CellLastElev, result.cells[5].lastPassHeight, "wrong lastPassHeight 6");
    }

    [TestMethod]
    public void ProfileExecutorWithOnlyAGap()
    {
      // O----X    X-------O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
          //Gap here
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, "wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(4, result.cells.Count, "wrong number of cells");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[0].cellType, "wrong cellType 1");
      Assert.AreEqual(packager.CellList[0].Station, result.cells[0].station, "wrong station 1");
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.cells[0].lastPassHeight, "wrong lastPassHeight 1");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Gap, result.cells[1].cellType, "wrong cellType 2");
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result.cells[1].station, "wrong station 2");
      Assert.AreEqual(130, result.cells[1].lastPassHeight, "wrong lastPassHeight 2");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[2].cellType, "wrong cellType 3");
      Assert.AreEqual(packager.CellList[1].Station, result.cells[2].station, "wrong station 3");
      Assert.AreEqual(160, result.cells[2].lastPassHeight, "wrong lastPassHeight 3");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[3].cellType, "wrong cellType 4");
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result.cells[3].station, "wrong station 4");
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.cells[3].lastPassHeight, "wrong lastPassHeight 4");
    }

    [TestMethod]
    public void ProfileExecutorWithTwoGaps()
    {
      // O-------X        X---------X      X-----O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
          //Gap here
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          },
          //Gap here
          new TICProfileCell
          {
            Station= 2.0, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=235F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, "wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(7, result.cells.Count, "wrong number of cells");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[0].cellType, "wrong cellType 1");
      Assert.AreEqual(packager.CellList[0].Station, result.cells[0].station, "wrong station 1");
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.cells[0].lastPassHeight, "wrong lastPassHeight 1");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Gap, result.cells[1].cellType, "wrong cellType 2");
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result.cells[1].station, "wrong station 2");
      Assert.AreEqual(136, result.cells[1].lastPassHeight, "wrong lastPassHeight 2");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[2].cellType, "wrong cellType 3");
      Assert.AreEqual(packager.CellList[1].Station, result.cells[2].station, "wrong station 3");
      Assert.AreEqual(172, result.cells[2].lastPassHeight, "wrong lastPassHeight 3");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[3].cellType, "wrong cellType 4");
      Assert.AreEqual(1.25, result.cells[3].station, "wrong station 4");
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.cells[3].lastPassHeight, "wrong lastPassHeight 4");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Gap, result.cells[4].cellType, "wrong cellType 5");
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result.cells[4].station, "wrong station 5");
      Assert.AreEqual(199, result.cells[4].lastPassHeight, "wrong lastPassHeight 5");

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.Edge, result.cells[5].cellType, "wrong cellType 6");
      Assert.AreEqual(packager.CellList[2].Station, result.cells[5].station, "wrong station 6");
      Assert.AreEqual(217, result.cells[5].lastPassHeight, "wrong lastPassHeight 6"); 

      Assert.AreEqual(CompactionProfileCell.ProfileCellType.MidPoint, result.cells[6].cellType, "wrong cellType 7");
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result.cells[6].station, "wrong station 7");
      Assert.AreEqual(packager.CellList[2].CellLastElev, result.cells[6].lastPassHeight, "wrong lastPassHeight 7");
    }

    private CompactionProfileResult MockGetProfile(TICProfileCellListPackager packager)
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var raptorClient = new Mock<IASNodeClient>();
      using (var ms = new MemoryStream())
      {
        packager.WriteToStream(ms);
        ms.Position = 0;
        raptorClient
          .Setup(x => x.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()))
          .Returns(ms);

        var request = ProfileProductionDataRequest.CreateProfileProductionData(1234, Guid.Empty,
          ProductionDataType.Height, null, -1,
          null, null, null, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION, null, false);

        var executor = RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor>(logger, raptorClient.Object);
        var result = executor.Process(request) as CompactionProfileResult;
        return result;
      }
    }

  }
}
