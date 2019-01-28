using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using VSS.TRex.Common.Utilities;

namespace GuidHashCodeBenchmark
{
  public class GuidHashCodePerfTest
  {
    private Guid guid;

    [GlobalSetup]
    public void GlobalSetup()
    {
      guid = Guid.NewGuid();
    }

    [Benchmark]
    public void GuidHashCode_UsingByteArrays()
    {
      int hash = GuidHashCode.Hash_Old(guid);
    }

    [Benchmark]
    public void GuidHashCode_UsingLessByteArrays_Safe()
    {
      int hash = GuidHashCode.Hash(guid);
    }

    /*
    [Benchmark]
    public void GuidHashCode_UsingLessByteArrays_UnSafe()
    {
      int hash = GuidHashCode.HashExUnsafe(guid);
    }
    */

    public static int HashWithSpan(Guid g)
    {
      Span<byte> bytes = stackalloc byte[16];
      g.TryWriteBytes(bytes);

      byte[] b = new [] {
        bytes[6],
        bytes[7],
        bytes[4],
        bytes[5],
        bytes[0],
        bytes[1],
        bytes[2],
        bytes[3],
        bytes[15],
        bytes[14],
        bytes[13],
        bytes[12],
        bytes[11],
        bytes[10],
        bytes[9],
        bytes[8]
      };

      long hilo = BitConverter.ToInt64(b, 0) ^ BitConverter.ToInt64(b, 8);
      return (int)(hilo >> 32) ^ (int)hilo;
    }

    [Benchmark]
    public void GuidHashCode_UsingSpanByte()
    {
      int hash = HashWithSpan(guid);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
    }
  }

  class Program
  {
    static void Main(string[] args)
    {
      var summary = BenchmarkRunner.Run<GuidHashCodePerfTest>();

      Console.ReadKey();
    }
  }
}
