using System;
using ASNode.Volumes.RPC;
using BoundingExtents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.WebApi.Models.Report.Executors.Utilities;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class ResultConverterTests
  {
    [TestMethod]
    public void Should_return_expected_BoundingBox_result()
    {
      var simpleResult = new TASNodeSimpleVolumesResult
      {
        BoundingExtents = new T3DBoundingWorldExtent
        {
          MaxX = new Random().NextDouble(),
          MaxY = new Random().NextDouble(),
          MaxZ = new Random().NextDouble(),
          MinX = new Random().NextDouble(),
          MinY = new Random().NextDouble(),
          MinZ = new Random().NextDouble()
        },
        Cut = new Random().NextDouble(),
        CutArea = new Random().NextDouble(),
        Fill = new Random().NextDouble(),
        FillArea = new Random().NextDouble(),
        TotalCoverageArea = new Random().NextDouble()
      };

      var convertedResult = ResultConverter.SimpleVolumesResultToSummaryVolumesResult(simpleResult);

      Assert.AreEqual(simpleResult.BoundingExtents.MaxX, convertedResult.BoundingExtents.MaxX);
      Assert.AreEqual(simpleResult.BoundingExtents.MaxY, convertedResult.BoundingExtents.MaxX);
      Assert.AreEqual(simpleResult.BoundingExtents.MaxZ, convertedResult.BoundingExtents.MinX);
      Assert.AreEqual(simpleResult.BoundingExtents.MinX, convertedResult.BoundingExtents.MaxY);
      Assert.AreEqual(simpleResult.BoundingExtents.MinZ, convertedResult.BoundingExtents.MinX);
      Assert.AreEqual(simpleResult.BoundingExtents.MinY, convertedResult.BoundingExtents.MinZ);
      Assert.AreEqual(simpleResult.Cut, convertedResult.Cut);
      Assert.AreEqual(simpleResult.CutArea, convertedResult.CutArea);
      Assert.AreEqual(simpleResult.Fill, convertedResult.Fill);
      Assert.AreEqual(simpleResult.FillArea, convertedResult.FillArea);
      Assert.AreEqual(simpleResult.TotalCoverageArea, convertedResult.TotalCoverageArea);
    }
  }
}
