using System;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.DI;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Analytics.Foundation
{
  public class BaseAnalyticsCoordinatorTests : IClassFixture<DILoggingFixture>
  {
    /// <summary>
    /// Note: This uses a pass count coordinator as a proxy to test the base coordinator exception handling
    /// </summary>
    [Fact]
    public void Execute_ExceptionHandling()
    {
      var coordinator = new PassCountStatisticsCoordinator();

      // This will cause a null reference exception due to the absence of the ISiteModels resource in the DIContext
      Func<Task<PassCountStatisticsResponse>> act = async () => await coordinator.ExecuteAsync(new PassCountStatisticsArgument());

      if (DIContext.DefaultIsRequired)
        act.Should().Throw<InvalidOperationException>().WithMessage("No service for type 'VSS.TRex.SiteModels.Interfaces.ISiteModels' has been registered.");
      else
        act.Should().Throw<NullReferenceException>();
    }
  }
}
