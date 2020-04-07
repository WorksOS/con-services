using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
  public class SubGridUtilitiesTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_SubGridDimensionalIterator_ActionFunctor()
    {
      // Ensure the iterator covers all the cells in a sub grid
      int counter = 0;

      SubGridUtilities.SubGridDimensionalIterator((x, y) => counter++);
      Assert.Equal(SubGridTreeConsts.SubGridTreeCellsPerSubGrid, counter);
    }

    [Fact]
    public void Test_SubGridDimensionalIterator_FunctionDelegate()
    {
      // Ensure the iterator covers all the cells in a sub grid
      int counter = 0;

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        counter++;
        return true;
      });
      Assert.Equal(SubGridTreeConsts.SubGridTreeCellsPerSubGrid, counter);

      // Ensure the iterator covers all the cells in a sub grid until a false return result
      counter = 0;

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        counter++;
        return false;
      });
      Assert.Equal(1, counter);
    }
  }
}
