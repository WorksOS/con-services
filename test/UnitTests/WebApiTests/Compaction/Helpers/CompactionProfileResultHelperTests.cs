using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Filters.Caching;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class CompactionProfileResultHelperTests
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

      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    #region FindCutFillElevations tests
    [TestMethod]
    public void NoProdDataAndNoDesignProfile()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>();
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>();

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult);

      Assert.IsNull(slicerProfileResult.results, "Slicer profile should be null");
    }

    [TestMethod]
    public void NoDesignProfileShouldNotChangeProdData()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          results = new List<CompactionProfileCell>
          {
            new CompactionProfileCell {station = 0, cutFillHeight = float.NaN},
            new CompactionProfileCell {station = 1, cutFillHeight = float.NaN},
            new CompactionProfileCell {station = 2, cutFillHeight = float.NaN},
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>();

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult);

      Assert.AreEqual(3, slicerProfileResult.results.Count, "Wrong number of profile points");
      for (int i = 0; i < 3; i++)
      {
        Assert.IsTrue(float.IsNaN(slicerProfileResult.results[i].cutFillHeight), $"{i}: Wrong cut-fill height");
      }

    }

    [TestMethod]
    public void CellStationsOutsideDesign()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          results = new List<CompactionProfileCell>
          {
            new CompactionProfileCell {station = 0, cutFillHeight = float.NaN},
            new CompactionProfileCell {station = 1, cutFillHeight = float.NaN},
            new CompactionProfileCell {station = 2, cutFillHeight = float.NaN},
            new CompactionProfileCell {station = 3, cutFillHeight = float.NaN},
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          results = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex {station = 0.5, elevation = 10},
            new CompactionProfileVertex {station = 1.5, elevation = 20},
            new CompactionProfileVertex {station = 2.5, elevation = 40},
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult);

      Assert.AreEqual(4, slicerProfileResult.results.Count, "Wrong number of profile points");
      Assert.IsTrue(float.IsNaN(slicerProfileResult.results[0].cutFillHeight), "0: Wrong cut-fill height");
      Assert.AreEqual(15, slicerProfileResult.results[1].cutFillHeight, "1: Wrong cut-fill height");
      Assert.AreEqual(30, slicerProfileResult.results[2].cutFillHeight, "2: Wrong cut-fill height");
      Assert.IsTrue(float.IsNaN(slicerProfileResult.results[3].cutFillHeight), "3: Wrong cut-fill height");
    }


    [TestMethod]
    public void CellStationsMatchDesign()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          results = new List<CompactionProfileCell>
          {
            new CompactionProfileCell {station = 0, cutFillHeight = float.NaN},
            new CompactionProfileCell {station = 1, cutFillHeight = float.NaN},
            new CompactionProfileCell {station = 2, cutFillHeight = float.NaN},
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          results = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex {station = 0, elevation = 10},
            new CompactionProfileVertex {station = 0.5, elevation = 30},
            new CompactionProfileVertex {station = 1.0, elevation = 20},
            new CompactionProfileVertex {station = 1.5, elevation = 10},
            new CompactionProfileVertex {station = 2.0, elevation = 40},
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult);

      Assert.AreEqual(3, slicerProfileResult.results.Count, "Wrong number of profile points");
      Assert.AreEqual(10, slicerProfileResult.results[0].cutFillHeight, "0: Wrong cut-fill height");
      Assert.AreEqual(20, slicerProfileResult.results[1].cutFillHeight, "1: Wrong cut-fill height");
      Assert.AreEqual(40, slicerProfileResult.results[2].cutFillHeight, "2: Wrong cut-fill height");
    }

    [TestMethod]
    public void CellStationsWithNoDesignElevation()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          results = new List<CompactionProfileCell>
          {
            new CompactionProfileCell {station = 0, cutFillHeight = float.NaN},
            new CompactionProfileCell {station = 1, cutFillHeight = float.NaN},
            new CompactionProfileCell {station = 2, cutFillHeight = float.NaN},
            new CompactionProfileCell {station = 3, cutFillHeight = float.NaN},
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          results = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex {station = 0.0, elevation = 10},
            new CompactionProfileVertex {station = 0.75, elevation = float.NaN},
            new CompactionProfileVertex {station = 1.5, elevation = 40},
            new CompactionProfileVertex {station = 2.25, elevation = 10},
            new CompactionProfileVertex {station = 2.9, elevation = float.NaN},
            new CompactionProfileVertex {station = 3.5, elevation = 40}
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult);

      Assert.AreEqual(4, slicerProfileResult.results.Count, "Wrong number of profile points");
      Assert.AreEqual(10, slicerProfileResult.results[0].cutFillHeight, "0: Wrong cut-fill height");
      Assert.IsTrue(float.IsNaN(slicerProfileResult.results[1].cutFillHeight), "1: Wrong cut-fill height");
      Assert.AreEqual(20, slicerProfileResult.results[2].cutFillHeight, "2: Wrong cut-fill height");
      Assert.IsTrue(float.IsNaN(slicerProfileResult.results[3].cutFillHeight), "3: Wrong cut-fill height");
    }
    #endregion

    #region ConvertProfileResult production data profile tests

    [TestMethod]
    public void ConvertProductionDataProfileResultWithNull()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      Assert.ThrowsException<ServiceException>(
        () => helper.ConvertProfileResult((CompactionProfileResult<CompactionProfileCell>) null));
    }

    [TestMethod]
    public void ConvertProductionDataProfileResultWithNoProfile()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>();

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      Assert.ThrowsException<ServiceException>(() => helper.ConvertProfileResult(slicerProfileResult));
    }

    [TestMethod]
    public void ConvertProductionDataProfileResultSuccess()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileCell>
          {
            new CompactionProfileCell
            {
              cellType = ProfileCellType.MidPoint,
              station = 0,
              firstPassHeight = 0.5F,
              highestPassHeight = 0.8F,
              lastPassHeight = 0.7F,
              lowestPassHeight = 0.6F,
              lastCompositeHeight = 0.9F,
              designHeight = 0,
              cmv = 10.2F,
              cmvPercent = 89.6F,
              cmvHeight = 0.6F,
              mdpPercent = 78.3F,
              mdpHeight = 0.7F,
              temperature = 100.2F,
              temperatureHeight = 0.77F,
              topLayerPassCount = 5,
              cmvPercentChange = 4.5F,
              minSpeed = 5.9F,
              maxSpeed = 6.5F,
              speedHeight = 0.56F,
              cutFill = 0.06F,
              cutFillHeight = 0.8F,
              passCountIndex = ValueTargetType.NoData,
              temperatureIndex = ValueTargetType.AboveTarget,
              cmvIndex = ValueTargetType.NoData,
              mdpIndex = ValueTargetType.BelowTarget,
              speedIndex = ValueTargetType.OnTarget
            },
            new CompactionProfileCell
            {
              cellType = ProfileCellType.Edge,
              station = 1,
              firstPassHeight = 0.6F,
              highestPassHeight = 0.9F,
              lastPassHeight = 0.8F,
              lowestPassHeight = 0.7F,
              lastCompositeHeight = 0.4F,
              designHeight = 0.1F,
              cmv = 11.9F,
              cmvPercent = 90.1F,
              cmvHeight = 0.67F,
              mdpPercent = float.NaN,
              mdpHeight = float.NaN,
              temperature = 90.2F,
              temperatureHeight = 0.78F,
              topLayerPassCount = 6,
              cmvPercentChange = 3.9F,
              minSpeed = 8.1F,
              maxSpeed = 9.2F,
              speedHeight = 0.7F,
              cutFill = 0.15F,
              cutFillHeight = 0.9F,
              passCountIndex = ValueTargetType.AboveTarget,
              temperatureIndex = ValueTargetType.BelowTarget,
              cmvIndex = ValueTargetType.OnTarget,
              mdpIndex = ValueTargetType.NoData,
              speedIndex = ValueTargetType.NoData
            },
            new CompactionProfileCell
            {
              cellType = ProfileCellType.MidPoint,
              station = 1,
              firstPassHeight = 0.3F,
              highestPassHeight = 0.4F,
              lastPassHeight = 0.9F,
              lowestPassHeight = 0.7F,
              lastCompositeHeight = 0.99F,
              designHeight = float.NaN,
              cmv = 12.4F,
              cmvPercent = 85.1F,
              cmvHeight = float.NaN,
              mdpPercent = 45.6F,
              mdpHeight = 0.89F,
              temperature = 102.5F,
              temperatureHeight = 0.67F,
              topLayerPassCount = 8,
              cmvPercentChange = 2.1F,
              minSpeed = float.NaN,
              maxSpeed = float.NaN,
              speedHeight = 0.65F,
              cutFill = float.NaN,
              cutFillHeight = float.NaN,
              passCountIndex = ValueTargetType.NoData,
              temperatureIndex = ValueTargetType.BelowTarget,
              cmvIndex = ValueTargetType.AboveTarget,
              mdpIndex = ValueTargetType.OnTarget,
              speedIndex = ValueTargetType.NoData
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      var result = helper.ConvertProfileResult(slicerProfileResult);
      Assert.IsNotNull(result);
      Assert.AreEqual(slicerProfileResult.gridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      const int expectedCount = 14;
      Assert.AreEqual(expectedCount, result.results.Count, "Wrong number of profiles");
      string[] expectedTypes =
      {
        "firstPass", "highestPass", "lastPass", "lowestPass", "lastComposite",
        "cmvSummary", "cmvDetail", "cmvPercentChange", "mdpSummary", "temperatureSummary",
        "speedSummary", "passCountSummary", "passCountDetail", "cutFill"
      };
      for (int i = 0; i < expectedCount; i++)
      {
        ValidateList(expectedTypes[i], i, slicerProfileResult.results, result.results);
      }
    }

    private void ValidateList(string expectedType, int i, List<CompactionProfileCell> expectedList,
      List<CompactionProfileDataResult> actualList)
    {
      Assert.AreEqual(expectedType, actualList[i].type, $"{i}: Wrong type");
      Assert.AreEqual(expectedList.Count, actualList[i].data.Count, $"{i}: Wrong number of points");

      ValueTargetType? expectedValueType = null;
      float? expectedY2 = null;
      float? expectedValue2 = null;

      for (int j = 0; j < expectedList.Count; j++)
      {
        float expectedHeight = float.NaN;
        float expectedValue = float.NaN;
        switch (i)
        {
          case 0: //firstPass
            expectedHeight = expectedList[j].firstPassHeight;
            expectedValue = expectedList[j].firstPassHeight;
            break;
          case 1: //highestPass
            expectedHeight = expectedList[j].highestPassHeight;
            expectedValue = expectedList[j].highestPassHeight;
            break;
          case 2: //lastPass
            expectedHeight = expectedList[j].lastPassHeight;
            expectedValue = expectedList[j].lastPassHeight;
            break;
          case 3: //lowestPass
            expectedHeight = expectedList[j].lowestPassHeight;
            expectedValue = expectedList[j].lowestPassHeight;
            break;
          case 4: //lastComposite
            expectedHeight = expectedList[j].lastCompositeHeight;
            expectedValue = expectedList[j].lastCompositeHeight;
            break;
          case 5: //cmvSummary
            expectedHeight = expectedList[j].cmvHeight;
            expectedValue = expectedList[j].cmvPercent;
            expectedValueType = expectedList[j].cmvIndex;
            break;
          case 6: //cmvPercentChange
            expectedHeight = expectedList[j].cmvHeight;
            expectedValue = expectedList[j].cmv;
            break;
          case 7: //cmvDetail
            expectedHeight = expectedList[j].cmvHeight;
            expectedValue = expectedList[j].cmvPercentChange;
            break;
          case 8: //mdpSummary
            expectedHeight = expectedList[j].mdpHeight;
            expectedValue = expectedList[j].mdpPercent;
            expectedValueType = expectedList[j].mdpIndex;
            break;
          case 9: //temperatureSummary
            expectedHeight = expectedList[j].temperatureHeight;
            expectedValue = expectedList[j].temperature;
            expectedValueType = expectedList[j].temperatureIndex;
            break;
          case 10: //speedSummary
            expectedHeight = expectedList[j].speedHeight;
            expectedValue = expectedList[j].minSpeed;
            expectedValue2 = expectedList[j].maxSpeed;
            expectedValueType = expectedList[j].speedIndex;
            break;
          case 11: //passCountSummary
            expectedHeight = expectedList[j].lastPassHeight;
            expectedValue = expectedList[j].topLayerPassCount;
            expectedValueType = expectedList[j].passCountIndex;
            break;
          case 12: //passCountDetail
            expectedHeight = expectedList[j].lastPassHeight;
            expectedValue = expectedList[j].topLayerPassCount;
            break;
          case 13: //cutFill
            expectedHeight = expectedList[j].lastCompositeHeight;
            expectedValue = expectedList[j].cutFill;
            expectedY2 = expectedList[j].cutFillHeight;
            break;
        }
        ValidatePoint(expectedType, j, expectedList[j], actualList[i].data[j], expectedHeight, expectedValue,
          expectedValueType, expectedY2, expectedValue2);
      }
    }

    private void ValidatePoint(string expectedType, int j, CompactionProfileCell expectedCell,
      CompactionDataPoint actualResult, float expectedY, float expectedValue, ValueTargetType? expectedValueType,
      float? expectedY2, float? expectedValue2)
    {
      Assert.AreEqual(expectedCell.cellType, actualResult.cellType, $"{j}: {expectedType} Wrong cellType");
      Assert.AreEqual(expectedCell.station, actualResult.x, $"{j}: {expectedType} Wrong x");
      Assert.AreEqual(expectedY, actualResult.y, $"{j}: {expectedType} Wrong y");
      Assert.AreEqual(expectedValue, actualResult.value, $"{j}: {expectedType} Wrong value");
      Assert.AreEqual(expectedValueType, actualResult.valueType, $"{j}: {expectedType} Wrong valueType");
      Assert.AreEqual(expectedY2, actualResult.y2, $"{j}: {expectedType} Wrong y2");
      Assert.AreEqual(expectedValue2, actualResult.value2, $"{j}: {expectedType} Wrong value2");
    }
    #endregion

    #region ConvertProfileResult design profile tests
    [TestMethod]
    public void ConvertDesignProfileResultWithNull()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      Assert.ThrowsException<ServiceException>(
        () => helper.ConvertProfileResult((Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>>) null));
    }

    [TestMethod]
    public void ConvertDesignProfileResultWithNoProfile()
    {
      Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>> slicerProfileResults =
        new Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>>();

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      Assert.ThrowsException<ServiceException>(() => helper.ConvertProfileResult(slicerProfileResults));
    }

    [TestMethod]
    public void ConvertDesignProfileResultSuccess()
    {
      Guid designUid1 = Guid.NewGuid();
      Guid designUid2 = Guid.NewGuid();

      Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>> slicerProfileResults =
        new Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>>();
      slicerProfileResults.Add(designUid1, new CompactionProfileResult<CompactionProfileVertex>
      {
        gridDistanceBetweenProfilePoints = 12.34,
        results = new List<CompactionProfileVertex>
        {
          new CompactionProfileVertex{station = 1.2, elevation = 0.9F},
          new CompactionProfileVertex{station = 1.7, elevation = 1.3F},
          new CompactionProfileVertex{station = 2.8, elevation = 2.1F},
          new CompactionProfileVertex{station = 2.9, elevation = float.NaN},
        }
      });
      slicerProfileResults.Add(designUid2, new CompactionProfileResult<CompactionProfileVertex>
      {
        gridDistanceBetweenProfilePoints = 12.34,
        results = new List<CompactionProfileVertex>
        {
          new CompactionProfileVertex{station = 0.8, elevation = 2.1F},
          new CompactionProfileVertex{station = 1.1, elevation = 1.3F},
          new CompactionProfileVertex{station = 2.7, elevation = float.NaN},
          new CompactionProfileVertex{station = 3.8, elevation = 3.4F},
          new CompactionProfileVertex{station = 4.2, elevation = 2.3F},
        }
      });

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      var result = helper.ConvertProfileResult(slicerProfileResults);
      Assert.IsNotNull(result);
      Assert.AreEqual(slicerProfileResults.Values.First().gridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(slicerProfileResults.Keys.Count, result.results.Count, "Wrong number of profiles");
      int i = 0;
      foreach (var item in slicerProfileResults)
      {
        ValidateDesignProfile(item.Key, i, item.Value.results, result.results[i]);
        i++;
      }
    }

    [TestMethod]
    public void ConvertDesignProfileResultWithEmptyProfile()
    {
      Guid designUid1 = Guid.NewGuid();
      Guid designUid2 = Guid.NewGuid();

      Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>> slicerProfileResults =
        new Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>>();
      slicerProfileResults.Add(designUid1, new CompactionProfileResult<CompactionProfileVertex>
      {
        gridDistanceBetweenProfilePoints = 0,
        results = new List<CompactionProfileVertex>()
      });
      slicerProfileResults.Add(designUid2, new CompactionProfileResult<CompactionProfileVertex>
      {
        gridDistanceBetweenProfilePoints = 12.34,
        results = new List<CompactionProfileVertex>
        {
          new CompactionProfileVertex{station = 0.8, elevation = 2.1F},
          new CompactionProfileVertex{station = 1.1, elevation = 1.3F},
          new CompactionProfileVertex{station = 2.7, elevation = float.NaN},
          new CompactionProfileVertex{station = 3.8, elevation = 3.4F},
          new CompactionProfileVertex{station = 4.2, elevation = 2.3F},
        }
      });

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      var result = helper.ConvertProfileResult(slicerProfileResults);
      Assert.IsNotNull(result);
      Assert.AreEqual(12.34, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(slicerProfileResults.Keys.Count, result.results.Count, "Wrong number of profiles");
      int i = 0;
      foreach (var item in slicerProfileResults)
      {
        ValidateDesignProfile(item.Key, i, item.Value.results, result.results[i]);
        i++;
      }
    }


    private void ValidateDesignProfile(Guid expectedDesignUid, int j, List<CompactionProfileVertex>  expectedVertices, CompactionDesignProfileResult actualResult)
    {
      Assert.AreEqual(expectedDesignUid, actualResult.designFileUid, $"{j}: Wrong designUid");
      Assert.IsNotNull(actualResult.data, $"{j}: Should have some data returned");
      Assert.AreEqual(expectedVertices.Count, actualResult.data.Count, $"{j}: Wrong vertex count");
      for (int i = 0; i < expectedVertices.Count; i++)
      {
        Assert.AreEqual(expectedVertices[i].station, actualResult.data[i].station, $"{j}: Wrong station {i}");
        Assert.AreEqual(expectedVertices[i].elevation, actualResult.data[i].elevation, $"{j}: Wrong elevation {i}");
      }
    }
    #endregion

    #region RemoveRepeatedNoData tests

    [TestMethod]
    public void AllGapsShouldReturnEmptyList()
    {
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = "firstPass",
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = float.NaN
                }
              }             
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result);
      Assert.AreEqual(1, result.results.Count, "Wrong number of results");
      foreach (var item in result.results)
      {
        Assert.AreEqual(0, item.data.Count, $"{item.type}: Wrong number of data items");
      }
    }

    [TestMethod]
    public void NoRepeatedGapsNoShouldNotChangeData()
    {
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = "firstPass",
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = 1.2F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = 1.5F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Gap,
                  x = 3,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 4,
                  y = 1.3F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 5,
                  y = float.NaN
                }
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result);
      Assert.AreEqual(1, result.results.Count, "Wrong number of results");
      foreach (var item in result.results)
      {
        Assert.AreEqual(6, item.data.Count, $"{item.type}: Wrong number of data items");
        ValidateItem(0, item.data[0], ProfileCellType.Gap, 0, float.NaN);
        ValidateItem(1, item.data[1], ProfileCellType.Edge, 1, 1.2F);
        ValidateItem(2, item.data[2], ProfileCellType.MidPoint, 2, 1.5F);
        ValidateItem(3, item.data[3], ProfileCellType.Gap, 3, float.NaN);
        ValidateItem(4, item.data[4], ProfileCellType.Edge, 4, 1.3F);
        ValidateItem(5, item.data[5], ProfileCellType.Gap, 5, float.NaN);
      }
    }

    [TestMethod]
    public void NoGapsShouldNotChangeData()
    {
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = "firstPass",
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = 1.1F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = 1.2F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = 1.5F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 4,
                  y = 1.3F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 5,
                  y = 1.0F
                }
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result);
      Assert.AreEqual(1, result.results.Count, "Wrong number of results");
      foreach (var item in result.results)
      {
        Assert.AreEqual(5, item.data.Count, $"{item.type}: Wrong number of data items");
        ValidateItem(0, item.data[0], ProfileCellType.MidPoint, 0, 1.1F);
        ValidateItem(1, item.data[1], ProfileCellType.Edge, 1, 1.2F);
        ValidateItem(2, item.data[2], ProfileCellType.MidPoint, 2, 1.5F);
        ValidateItem(3, item.data[3], ProfileCellType.Edge, 4, 1.3F);
        ValidateItem(4, item.data[4], ProfileCellType.MidPoint, 5, 1.0F);
      }
    }

    [TestMethod]
    public void RepeatedGapsShouldBeRemoved()
    {
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = "firstPass",
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = 1.3F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = 1.2F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Gap,
                  x = 5,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 6,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 7,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 8,
                  y = 1.8F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 9,
                  y = 1.6F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 10,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 11,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Gap,
                  x = 12,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 13,
                  y = float.NaN
                }
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result);
      Assert.AreEqual(1, result.results.Count, "Wrong number of results");
      foreach (var item in result.results)
      {
        Assert.AreEqual(8, item.data.Count, $"{item.type}: Wrong number of data items");
        ValidateItem(0, item.data[0], ProfileCellType.Gap, 0, float.NaN);
        ValidateItem(1, item.data[1], ProfileCellType.Edge, 3, 1.3F);
        ValidateItem(2, item.data[2], ProfileCellType.MidPoint, 4, 1.2F);
        ValidateItem(3, item.data[3], ProfileCellType.Gap, 5, float.NaN);
        ValidateItem(4, item.data[4], ProfileCellType.Edge, 8, 1.8F);
        ValidateItem(5, item.data[5], ProfileCellType.MidPoint, 9, 1.6F);
        ValidateItem(6, item.data[6], ProfileCellType.Gap, 10, float.NaN);
        ValidateItem(7, item.data[7], ProfileCellType.Gap, 13, float.NaN);
      }
    }

   
    [TestMethod]
    public void RepeatedGapsForDifferentTypesShouldBeRemoved()
    {
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = "firstPass",
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = 1.2F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = 1.3F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = 1.2F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Gap,
                  x = 5,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 6,
                  y = 1.7F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 7,
                  y = 1.1F
                }
              }
            },
            new CompactionProfileDataResult
            {
              type = "cmvSummary",
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = "cmvSummary",
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "cmvSummary",
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = 1.8F
                },
                new CompactionDataPoint
                {
                  type = "cmvSummary",
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = 1.7F
                },
                new CompactionDataPoint
                {
                  type = "cmvSummary",
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "cmvSummary",
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "cmvSummary",
                  cellType = ProfileCellType.Gap,
                  x = 5,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "cmvSummary",
                  cellType = ProfileCellType.Edge,
                  x = 6,
                  y = 1.1F
                },
                new CompactionDataPoint
                {
                  type = "cmvSummary",
                  cellType = ProfileCellType.MidPoint,
                  x = 7,
                  y = 1.0F
                }
              }
            },
            new CompactionProfileDataResult
            {
              type = "passCountDetail",
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = "passCountDetail",
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = 1.4F
                },
                new CompactionDataPoint
                {
                  type = "passCountDetail",
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = 1.1F
                },
                new CompactionDataPoint
                {
                  type = "passCountDetail",
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = 1.4F
                },
                new CompactionDataPoint
                {
                  type = "passCountDetail",
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = 1.0F
                },
                new CompactionDataPoint
                {
                  type = "passCountDetail",
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = 1.5F
                },
                new CompactionDataPoint
                {
                  type = "passCountDetail",
                  cellType = ProfileCellType.Gap,
                  x = 5,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "passCountDetail",
                  cellType = ProfileCellType.Edge,
                  x = 6,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "passCountDetail",
                  cellType = ProfileCellType.MidPoint,
                  x = 7,
                  y = float.NaN
                }
              }
            },
            new CompactionProfileDataResult
            {
              type = "cutFill",
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = "cutFill",
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "cutFill",
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "cutFill",
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "cutFill",
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = 1.3F
                },
                new CompactionDataPoint
                {
                  type = "cutFill",
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = 1.2F
                },
                new CompactionDataPoint
                {
                  type = "cutFill",
                  cellType = ProfileCellType.Gap,
                  x = 5,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "cutFill",
                  cellType = ProfileCellType.Edge,
                  x = 6,
                  y = 1.7F
                },
                new CompactionDataPoint
                {
                  type = "cutFill",
                  cellType = ProfileCellType.MidPoint,
                  x = 7,
                  y = 1.6F
                }
              }
            },

          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result);
      Assert.AreEqual(4, result.results.Count, "Wrong number of results");
      for (int i=0; i<4; i++)
      {
        var item = result.results[i];
        switch (i)
        {
          case 0:
            Assert.AreEqual(7, item.data.Count, $"{i}: Wrong number of data items");
            Assert.AreEqual("firstPass", item.type, $"{i}: Wrong type");
            ValidateItem(0, item.data[0], ProfileCellType.MidPoint, 0, 1.2F);
            ValidateItem(1, item.data[1], ProfileCellType.Gap, 1, float.NaN);
            ValidateItem(2, item.data[2], ProfileCellType.Edge, 3, 1.3F);
            ValidateItem(3, item.data[3], ProfileCellType.MidPoint, 4, 1.2F);
            ValidateItem(4, item.data[4], ProfileCellType.Gap, 5, float.NaN);
            ValidateItem(5, item.data[5], ProfileCellType.Edge, 6, 1.7F);
            ValidateItem(6, item.data[6], ProfileCellType.MidPoint, 7, 1.1F);
            break;
          case 1:
            Assert.AreEqual(6, item.data.Count, $"{i}: Wrong number of data items");
            Assert.AreEqual("cmvSummary", item.type, $"{i}: Wrong type");
            ValidateItem(0, item.data[0], ProfileCellType.Gap, 0, float.NaN);
            ValidateItem(1, item.data[1], ProfileCellType.Edge, 1, 1.8F);
            ValidateItem(2, item.data[2], ProfileCellType.MidPoint, 2, 1.7F);
            ValidateItem(3, item.data[3], ProfileCellType.Gap, 3, float.NaN);
            ValidateItem(4, item.data[4], ProfileCellType.Edge, 6, 1.1F);
            ValidateItem(5, item.data[5], ProfileCellType.MidPoint, 7, 1.0F);
            break;
          case 2:
            Assert.AreEqual(7, item.data.Count, $"{i}: Wrong number of data items");
            Assert.AreEqual("passCountDetail", item.type, $"{i}: Wrong type");
            ValidateItem(0, item.data[0], ProfileCellType.MidPoint, 0, 1.4F);
            ValidateItem(1, item.data[1], ProfileCellType.Edge, 1, 1.1F);
            ValidateItem(2, item.data[2], ProfileCellType.MidPoint, 2, 1.4F);
            ValidateItem(3, item.data[3], ProfileCellType.Edge, 3, 1.0F);
            ValidateItem(4, item.data[4], ProfileCellType.MidPoint, 4, 1.5F);
            ValidateItem(5, item.data[5], ProfileCellType.Gap, 5, float.NaN);
            ValidateItem(6, item.data[6], ProfileCellType.Gap, 7, float.NaN);
            break;
          case 3:
            Assert.AreEqual(6, item.data.Count, $"{i}: Wrong number of data items");
            Assert.AreEqual("cutFill", item.type, $"{i}: Wrong type");
            ValidateItem(0, item.data[0], ProfileCellType.Gap, 0, float.NaN);
            ValidateItem(1, item.data[1], ProfileCellType.Edge, 3, 1.3F);
            ValidateItem(2, item.data[2], ProfileCellType.MidPoint, 4, 1.2F);
            ValidateItem(3, item.data[3], ProfileCellType.Gap, 5, float.NaN);
            ValidateItem(4, item.data[4], ProfileCellType.Edge, 6, 1.7F);
            ValidateItem(5, item.data[5], ProfileCellType.MidPoint, 7, 1.6F);
            break;
        }
      }
    }

    [TestMethod]
    public void RepeatedGapsWithNoDataShouldBeRemoved()
    {
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = "firstPass",
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = 2.0F,
                  value = 2.0F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = 1.8F,
                  value = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = float.NaN,
                  value = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = 1.3F,
                  value = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = float.NaN,
                  value = float.NaN
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.Edge,
                  x = 5,
                  y = 1.7F,
                  value = 1.7F
                },
                new CompactionDataPoint
                {
                  type = "firstPass",
                  cellType = ProfileCellType.MidPoint,
                  x = 6,
                  y = 1.2F,
                  value = 1.2F
                }
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result);
      Assert.AreEqual(1, result.results.Count, "Wrong number of results");
      foreach (var item in result.results)
      {
        Assert.AreEqual(4, item.data.Count, $"{item.type}: Wrong number of data items");
        ValidateItem(0, item.data[0], ProfileCellType.MidPoint, 0, 2.0F);
        ValidateItem(1, item.data[1], ProfileCellType.Gap, 1, 1.8F);
        ValidateItem(2, item.data[2], ProfileCellType.Edge, 5, 1.7F);
        ValidateItem(3, item.data[3], ProfileCellType.MidPoint, 6, 1.2F);
      }
    }

    private void ValidateItem(int i, CompactionDataPoint actual, ProfileCellType expectedCellType, double expectedStation, float expectedElevation)
    {
      Assert.AreEqual(expectedCellType, actual.cellType, $"{i}: Wrong cellType");
      Assert.AreEqual(expectedStation, actual.x, $"{i}: Wrong x");
      Assert.AreEqual(expectedElevation, actual.y, $"{i}: Wrong y");
    }
    #endregion

    #region AddSlicerEndPoints tests

    [TestMethod]
    public void DesignProfileWithSlicerEndPointsPresentShouldNotChange()
    {
      Guid designUid = Guid.NewGuid();
      var distance = 7.3;
      var v1 = new CompactionProfileVertex {station = 0.0, elevation = 2.1F};
      var v2 = new CompactionProfileVertex { station = 1.1, elevation = 1.3F };
      var v3 = new CompactionProfileVertex {station = 2.7, elevation = float.NaN};
      var v4 = new CompactionProfileVertex {station = 3.8, elevation = 3.4F};
      var v5 = new CompactionProfileVertex {station = distance, elevation = 2.3F};
      var expectedVertices = new List<CompactionProfileVertex>
      {
        v1,
        v2,
        v3,
        v4,
        v5
      };

      CompactionProfileResult<CompactionDesignProfileResult> result =
        new CompactionProfileResult<CompactionDesignProfileResult>
        {
          gridDistanceBetweenProfilePoints = distance,
          results = new List<CompactionDesignProfileResult>
          {
            new CompactionDesignProfileResult
            {
              designFileUid = designUid,
              data = new List<CompactionProfileVertex>(expectedVertices)
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      helper.AddSlicerEndPoints(result);
      Assert.AreEqual(distance, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(1, result.results.Count);
      Assert.AreEqual(designUid, result.results[0].designFileUid);
      Assert.AreEqual(expectedVertices.Count, result.results[0].data.Count);
      for (int i = 0; i < expectedVertices.Count; i++)
      {
        Assert.AreEqual(expectedVertices[i].station, result.results[0].data[i].station);
        Assert.AreEqual(expectedVertices[i].elevation, result.results[0].data[i].elevation);
      }
    }

    [TestMethod]
    public void DesignProfileWithoutSlicerEndPointsPresentShouldAddThem()
    {
      Guid designUid = Guid.NewGuid();
      var distance = 7.3;
      var v1 = new CompactionProfileVertex { station = 0.5, elevation = 2.1F };
      var v2 = new CompactionProfileVertex { station = 1.1, elevation = 1.3F };
      var v3 = new CompactionProfileVertex { station = 2.7, elevation = float.NaN };
      var v4 = new CompactionProfileVertex { station = 3.8, elevation = 3.4F };
      var v5 = new CompactionProfileVertex { station = 5.1, elevation = 2.3F };
      var expectedVertices = new List<CompactionProfileVertex>
      {
        v1,
        v2,
        v3,
        v4,
        v5
      };

      CompactionProfileResult<CompactionDesignProfileResult> result =
        new CompactionProfileResult<CompactionDesignProfileResult>
        {
          gridDistanceBetweenProfilePoints = distance,
          results = new List<CompactionDesignProfileResult>
          {
            new CompactionDesignProfileResult
            {
              designFileUid = designUid,
              data = new List<CompactionProfileVertex>(expectedVertices)
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      helper.AddSlicerEndPoints(result);
      Assert.AreEqual(distance, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(1, result.results.Count);
      Assert.AreEqual(designUid, result.results[0].designFileUid);
      Assert.AreEqual(expectedVertices.Count+2, result.results[0].data.Count);
      Assert.AreEqual(0, result.results[0].data[0].station);
      Assert.AreEqual(float.NaN, result.results[0].data[0].elevation);
      for (int i = 1; i < 6; i++)
      {
        Assert.AreEqual(expectedVertices[i-1].station, result.results[0].data[i].station);
        Assert.AreEqual(expectedVertices[i-1].elevation, result.results[0].data[i].elevation);
      }
      Assert.AreEqual(distance, result.results[0].data[6].station);
      Assert.AreEqual(float.NaN, result.results[0].data[6].elevation);
    }

    [TestMethod]
    public void DesignProfileWithNoPointsShouldNotChange()
    {
      Guid designUid = Guid.NewGuid();
      var distance = 0;
 
      CompactionProfileResult<CompactionDesignProfileResult> result =
        new CompactionProfileResult<CompactionDesignProfileResult>
        {
          gridDistanceBetweenProfilePoints = distance,
          results = new List<CompactionDesignProfileResult>
          {
            new CompactionDesignProfileResult
            {
              designFileUid = designUid,
              data = new List<CompactionProfileVertex>()
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      helper.AddSlicerEndPoints(result);
      Assert.AreEqual(distance, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(1, result.results.Count);
      Assert.AreEqual(designUid, result.results[0].designFileUid);
      Assert.AreEqual(0, result.results[0].data.Count);
    }
    #endregion
  }
}
