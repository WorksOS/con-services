using Serilog;
using VSS.Serilog.Extensions;

namespace IntegrationTests.WebApiTests
{
  public class WebApiTestsBase
  {
    public WebApiTestsBase()
    {
      if (Log.Logger == null) { Log.Logger = SerilogExtensions.Configure("IntegrationTests.WebApiTests.log", null); }
    }
  }
}
