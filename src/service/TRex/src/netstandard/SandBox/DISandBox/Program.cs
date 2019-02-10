using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace DISandBox
{
  public interface IClass
  {
    void Test();
  }

  public class AClass : IClass
  {
    public void Test()
    {
      Console.WriteLine("Test");
    }
  }

  public class DIPerfTest
  {
    [GlobalSetup]
    public void GlobalSetup()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IClass>(new AClass())).Complete();
    }

    [Benchmark]
    public void ExtractDIElement()
    {
      var di = DIContext.Obtain<IClass>();
      if (di == null)
        Console.WriteLine("Its null!!!");
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
      DIBuilder.Eject();
    }
  }

  class Program
  {
    public void ExecuteTestAsSimpleLoop()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IClass>(new AClass())).Complete();

      var count = 1_000_000_000;
      var sw = Stopwatch.StartNew();

      for (int i = 0; i < count; i++)
      {
        var di = DIContext.Obtain<IClass>();
      }

      var elapsed = sw.Elapsed;

      Console.WriteLine($"Time to obtain {count} instances from DI: {elapsed}, {elapsed.Ticks / (1.0 * count)} ticks per invocation");
    }

    static void Main(string[] args)
    {
      var summary = BenchmarkRunner.Run<DIPerfTest>();

      Console.ReadKey();
    }
  }
}
