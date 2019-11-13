using Hangfire.Dashboard;

namespace VSS.Productivity3D.Scheduler.WebApi
{
  public partial class Startup
  {
    private class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
      private readonly string[] _roles;

      public HangfireAuthorizationFilter(params string[] roles)
      {
        _roles = roles;
      }

      public bool Authorize(DashboardContext context)
      {
        return true;
      }
    }
  }
}
