using System;
using VSS.TRex.Designs;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Sandbox.TTMPerformanceTest
{

    class Program
    {
      public static void ScanAllElevationsOverGiantDesign()
      {
        DateTime _start = DateTime.Now;
        TTMDesign design = new TTMDesign(SubGridTree.DefaultCellSize);
        design.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");
        TimeSpan loadTime = DateTime.Now - _start;

        Console.WriteLine($"Perf Test: Duration for file load and index preparation = {loadTime}");

        TimeSpan bestTime = TimeSpan.MaxValue;

        float[,] Patch = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];  

        for (int i = 0; i < 100; i++)
        {
          int numPatches = 0;
          _start = DateTime.Now;

          design.NumTINProbeLookups = 0;
          design.NumTINHeightRequests = 0;
          design.NumNonNullProbeResults = 0;
          design.SpatialIndexOptimised.ScanAllSubGrids(leaf =>
          {
            double cellSize = leaf.Owner.CellSize;
            leaf.CalculateWorldOrigin(out double originX, out double originY);

            leaf.ForEach((x, y) =>
            {
              if (design.InterpolateHeights(Patch, originX + x * cellSize, originY + y * cellSize, cellSize / SubGridTree.SubGridTreeDimension, 0))
                numPatches++;
            });

            return true;
          });

          TimeSpan lookupTime = DateTime.Now - _start;

          if (lookupTime < bestTime)
          {
            bestTime = lookupTime;
            Console.WriteLine($"Perf Test: Run {i}: Duration for {numPatches} lookups = {lookupTime}, probes = {design.NumTINProbeLookups}, Triangle evaluations = {design.NumTINHeightRequests}, non-null results = {design.NumNonNullProbeResults}");
          }
        }
      }

      public static void LoadTheGiantDesignALotOfTimes()
      {
        TimeSpan bestTime = TimeSpan.MaxValue;

        for (int i = 0; i < 1000; i++)
        {
          Designs.TTM.Optimised.TrimbleTINModel readonly_tin = new Designs.TTM.Optimised.TrimbleTINModel();

          DateTime _start = DateTime.Now;
          readonly_tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");
          DateTime _end = DateTime.Now;

          if (_end - _start < bestTime)
          {
            bestTime = _end - _start;
            Console.WriteLine($"Readonly tin read (#{i}) in best time of {bestTime}");
          }
        }

        for (int i = 0; i < 0; i++)
        {
          Designs.TTM.TrimbleTINModel readwrite_tin = new Designs.TTM.TrimbleTINModel();
          DateTime _start = DateTime.Now;
          readwrite_tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");
          DateTime _end = DateTime.Now;
          Console.WriteLine($"Read/write tin read in {_end - _start}");
        }     
    }

    static void Main(string[] args)
    {
      DIBuilder.New().AddLogging().Complete();

      //LoadTheGiantDesignALotOfTimes();
      ScanAllElevationsOverGiantDesign();

      Console.ReadKey();
    }
  }
}
