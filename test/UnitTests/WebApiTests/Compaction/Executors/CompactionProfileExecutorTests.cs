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
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionProfileExecutorTests : ExecutorTestsBase
  {
    private const string WrongCellType1 = "wrong cellType 1";
    private const string WrongCellType2 = "wrong cellType 2";
    private const string WrongCellType3 = "wrong cellType 3";
    private const string WrongCellType4 = "wrong cellType 4";
    private const string WrongCellType5 = "wrong cellType 5";
    private const string WrongCellType6 = "wrong cellType 6";
    private const string WrongCellType7 = "wrong cellType 7";
    private const string WrongStation1 = "wrong station 1";
    private const string WrongStation2 = "wrong station 2";
    private const string WrongStation3 = "wrong station 3";
    private const string WrongStation4 = "wrong station 4";
    private const string WrongStation5 = "wrong station 5";
    private const string WrongStation6 = "wrong station 6";
    private const string WrongStation7 = "wrong station 7";
    private const string WrongLastPassHeight1 = "wrong lastPassHeight 1";
    private const string WrongLastPassHeight2 = "wrong lastPassHeight 2";
    private const string WrongLastPassHeight3 = "wrong lastPassHeight 3";
    private const string WrongLastPassHeight4 = "wrong lastPassHeight 4";
    private const string WrongLastPassHeight5 = "wrong lastPassHeight 5";
    private const string WrongLastPassHeight6 = "wrong lastPassHeight 6";
    private const string WrongLastPassHeight7 = "wrong lastPassHeight 7";

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

      var request = CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(1234, Guid.Empty, ProductionDataType.Height, null, -1,
        null, null, null, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION, null, false, null);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionProfileExecutor<CompactionProfileCell>>(logger, raptorClient.Object);
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(2, result.points.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result.points[0].station, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.points[0].lastPassHeight, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result.points[1].station, WrongStation2);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.points[1].lastPassHeight, WrongLastPassHeight2);
      
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 0.05, InterceptLength= 0.07,
            CellLowestElev=597.4F, CellHighestElev=598.0F, CellFirstElev=597.1F, CellLastElev=596.7F, CellLastCompositeElev=597.8F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);
      
      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(3, result.points.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result.points[0].station, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.points[0].lastPassHeight, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result.points[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result.points[1].station, WrongStation2);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.points[1].lastPassHeight, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result.points[2].station, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.points[2].lastPassHeight, WrongLastPassHeight3);
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 0.5, InterceptLength= 0.5,
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=250F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(5, result.points.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result.points[0].station, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.points[0].lastPassHeight, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result.points[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result.points[1].station, WrongStation2);
      Assert.AreEqual(200, result.points[1].lastPassHeight, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                             (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result.points[2].station, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.points[2].lastPassHeight, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result.points[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result.points[3].station, WrongStation4);
      Assert.AreEqual(230, result.points[3].lastPassHeight, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result.points[4].station, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result.points[4].lastPassHeight, WrongLastPassHeight5);
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 0.5, InterceptLength= 0.5,
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=250F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.5, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=235F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(7, result.points.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result.points[0].station, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.points[0].lastPassHeight, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result.points[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result.points[1].station, WrongStation2);
      Assert.AreEqual(200, result.points[1].lastPassHeight, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result.points[2].station, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.points[2].lastPassHeight, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result.points[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result.points[3].station, WrongStation4);
      Assert.AreEqual(220, result.points[3].lastPassHeight, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[4].cellType, WrongCellType5);
      expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result.points[4].station, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result.points[4].lastPassHeight, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Edge, result.points[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station, result.points[5].station, WrongStation6);
      Assert.AreEqual(205, result.points[5].lastPassHeight, WrongLastPassHeight6);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result.points[6].station, WrongStation7);
      Assert.AreEqual(packager.CellList[3].CellLastElev, result.points[6].lastPassHeight, WrongLastPassHeight7);
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.5, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=235F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(6, result.points.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result.points[0].station, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.points[0].lastPassHeight, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result.points[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result.points[1].station, WrongStation2);
      Assert.AreEqual(136, result.points[1].lastPassHeight, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result.points[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station, result.points[2].station, WrongStation3);
      Assert.AreEqual(172, result.points[2].lastPassHeight, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[3].cellType, WrongCellType4);
      var expectedStation = packager.CellList[1].Station +
                        (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result.points[3].station, WrongStation4);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.points[3].lastPassHeight, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.Edge, result.points[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[2].Station, result.points[4].station, WrongStation5);
      Assert.AreEqual(205, result.points[4].lastPassHeight, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result.points[5].station, WrongStation6);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result.points[5].lastPassHeight, WrongLastPassHeight6);
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(4, result.points.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result.points[0].station, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.points[0].lastPassHeight, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result.points[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result.points[1].station, WrongStation2);
      Assert.AreEqual(130, result.points[1].lastPassHeight, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result.points[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station, result.points[2].station, WrongStation3);
      Assert.AreEqual(160, result.points[2].lastPassHeight, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result.points[3].station, WrongStation4);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.points[3].lastPassHeight, WrongLastPassHeight4);
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
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
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(7, result.points.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result.points[0].station, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result.points[0].lastPassHeight, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result.points[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result.points[1].station, WrongStation2);
      Assert.AreEqual(136, result.points[1].lastPassHeight, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result.points[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station, result.points[2].station, WrongStation3);
      Assert.AreEqual(172, result.points[2].lastPassHeight, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[3].cellType, WrongCellType4);
      Assert.AreEqual(1.25, result.points[3].station, WrongStation4);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result.points[3].lastPassHeight, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.Gap, result.points[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result.points[4].station, WrongStation5);
      Assert.AreEqual(199, result.points[4].lastPassHeight, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Edge, result.points[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[2].Station, result.points[5].station, WrongStation6);
      Assert.AreEqual(217, result.points[5].lastPassHeight, WrongLastPassHeight6); 

      Assert.AreEqual(ProfileCellType.MidPoint, result.points[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result.points[6].station, WrongStation7);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result.points[6].lastPassHeight, WrongLastPassHeight7);
    }

    private CompactionProfileResult<CompactionProfileCell> MockGetProfile(TICProfileCellListPackager packager)
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

        var request = CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(1234, Guid.Empty,
          ProductionDataType.Height, null, -1,
          null, null, null, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION, null, false, null);

        var executor = RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor<CompactionProfileCell>>(logger, raptorClient.Object);
        var result = executor.Process(request) as CompactionProfileResult<CompactionProfileCell>;
        return result;
      }
    }
  }
}