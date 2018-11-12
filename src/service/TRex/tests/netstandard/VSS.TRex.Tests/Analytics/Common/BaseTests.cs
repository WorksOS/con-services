using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Analytics.Common
{
  public class BaseTests : IClassFixture<DILoggingFixture>
  {
    protected const double CELL_SIZE = 0.34;
    protected const int CELLS_OVER_TARGET = 25;
    protected const int CELLS_AT_TARGET = 45;
    protected const int CELLS_UNDER_TARGET = 85;

    protected long[] CountsArray = {10, 5, 45, 30, 15, 25, 55};
  }
}
