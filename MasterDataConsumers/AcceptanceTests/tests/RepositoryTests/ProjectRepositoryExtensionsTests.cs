using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repo.Extensions;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectRepositoryExtensionsTests
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
  }
}