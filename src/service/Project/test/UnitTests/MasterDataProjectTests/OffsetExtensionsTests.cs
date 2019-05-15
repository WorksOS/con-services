using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Project.Abstractions.Extensions;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class OffsetExtensionsTests
  {
    [TestMethod]
    [DataRow(null, null)]
    [DataRow(null, 0.0)]
    [DataRow(0.0, null)]
    [DataRow(0.0, 0.0)]
    [DataRow(0.1, 0.1)]
    [DataRow(1.2349, 1.2341)]
    public void NullableOffsetsShouldBeEqual(double? offset1, double? offset2)
    {
      Assert.IsTrue(offset1.EqualsToNearestMillimeter(offset2));
    }

    [TestMethod]
    [DataRow(null, 1.0)]
    [DataRow(1.0, null)]
    [DataRow(0.0, 1.0)]
    [DataRow(1.234, 1.235)]
    public void NullableOffsetsShouldBeNotEqual(double? offset1, double? offset2)
    {
      Assert.IsFalse(offset1.EqualsToNearestMillimeter(offset2));
    }

    [TestMethod]
    [DataRow(0.0, 0.0)]
    [DataRow(0.1, 0.1)]
    [DataRow(1.2349, 1.2341)]
    public void OffsetsShouldBeEqual(double offset1, double offset2)
    {
      Assert.IsTrue(offset1.EqualsToNearestMillimeter(offset2));
    }

    [TestMethod]
    [DataRow(0.0, 1.0)]
    [DataRow(1.234, 1.235)]
    public void OffsetsShouldBeNotEqual(double offset1, double offset2)
    {
      Assert.IsFalse(offset1.EqualsToNearestMillimeter(offset2));
    }
  }
}
