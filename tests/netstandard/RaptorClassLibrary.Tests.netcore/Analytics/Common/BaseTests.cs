using VSS.TRex.Tests.netcore.TestFixtures;
using Xunit;

namespace RaptorClassLibrary.Tests.netcore.Analytics.Common
{
  public class BaseTests : IClassFixture<DILoggingFixture>
  {
    protected const double CELL_SIZE = 0.34;
    protected const int CELLS_OVER_TARGET = 25;
    protected const int CELLS_AT_TARGET = 45;
    protected const int CELLS_UNDER_TARGET = 85;
    protected const double TOLERANCE = 0.00001;

  }
}
