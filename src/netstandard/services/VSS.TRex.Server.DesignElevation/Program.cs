using System;
using VSS.TRex.Designs.Servers.Client;
using System.Threading;
using VSS.TRex.DI;

namespace VSS.TRex.Server.DesignElevation
{
  class Program
  {
    private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

    private static void DependencyInjection()
    {
      DIBuilder.New().AddLogging().Complete();
    }

    static void Main(string[] args)
    {
      DependencyInjection();

      var server = new CalculateDesignElevationsServer();
      Console.CancelKeyPress += (s, a) =>
      {
        Console.WriteLine("Exiting");
        WaitHandle.Set();
      };

      WaitHandle.WaitOne();
    }
  }
}
