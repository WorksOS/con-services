using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories.Extensions;

namespace RepositoryTests.ProjectRepositoryTests
{
  [TestClass]
  public class ProjectExtensionsTests
  {
    [TestMethod]
    public void Integer_CalculateUpsertCount1()
    {
      Assert.AreEqual(0, 0.CalculateUpsertCount());
    }
    [TestMethod]
    public void Integer_CalculateUpsertCount2()
    {
      Assert.AreEqual(1, 1.CalculateUpsertCount());
    }
    [TestMethod]
    public void Integer_CalculateUpsertCount3()
    {
      Assert.AreEqual(1, 2.CalculateUpsertCount());
    }
    [TestMethod]
    public void Integer_CalculateUpsertCount4()
    {
      Assert.AreEqual(4, 4.CalculateUpsertCount());
    }
  }
}