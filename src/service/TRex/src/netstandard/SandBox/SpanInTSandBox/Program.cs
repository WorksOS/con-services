using System;
using System.Diagnostics;

namespace SpanInTSandBox
{
  class Program
  {
    static Guid g = Guid.NewGuid();

    static void UseSpan()
    {
      Span<byte> span = stackalloc byte[16];
      Guid g = Guid.NewGuid();
      g.TryWriteBytes(span);
    }

    static void UseByteArray()
    {
      Guid g = Guid.NewGuid();
      var b = g.ToByteArray();
    }

    static void Main(string[] args)
    {
      Span<byte> span = stackalloc byte[16];

      var sw = Stopwatch.StartNew();
      for (int i = 0; i < 1_000_000_000; i++)
      {
        byte[] b = new byte[16];
      }

      Console.WriteLine($"new byte[16], 1,000,000,000 times: {sw.Elapsed}");

      sw = Stopwatch.StartNew();
      for (int i = 0; i < 1_000_000_000; i++)
      {
        byte[] b = new byte[17];
      }

      Console.WriteLine($"new byte[17], 1,000,000,000 times: {sw.Elapsed}");

      sw = Stopwatch.StartNew();
      for (int i = 0; i < 1_000_000_000; i++)
      {
        UseSpan();
      }

      Console.WriteLine($"Guid to span function, 1,000,000,000 times: {sw.Elapsed}");

      sw = Stopwatch.StartNew();
      for (int i = 0; i < 1_000_000_000; i++)
      {
        Guid g = Guid.NewGuid();
        g.TryWriteBytes(span);
      }

      Console.WriteLine($"Guid to span direct, 1,000,000,000 times: {sw.Elapsed}");

      sw = Stopwatch.StartNew();
      for (int i = 0; i < 1_000_000_000; i++)
      {
        UseByteArray();
      }

      Console.WriteLine($"Guid to byte[], 1,000,000,000 times: {sw.Elapsed}");
      Console.ReadKey();
    }
  }
}
