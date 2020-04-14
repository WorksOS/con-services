using VSS.MasterData.Repositories.Extensions;
using Xunit;

namespace RepositoryTests.ProjectRepositoryTests
{
  public class ProjectExtensionsTests
  {
    [Fact]
    public void Integer_CalculateUpsertCount1()
    {
      Assert.Equal(0, 0.CalculateUpsertCount());
    }
    [Fact]
    public void Integer_CalculateUpsertCount2()
    {
      Assert.Equal(1, 1.CalculateUpsertCount());
    }
    [Fact]
    public void Integer_CalculateUpsertCount3()
    {
      Assert.Equal(1, 2.CalculateUpsertCount());
    }
    [Fact]
    public void Integer_CalculateUpsertCount4()
    {
      Assert.Equal(4, 4.CalculateUpsertCount());
    }
  }
}
