using System;
using System.Buffers;
using System.IO;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.IO;
using VSS.TRex.IO.Helpers;

namespace ArrayPoolBenchmark
{
  public class Program
  {
    public class ArrayPool_T_BenchMark
    {
      private ArrayPool<byte> _arrayPool;
      private IGenericArrayPoolCaches<byte> _genericArrayPoolIntf;
      private GenericArrayPoolCaches<byte> _genericArrayPoolObj;

      [GlobalSetup]
      public void GlobalSetup()
      {
        _arrayPool = ArrayPool<byte>.Shared;

        DIBuilder.New().AddLogging()
          .Add(x => x.AddSingleton<IGenericArrayPoolCaches<byte>>(new GenericArrayPoolCaches<byte>())).Build()
          .Complete();

        _genericArrayPoolIntf = GenericArrayPoolCacheHelper<byte>.Caches();
        _genericArrayPoolObj = GenericArrayPoolCacheHelper<byte>.Caches() as GenericArrayPoolCaches<byte>;
      }
      
      [Benchmark]
      public void ArrayPool_T_RentAndReturn_SingleItem()
      {
        for (int i = 0; i < 20; i++)
        {
          byte[] b = _arrayPool.Rent(1 << i + 1);

          _arrayPool.Return(b);
        }
      }

      [Benchmark]
      public void GenericArrayPool_T_RentAndReturn_SingleItem()
      {
        for (int i = 0; i < 20; i++)
        {
          byte[] b = GenericArrayPoolCacheHelper<byte>.Caches().Rent(1 << i + 1);

          GenericArrayPoolCacheHelper<byte>.Caches().Return(b);
        }
      }

      [Benchmark]
      public void GenericArrayPool_T_RentAndReturn_CachedObjectImpl_SimgleItem()
      {
        for (int i = 0; i < 20; i++)
        {
          byte[] b = _genericArrayPoolObj.Rent(1 << i + 1);

          _genericArrayPoolObj.Return(b);
        }
      }

      [Benchmark]
      public void ArrayPool_T_RentAndReturn_MultipleItems()
      {
        for (int i = 0; i < 20; i++)
        {
          byte[] b1 = _arrayPool.Rent(1 << i + 1);
          byte[] b2 = _arrayPool.Rent(1 << i + 1);

          _arrayPool.Return(b1);
          _arrayPool.Return(b2);
        }
      }

      [Benchmark]
      public void GenericArrayPool_T_RentAndReturn_MultipleItems()
      {
        for (int i = 0; i < 20; i++)
        {
          byte[] b1 = GenericArrayPoolCacheHelper<byte>.Caches().Rent(1 << i + 1);
          byte[] b2 = GenericArrayPoolCacheHelper<byte>.Caches().Rent(1 << i + 1);

          GenericArrayPoolCacheHelper<byte>.Caches().Return(b1);
          GenericArrayPoolCacheHelper<byte>.Caches().Return(b2);
        }
      }

      [Benchmark]
      public void GenericArrayPool_T_RentAndReturn_CachedObjectImpl_MultipleItems()
      {
        for (int i = 0; i < 20; i++)
        {
          byte[] b1 = _genericArrayPoolObj.Rent(1 << i + 1);
          byte[] b2 = _genericArrayPoolObj.Rent(1 << i + 1);

          _genericArrayPoolObj.Return(b1);
          _genericArrayPoolObj.Return(b2);
        }
      }

      private static object _lock = new object();

      [Benchmark]
      public void LoopWithLock()
      {
        int sum = 0;
        for (int i = 1; i < 2; i++)
        {
          lock (_lock)
          {
            sum += i;
          }
        }

        if (sum == 0)
          throw new Exception();
      }

      [Benchmark]
      public void LoopWithOutLock()
      {
        int sum = 0;
        for (int i = 1; i < 2; i++)
        {
          sum += i;
        }

        if (sum == 0)
          throw new Exception();
      }

      [Benchmark]
      public void LoopWithInterlockedAdd()
      {
        int sum = 0;
        for (int i = 1; i < 2; i++)
        {
          Interlocked.Add(ref sum, i);
        }

        if (sum == 0)
          throw new Exception();
      }

      [GlobalCleanup]
      public void GlobalCleanup()
      {
      }
    }

    static void Main(string[] args)
    {
      Console.WriteLine("Hello World!");
      {
        var summary = BenchmarkRunner.Run<ArrayPool_T_BenchMark>();

        Console.ReadKey();
      }
    }
  }
}
