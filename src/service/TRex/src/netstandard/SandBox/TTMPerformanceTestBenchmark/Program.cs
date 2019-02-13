using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Factories;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Designs.TTM.Optimised;

namespace TTMPerformanceTestBenchmark
{
  public class TestBenchmark
  {
    private TrimbleTINModel readonly_tin;
    private TTMDesign design;

    [Benchmark]
    public void ScanAllElevationsOverGiantDesign()
    {
      var Patch = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

      int numPatches = 0;

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

      Console.WriteLine($"Number of patches = {numPatches}, cells = {numPatches * 1024}, probes = {design.NumTINProbeLookups}");
    }

    [Benchmark]
    public void LoadTheGiantDesign()
    {
      TrimbleTINModel readonly_tin2 = new TrimbleTINModel();
      readonly_tin2.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");
    }

    [Benchmark]
    public void ProfileAcrossTheGiantDesign()
    {
      var profile = design.ComputeProfile(new[]
      {
        new XYZ(design.Data.Header.MinimumEasting, design.Data.Header.MinimumNorthing, 0),
        new XYZ(design.Data.Header.MaximumEasting, design.Data.Header.MaximumNorthing, 0)
      }, 0.34);

      var profileDistance = MathUtilities.Hypot(profile.First().X - profile.Last().X, profile.First().Y - profile.Last().Y);
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<IOptimisedTTMProfilerFactory>(new OptimisedTTMProfilerFactory()))
        .Complete();

      design = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
      readonly_tin = new TrimbleTINModel();

      readonly_tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");

      //design.LoadFromFile(@"C:\Temp\141020 Finish Surface.ttm"); // 0.5 Mb
      //design.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm"); // 165Mb

      design.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
      DIBuilder.Eject();
      design = null;
      readonly_tin = null;
    }
  }

  class Program
  {
    static void Main(string[] args)
    {
      //var x = new TestBenchmark();
      //x.GlobalSetup();
      var _ = BenchmarkRunner.Run<TestBenchmark>();

      Console.ReadKey();
    }
  }
}
