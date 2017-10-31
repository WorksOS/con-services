using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICProfileCell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SVOICSummaryVolumesProfileCell;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionProfileExecutorTests : ExecutorTestsBase
  {
    //NOTE: Tests all use "last pass" to test logic as all interpolation is the same for each type of data (last pass, composite, CMV, temperature etc.)

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
    private const string WrongY_1 = "wrong y 1";
    private const string WrongY_2 = "wrong y 2";
    private const string WrongY2_1 = "wrong y2 1";
    private const string WrongY2_2 = "wrong y2 2";
    private const string WrongValue1 = "wrong value 1";
    private const string WrongValue2 = "wrong value 2";


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
        .AddTransient<IErrorCodesProvider, ErrorCodesProvider>()
        .AddTransient<ICompactionProfileResultHelper, CompactionProfileResultHelper>();
 
      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void ProfileExecutorSlicerNoResult()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var profileResultHelper = serviceProvider.GetRequiredService<ICompactionProfileResultHelper>();

      var raptorClient = new Mock<IASNodeClient>();
    
      raptorClient
        .Setup(x => x.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()))
        .Returns((MemoryStream)null);

      var settingsManager = new CompactionSettingsManager();
      var liftBuildSettings = settingsManager.CompactionLiftBuildSettings(CompactionProjectSettings.DefaultSettings);

      var request = CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(1234, Guid.Empty, ProductionDataType.Height, null, -1,
        null, null, null, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION, liftBuildSettings, false, null, null, null, null, null);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionProfileExecutor>(logger, raptorClient.Object, null, null, null, null, null, profileResultHelper);

      var result = executor.Process(request) as CompactionProfileResult<CompactionProfileDataResult>;
      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(0, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(0, result.results.Count, ResultsShouldBeEmpty);
    }

    #region Production Data Profile
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

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(2, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.MidPoint, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[1].y, WrongLastPassHeight2);
      
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
            CellLowestElev=597.2F, CellHighestElev=598.6F, CellFirstElev=597.2F, CellLastElev=596.0F, CellLastCompositeElev=597.3F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 0.05, InterceptLength= 0.05,
            CellLowestElev=597.4F, CellHighestElev=598.0F, CellFirstElev=597.1F, CellLastElev=597.0F, CellLastCompositeElev=597.8F,
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

      var result = MockGetProductionDataProfile(packager);
      
      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(3, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(596.5F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);
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

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(5, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(200F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                             (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result[3].x, WrongStation4);
      Assert.AreEqual(230F, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result[4].x, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[4].y, WrongLastPassHeight5);
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

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(7, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(200F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result[3].x, WrongStation4);
      Assert.AreEqual(220F, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result[4].cellType, WrongCellType5);
      expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result[4].x, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Edge, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station, result[5].x, WrongStation6);
      Assert.AreEqual(205F, result[5].y, WrongLastPassHeight6);

      Assert.AreEqual(ProfileCellType.MidPoint, result[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result[6].x, WrongStation7);
      Assert.AreEqual(packager.CellList[3].CellLastElev, result[6].y, WrongLastPassHeight7);
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

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(6, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      Assert.AreEqual(136F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station, result[2].x, WrongStation3);
      Assert.AreEqual(172F, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result[3].cellType, WrongCellType4);
      var expectedStation = packager.CellList[1].Station +
                        (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[3].x, WrongStation4);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.Edge, result[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[2].Station, result[4].x, WrongStation5);
      Assert.AreEqual(205F, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.MidPoint, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result[5].x, WrongStation6);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[5].y, WrongLastPassHeight6);
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

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(4, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      Assert.AreEqual(130F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station, result[2].x, WrongStation3);
      Assert.AreEqual(160F, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result[3].x, WrongStation4);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[3].y, WrongLastPassHeight4);
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

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(7, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      Assert.AreEqual(136F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station, result[2].x, WrongStation3);
      Assert.AreEqual(172F, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result[3].cellType, WrongCellType4);
      Assert.AreEqual(1.25, result[3].x, WrongStation4);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.Gap, result[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result[4].x, WrongStation5);
      Assert.AreEqual(199F, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Edge, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[2].Station, result[5].x, WrongStation6);
      Assert.AreEqual(217F, result[5].y, WrongLastPassHeight6); 

      Assert.AreEqual(ProfileCellType.MidPoint, result[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result[6].x, WrongStation7);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[6].y, WrongLastPassHeight7);
    }

    [TestMethod]
    public void ProfileExecutorWithGapFromNoData()
    {
      // O-----X------X------X------O becomes O-----X      X------X------O


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
          //No data = gap here
          new TICProfileCell
          {
            Station= 0.5, InterceptLength= 0.5,
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=VelociraptorConstants.NULL_SINGLE, CellLastCompositeElev=300F,
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
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=200F, CellLastCompositeElev=400F,
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
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=350F, CellLastCompositeElev=300F,
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

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(6, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(140F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[2].Station, result[2].x, WrongStation3);
      Assert.AreEqual(180F, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result[3].cellType, WrongCellType4);
      var expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result[3].x, WrongStation4);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.Edge, result[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[3].Station, result[4].x, WrongStation5);
      Assert.AreEqual(250F, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.MidPoint, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result[5].x, WrongStation6);
      Assert.AreEqual(packager.CellList[3].CellLastElev, result[5].y, WrongLastPassHeight6);
    }

    [TestMethod]
    public void ProfileExecutorStartInGapFromNoData()
    {
      // O-----X------X------X------O becomes O     X------X------X------O

      //Tests the extrapolation special case

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=VelociraptorConstants.NULL_SINGLE, CellLastCompositeElev=200F,
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
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=100F, CellLastCompositeElev=300F,
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
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=200F, CellLastCompositeElev=400F,
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
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=350F, CellLastCompositeElev=300F,
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

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(7, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.Gap, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(float.NaN, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(50F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result[3].x, WrongStation4);
      Assert.AreEqual(150F, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result[4].cellType, WrongCellType5);
      expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result[4].x, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Edge, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station, result[5].x, WrongStation6);
      Assert.AreEqual(250F, result[5].y, WrongLastPassHeight6);

      Assert.AreEqual(ProfileCellType.MidPoint, result[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result[6].x, WrongStation7);
      Assert.AreEqual(packager.CellList[3].CellLastElev, result[6].y, WrongLastPassHeight7);
    }

    [TestMethod]
    public void ProfileExecutorEndInGapFromNoData()
    {
      // O-----X------X------X------O becomes O-----X------X------X      O

      //Tests the extrapolation special case

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=70F, CellLastCompositeElev=200F,
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
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=100F, CellLastCompositeElev=300F,
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
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=200F, CellLastCompositeElev=400F,
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
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=VelociraptorConstants.NULL_SINGLE, CellLastCompositeElev=300F,
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

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(7, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(70F, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(90F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result[3].x, WrongStation4);
      Assert.AreEqual(150F, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result[4].cellType, WrongCellType5);
      expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result[4].x, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Gap, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station, result[5].x, WrongStation6);
      Assert.AreEqual(250F, result[5].y, WrongLastPassHeight6);

      Assert.AreEqual(ProfileCellType.Gap, result[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result[6].x, WrongStation7);
      Assert.AreEqual(float.NaN, result[6].y, WrongLastPassHeight7);
    }

    private List<CompactionDataPoint> MockGetProductionDataProfile(TICProfileCellListPackager packager)
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var profileResultHelper = serviceProvider.GetRequiredService<ICompactionProfileResultHelper>();

      var raptorClient = new Mock<IASNodeClient>();
      using (var ms = new MemoryStream())
      {
        packager.WriteToStream(ms);
        ms.Position = 0;
        raptorClient
          .Setup(x => x.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()))
          .Returns(ms);

        var settingsManager = new CompactionSettingsManager();
        var liftBuildSettings = settingsManager.CompactionLiftBuildSettings(CompactionProjectSettings.DefaultSettings);

        var request = CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(1234, Guid.Empty,
          ProductionDataType.Height, null, -1, null, null, null, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION, 
          liftBuildSettings, false, null, null, null, null, null);

        var executor = RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor>(logger, raptorClient.Object, null, null, null, null, null, profileResultHelper);
        var result = executor.Process(request) as CompactionProfileResult<CompactionProfileDataResult>;
        Assert.IsNotNull(result, ExecutorFailed);
        Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);

        //NOTE: Tests all use "last pass" to test logic as it's the same for each type of data (last pass, composite, CMV, temperature etc.)
        var lastPassResult = (from r in result.results where r.type == CompactionDataPoint.LAST_PASS select r).SingleOrDefault();
        Assert.IsNotNull(lastPassResult, ExecutorFailed);
        return lastPassResult.data;
      }
    }
    #endregion

    #region Summary Volumes Profile

    [TestMethod]
    [DataRow(VolumeCalcType.GroundToGround)]
    [DataRow(VolumeCalcType.GroundToDesign)]
    [DataRow(VolumeCalcType.DesignToGround)]
    public void ProfileExecutorSummaryVolumesSlicerInOneCell(VolumeCalcType calcType)
    {
      // O-----O

      TICSummaryVolumesProfileCellListPackager packager = new TICSummaryVolumesProfileCellListPackager
      {
        CellList = new TICSummaryVolumesProfileCellList
        {
          //public TICSummaryVolumesProfileCell(float FilteredHeight1, float FilteredHeight2, float DesignHeight, int OTGX, int OTGY, double AStation, double AInterceptLength)
          new TICSummaryVolumesProfileCell(45F, 48F, 46.5F, 0, 0, 0.0, 0.085)
        },
        GridDistanceBetweenProfilePoints = 1.234
      };

      var result = MockGetSummaryVolumesProfile(packager, calcType);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(2, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      float yExpected, y2Expected, valueExpected;
      GetExpectedValues(calcType, packager.CellList[0], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, result[0].y, WrongY_1);
      Assert.AreEqual(y2Expected, result[0].y2, WrongY2_1);
      Assert.AreEqual(valueExpected, result[0].value, WrongValue1);

      Assert.AreEqual(ProfileCellType.MidPoint, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      GetExpectedValues(calcType, packager.CellList[0], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, result[1].y, WrongY_2);
      Assert.AreEqual(y2Expected, result[1].y2, WrongY2_2);
      Assert.AreEqual(valueExpected, result[1].value, WrongValue2);
    }

    [TestMethod]
    [DataRow(VolumeCalcType.GroundToGround)]
    [DataRow(VolumeCalcType.GroundToDesign)]
    [DataRow(VolumeCalcType.DesignToGround)]
    public void ProfileExecutorSummaryVolumesSlicerNoGaps(VolumeCalcType calcType)
    {
      // O-----X------X------X------O

      TICSummaryVolumesProfileCellListPackager packager = new TICSummaryVolumesProfileCellListPackager
      {
        CellList = new TICSummaryVolumesProfileCellList
        {
          //public TICSummaryVolumesProfileCell(float FilteredHeight1, float FilteredHeight2, float DesignHeight, int OTGX, int OTGY, double AStation, double AInterceptLength)
          new TICSummaryVolumesProfileCell(200F, 300F, 100F, 0, 0, 0.0, 0.5),
          new TICSummaryVolumesProfileCell(350F, 450F, 250F, 0, 0, 0.5, 0.5),
          new TICSummaryVolumesProfileCell(290F, 390F, 190F, 0, 0, 1.0, 0.5),
          new TICSummaryVolumesProfileCell(335F, 435F, 235F, 0, 0, 1.5, 0.5)
        },
        GridDistanceBetweenProfilePoints = 1.234
      };
 
      var result = MockGetSummaryVolumesProfile(packager, calcType);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(7, result.Count, IncorrectNumberOfPoints);
      /*
      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      float yExpected, y2Expected, valueExpected;
      GetExpectedValues(calcType, packager.CellList[0], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, result[0].y, WrongY_1);
      Assert.AreEqual(y2Expected, result[0].y2, WrongY2_1);
      Assert.AreEqual(valueExpected, result[0].value, WrongValue1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(200F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result[3].x, WrongStation4);
      Assert.AreEqual(220F, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result[4].cellType, WrongCellType5);
      expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result[4].x, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Edge, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station, result[5].x, WrongStation6);
      Assert.AreEqual(205F, result[5].y, WrongLastPassHeight6);

      Assert.AreEqual(ProfileCellType.MidPoint, result[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result[6].x, WrongStation7);
      Assert.AreEqual(packager.CellList[3].CellLastElev, result[6].y, WrongLastPassHeight7);
      */
    }


    private void GetExpectedValues(VolumeCalcType calcType, TICSummaryVolumesProfileCell cell, out float yExpected, out float y2Expected, out float valueExpected)
    {
      yExpected = float.NaN;
      y2Expected = float.NaN;
      valueExpected = float.NaN;
      switch (calcType)
      {
        case VolumeCalcType.GroundToGround:
          yExpected = cell.LastCellPassElevation1;
          y2Expected = cell.LastCellPassElevation2;
          valueExpected = y2Expected - yExpected;
          break;
        case VolumeCalcType.GroundToDesign:
          yExpected = cell.LastCellPassElevation1;
          //y2 is NaN as it will be set layer using the design
          valueExpected = cell.DesignElevation - yExpected;
          break;
        case VolumeCalcType.DesignToGround:
          //y is NaN as it will be set layer using the design
          y2Expected = cell.LastCellPassElevation2;
          valueExpected = y2Expected - cell.DesignElevation;
          break;
      }
    }

    private List<CompactionDataPoint> MockGetSummaryVolumesProfile(TICSummaryVolumesProfileCellListPackager packager, VolumeCalcType calcType)
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var profileResultHelper = serviceProvider.GetRequiredService<ICompactionProfileResultHelper>();

      var raptorClient = new Mock<IASNodeClient>();
      using (var ms = new MemoryStream())
      {
        packager.WriteToStream(ms);
        ms.Position = 0;
        raptorClient
          .Setup(x => x.GetSummaryVolumesProfile(It.IsAny<ASNode.RequestSummaryVolumesProfile.RPC.TASNodeServiceRPCVerb_RequestSummaryVolumesProfile_Args>()))
          .Returns(ms);

        var settingsManager = new CompactionSettingsManager();
        var liftBuildSettings = settingsManager.CompactionLiftBuildSettings(CompactionProjectSettings.DefaultSettings);

        var request = CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(1234, Guid.Empty,
          ProductionDataType.Height, null, -1, null, null, null, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION,
          liftBuildSettings, false, null, null, null, calcType, null);

        var executor = RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor>(logger, raptorClient.Object, null, null, null, null, null, profileResultHelper);
        var result = executor.Process(request) as CompactionProfileResult<CompactionProfileDataResult>;
        Assert.IsNotNull(result, ExecutorFailed);
        Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);

        var sumVolResult = (from r in result.results where r.type == CompactionDataPoint.SUMMARY_VOLUMES select r).SingleOrDefault();
        Assert.IsNotNull(sumVolResult, ExecutorFailed);
        return sumVolResult.data;
      }
    }
    #endregion
  }
}