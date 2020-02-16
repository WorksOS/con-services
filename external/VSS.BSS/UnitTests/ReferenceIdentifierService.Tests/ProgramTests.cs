using Microsoft.VisualStudio.TestTools.UnitTesting;
using Topshelf.Runtime;
using Topshelf.Runtime.Windows;
using VSS.Nighthawk.ReferenceIdentifierService.Service;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests
{
  [TestClass]
  public class ProgramTests
  {
    [TestMethod]
    public void ProgramTests_ServiceFactoryReturnsValidRestServiceTest()
    {
      HostSettings settings = new WindowsHostSettings("UnitTest", "UnitTest");
      RestService restService = Program.ServiceFactory(settings);
      //Start might throw an exception if port 8004 is not enabled
      try
      {
        restService.Start();
      }
      catch
      {
      }
      //make sure stop doesn't throw an error
      restService.Stop();
      //make sure Error doesn't throw an error
      restService.Error();
    }
  }
}
