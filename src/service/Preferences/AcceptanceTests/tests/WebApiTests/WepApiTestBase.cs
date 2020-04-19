using Serilog;
using VSS.Serilog.Extensions;

namespace WebApiTests
{
  public class WebApiTestsBase
  {
    public WebApiTestsBase()
    {
      Log.Logger = SerilogExtensions.Configure("CCSS.Productivity3D.Preferences.AcceptanceTests.WebApiTests.log");
    }
  }
}
