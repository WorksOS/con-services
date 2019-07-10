using Serilog;
using VSS.Serilog.Extensions;

namespace IntegrationTests.WebApiTests
{
  public class WebApiTestsBase
  {
    public WebApiTestsBase()
    {
      Log.Logger = SerilogExtensions.Configure("IntegrationTests.WebApiTests.log");
    }
  }
}
