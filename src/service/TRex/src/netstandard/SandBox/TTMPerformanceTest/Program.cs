using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Factories;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Utilities;

namespace VSS.TRex.Sandbox.TTMPerformanceTest
{

    class Program
    {
      public static void ScanAllElevationsOverGiantDesign()
      {
        DateTime _start = DateTime.Now;
        TTMDesign design = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
       
        //design.LoadFromFile(@"C:\Temp\141020 Finish Surface.ttm"); // 0.5 Mb
        design.LoadFromFile(@"C:\Temp\161006 Stripped less PRB & AS.ttm");  //600Mb
        //design.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm"); // 165Mb
        TimeSpan loadTime = DateTime.Now - _start;

        Console.WriteLine($"Perf Test: Duration for file load and index preparation = {loadTime}");

        TimeSpan bestTime = TimeSpan.MaxValue;

        float[,] Patch = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];  

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
              if (design.InterpolateHeights(Patch, originX + x * cellSize, originY + y * cellSize, cellSize / SubGridTreeConsts.SubGridTreeDimension, 0))
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

      public static void ProfileAcrossTheGiantDesign()
      {
          TimeSpan bestTime = TimeSpan.MaxValue;

          Designs.TTM.Optimised.TrimbleTINModel readonly_tin = new Designs.TTM.Optimised.TrimbleTINModel();
          readonly_tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");

          TTMDesign design = new TTMDesign(0.34);
          design.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");

          for (int i = 0; i < 10; i++)
          { 
            DateTime _start = DateTime.Now;

            var profile = design.ComputeProfile(new[]
            {
              new XYZ(design.Data.Header.MinimumEasting, design.Data.Header.MinimumNorthing, 0),
              new XYZ(design.Data.Header.MaximumEasting, design.Data.Header.MaximumNorthing, 0)
            }, 0.34);

            var profileDistance = MathUtilities.Hypot(profile.First().X - profile.Last().X, profile.First().Y - profile.Last().Y);
            DateTime _end = DateTime.Now;
      
            if (_end - _start < bestTime)
            {
              bestTime = _end - _start;
              Console.WriteLine($"TIN profile (#{i}) with an {profileDistance:F3} meter line with {profile.Count} vertices in best time of {bestTime}");
          }
        }
    }

    static void Main(string[] args)
    {
      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<IOptimisedTTMProfilerFactory>(new OptimisedTTMProfilerFactory()))
        .Complete();

      //LoadTheGiantDesignALotOfTimes();
      //ScanAllElevationsOverGiantDesign();
      ProfileAcrossTheGiantDesign();

      Console.WriteLine("Completed operations - press a key");
      Console.ReadKey();
    }
  }
}
