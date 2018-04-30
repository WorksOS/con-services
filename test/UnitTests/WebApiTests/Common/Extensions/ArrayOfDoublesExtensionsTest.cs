using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Common.Extensions;

namespace VSS.Productivity3D.WebApiTests.Common.Extensions
{
  [TestClass]
  public class ArrayOfDoublesExtensionsTest
  {
    [TestMethod]
    public void ShouldRemoveDuplicateAndSorByAscendingOrder()
    {
      double[] sourceArray = { 0.0, 1.2, -0.5, -2.0, 2.1, -1.0, 0.5, -2.0, 1.2 };
      var updatedArray = sourceArray.AddZeroDistinctSortBy();

      Assert.IsFalse(sourceArray.Length == updatedArray.Length, "Length of the source and updated arrays should not be the same");
      Assert.IsTrue(sourceArray.Length == updatedArray.Length + 2, "Length of the updated array is incorrect");
      double TOLERANCE = 0.001;
      Assert.IsTrue(Math.Abs(updatedArray[0] + 2.0) < TOLERANCE, "The 1st element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[1] + 1.0) < TOLERANCE, "The 2nd element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[2] + 0.5) < TOLERANCE, "The 3rd element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[3] - 0.0) < TOLERANCE, "The 4th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[4] - 0.5) < TOLERANCE, "The 5th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[5] - 1.2) < TOLERANCE, "The 6th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[6] - 2.1) < TOLERANCE, "The 7th element of the updated array is incorrect");
    }

    [TestMethod]
    public void ShouldRemoveDuplicateAndSorByDescendingOrder()
    {
      double[] sourceArray = { 0.0, 1.2, -0.5, -2.0, 2.1, -1.0, 0.5, -2.0, 1.2 };
      var updatedArray = sourceArray.AddZeroDistinctSortBy(false);

      Assert.IsFalse(sourceArray.Length == updatedArray.Length, "Length of the source and updated arrays should not be the same");
      Assert.IsTrue(sourceArray.Length == updatedArray.Length + 2, "Length of the updated array is incorrect");
      double TOLERANCE = 0.001;
      Assert.IsTrue(Math.Abs(updatedArray[0] - 2.1) < TOLERANCE, "The 1st element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[1] - 1.2) < TOLERANCE, "The 2nd element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[2] - 0.5) < TOLERANCE, "The 3rd element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[3] - 0.0) < TOLERANCE, "The 4th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[4] + 0.5) < TOLERANCE, "The 5th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[5] + 1.0) < TOLERANCE, "The 6th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[6] + 2.0) < TOLERANCE, "The 7th element of the updated array is incorrect");
    }

    [TestMethod]
    public void ShouldRemoveDuplicateAddMissingZeroAndSorByAscendingOrder()
    {
      double[] sourceArray = { 1.2, -0.5, -2.0, 2.1, -1.0, 0.5, -2.0, 1.2 };
      var updatedArray = sourceArray.AddZeroDistinctSortBy();

      Assert.IsFalse(sourceArray.Length == updatedArray.Length, "Length of the source and updated arrays should not be the same");
      Assert.IsTrue(sourceArray.Length == updatedArray.Length + 1, "Length of the updated array is incorrect");
      double TOLERANCE = 0.001;
      Assert.IsTrue(Math.Abs(updatedArray[0] + 2.0) < TOLERANCE, "The 1st element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[1] + 1.0) < TOLERANCE, "The 2nd element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[2] + 0.5) < TOLERANCE, "The 3rd element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[3] - 0.0) < TOLERANCE, "The 4th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[4] - 0.5) < TOLERANCE, "The 5th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[5] - 1.2) < TOLERANCE, "The 6th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[6] - 2.1) < TOLERANCE, "The 7th element of the updated array is incorrect");
    }

    [TestMethod]
    public void ShouldRemoveDuplicateAddMissingZeroAndSorByDescendingOrder()
    {
      double[] sourceArray = { 1.2, -0.5, -2.0, 2.1, -1.0, 0.5, -2.0, 1.2 };
      var updatedArray = sourceArray.AddZeroDistinctSortBy(false);

      Assert.IsFalse(sourceArray.Length == updatedArray.Length, "Length of the source and updated arrays should not be the same");
      Assert.IsTrue(sourceArray.Length == updatedArray.Length + 1, "Length of the updated array is incorrect");
      double TOLERANCE = 0.001;
      Assert.IsTrue(Math.Abs(updatedArray[0] - 2.1) < TOLERANCE, "The 1st element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[1] - 1.2) < TOLERANCE, "The 2nd element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[2] - 0.5) < TOLERANCE, "The 3rd element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[3] - 0.0) < TOLERANCE, "The 4th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[4] + 0.5) < TOLERANCE, "The 5th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[5] + 1.0) < TOLERANCE, "The 6th element of the updated array is incorrect");
      Assert.IsTrue(Math.Abs(updatedArray[6] + 2.0) < TOLERANCE, "The 7th element of the updated array is incorrect");
    }
  }
}
