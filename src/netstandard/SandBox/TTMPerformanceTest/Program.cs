using System;
using System.Collections.Generic;

namespace VSS.TRex.Sandbox.TTMPerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
          for (int i = 0; i < 20; i++)
          {
            Designs.TTM.Optimised.TrimbleTINModel readonly_tin = new Designs.TTM.Optimised.TrimbleTINModel();

            DateTime _start = DateTime.Now;
            readonly_tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");
            DateTime _end = DateTime.Now;
            Console.WriteLine($"Readonly tin read in {_end - _start}");
          }

          for (int i = 0; i < 0; i++)
          {
            Designs.TTM.TrimbleTINModel readwrite_tin = new Designs.TTM.TrimbleTINModel();
            DateTime _start = DateTime.Now;
            readwrite_tin.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");
            DateTime _end = DateTime.Now;
            Console.WriteLine($"Read/write tin read in {_end - _start}");
          }

//          Console.ReadKey();
        }
  }
}
