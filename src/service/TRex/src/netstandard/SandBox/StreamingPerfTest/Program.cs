using System;
using System.IO;

namespace StreamingPerfTest
{
  class Program
  {
    private static void TestIntStreaming()
    {
      const int iterations = 1000;
      const int numberCount = 16500000;

      byte[] bytes = null;

      // construct buffer 
      using (MemoryStream ms = new MemoryStream())
      {
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
          for (int i = 0; i < numberCount; i++)
            bw.Write(i);
          bytes = ms.ToArray();
        }
      }

      // Read in the ints using a memory stream

      DateTime _start1 = DateTime.Now;

      long sum = 0;
      for (int iterationCount = 0;
        iterationCount < iterations;
        iterationCount++)
      {
        sum = 0;

        using (MemoryStream ms = new MemoryStream(bytes))
        {
          using (BinaryReader br = new BinaryReader(ms))
          {
            for (int i = 0; i < numberCount; i++)
              sum += br.ReadInt32();
          }
        }
      }

      TimeSpan ts1 = DateTime.Now - _start1;

      // Read in the ints using a memory stream
      DateTime _start2 = DateTime.Now;

      long sum2 = 0;
      for (int iterationCount = 0;
        iterationCount < iterations;
        iterationCount++)
      {
        sum2 = 0;
        int BufPos = 0;

        for (int i = 0; i < numberCount; i++)
        {
          sum2 += bytes[BufPos] | bytes[BufPos + 1] << 8 | bytes[BufPos + 2] << 16 | bytes[BufPos + 3] << 24;
          BufPos += 4;
        }
      }

      TimeSpan ts2 = DateTime.Now - _start2;

      Console.WriteLine($"Sum over {iterations} iteration = [BinaryReader]:{sum} (in {ts1}) vs [BufferRead]:{sum2} (in {ts2})");
    }

    private static unsafe void TestDoubleStreaming()
    {
      const int iterations = 1; //100;
      const int numberCount = 7500000;

      byte[] bytes = null;

      // construct buffer 
      using (MemoryStream ms = new MemoryStream())
      {
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
          for (int i = 0; i < numberCount; i++)
            bw.Write(1.0 * i);
          bytes = ms.ToArray();
        }
      }

      // Read in the ints using a memory stream

      DateTime _start1 = DateTime.Now;

      double sum = 0;
      for (int iterationCount = 0;
        iterationCount < iterations;
        iterationCount++)
      {
        sum = 0;

        using (MemoryStream ms = new MemoryStream(bytes))
        {
          using (BinaryReader br = new BinaryReader(ms))
          {
            for (int i = 0; i < numberCount; i++)
              sum += br.ReadDouble();
          }
        }
      }

      TimeSpan ts1 = DateTime.Now - _start1;

      // Read in the ints using a memory stream
      DateTime _start2 = DateTime.Now;

      double sum2 = 0;
      for (int iterationCount = 0;
        iterationCount < iterations;
        iterationCount++)
      {
        sum2 = 0;
        int BufPos = 0;

        /*
         * for (int i = 0; i < numberCount; i++)
                {
                  uint lo = (uint) (bytes[BufPos] | bytes[BufPos + 1] << 8 | bytes[BufPos + 2] << 16 | bytes[BufPos + 3] << 24);
                  uint hi = (uint) (bytes[BufPos + 4] | bytes[BufPos + 5] << 8 | bytes[BufPos + 6] << 16 | bytes[BufPos + 7] << 24);

                  ulong tmpBuffer = ((ulong) hi) << 32 | lo;

                  double result = *((double*) &tmpBuffer);

                  sum += result;
                  BufPos += 8;
                }
                */

        for (int i = 0; i < numberCount; i++)
        {
          uint lo = (uint)(bytes[BufPos] | bytes[BufPos + 1] << 8 | bytes[BufPos + 2] << 16 | bytes[BufPos + 3] << 24);
          uint hi = (uint)(bytes[BufPos + 4] | bytes[BufPos + 5] << 8 | bytes[BufPos + 6] << 16 | bytes[BufPos + 7] << 24);

          ulong tmpBuffer = ((ulong)hi) << 32 | lo;
          double result = *((double*)&tmpBuffer);

          sum += result;
          BufPos += 8;
        }
      }

      TimeSpan ts2 = DateTime.Now - _start2;

      Console.WriteLine($"Sum over {iterations} iteration = [BinaryReader]:{sum} (in {ts1}) vs [BufferRead]:{sum2} (in {ts2})");
    }

    public static void Main(string[] args)
    {
      //TestIntStreaming();
      TestDoubleStreaming();
      Console.ReadKey();
    }
  }
}
