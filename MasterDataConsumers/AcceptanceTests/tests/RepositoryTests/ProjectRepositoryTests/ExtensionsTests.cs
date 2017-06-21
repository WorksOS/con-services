using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repo.Extensions;

namespace RepositoryTests.ProjectRepositoryTests
{
  [TestClass]
  public class ExtensionsTests
    {
      [TestMethod]
      [DataRow(0, 0)]
      [DataRow(1, 1)]
      [DataRow(2, 1)]
      [DataRow(4, 4)]
      public void Integer_CalculateUpsertCount(int upsertCount, int expected)
      {
        Assert.AreEqual(expected, upsertCount.CalculateUpsertCount());
      }

      [TestMethod]
      public void Integer_CalculateUpsertCount2()
      {
        Assert.AreEqual(4, 4.CalculateUpsertCount());
      }
  }
}